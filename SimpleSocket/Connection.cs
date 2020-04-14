using Microsoft.Extensions.Logging;
using MsgPack;
using MsgPack.Serialization;
using SimpleSocket.Emitters;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSocket
{
	/// <summary>
	/// A class representing a TCP connection between a local IP endpoint and a remote IP endpoint.
	/// </summary>
	public sealed partial class Connection : IDisposable
	{
		/// <summary>
		/// An event that is raised when the underlying connection goes into a faulted state.
		/// </summary>
		public event EventHandler<EventArgs> ConnectionFaulted;

		private readonly SerializationContext _serialContext = new SerializationContext(
			PackerCompatibilityOptions.PackBinaryAsRaw |
			PackerCompatibilityOptions.ProhibitExtendedTypeObjects);

		private readonly ILogger<Connection> _logger;
		private readonly NetworkStream _stream;
		private readonly Task _receiveWorker;
		private readonly Unpacker _unpacker;
		private readonly Packer _packer;
		private readonly IDatagramEmitter _datagramEmitter;
		private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
		private readonly bool _ownsTcpClient;
		private readonly TcpClient _clientSocket;

		/// <summary>
		/// Initialized a new instance of the <see cref="Connection"/> class.
		/// </summary>
		/// <param name="emitAction">An <see cref="Action{Connection, Datagram, DateTime}"/> to invoke when a <see cref="Datagram"/> is received.</param>
		/// <param name="clientSocket">The <see cref="TcpClient"/> to use as the underlying connection.</param>
		/// <param name="ownsTcpClient">A <see cref="bool"/> indicating whether the <paramref name="clientSocket"/> can be disposed when this instance is disposed or not.</param>
		/// <param name="logger">The <see cref="ILogger{TCategoryName}"/> to use for logging information.</param>
		public Connection(Action<Connection, Datagram, DateTime> emitAction, TcpClient clientSocket, bool ownsTcpClient = false, ILogger<Connection> logger = null) : this(clientSocket, ownsTcpClient, new ActionEmitter(emitAction), logger)
		{
		}

		/// <summary>
		/// Initialized a new instance of the <see cref="Connection"/> class.
		/// </summary>
		/// <param name="clientSocket">The <see cref="TcpClient"/> to use as the underlying connection.</param>
		/// <param name="ownsTcpClient">A <see cref="bool"/> indicating whether the <paramref name="clientSocket"/> can be disposed when this instance is disposed or not.</param>
		/// <param name="datagramEmitter">A <see cref="IDatagramEmitter"/> that is used to emit received datagrams.</param>
		/// <param name="logger">The <see cref="ILogger{TCategoryName}"/> to use for logging information.</param>
		public Connection(TcpClient clientSocket, bool ownsTcpClient = false, IDatagramEmitter datagramEmitter = null, ILogger<Connection> logger = null)
		{
			_logger = logger;

			if (clientSocket == null || !clientSocket.Connected)
				throw new ArgumentException("The supplied client is null or not connected", nameof(clientSocket));

			var client = clientSocket.Client;
			var localEndpoint = client.LocalEndPoint as IPEndPoint;
			var remoteEndpoint = client.RemoteEndPoint as IPEndPoint;
			Name = $"{localEndpoint} : {remoteEndpoint}";
			LocalEndPoint = localEndpoint;
			RemoteEndPoint = remoteEndpoint;
			_ownsTcpClient = ownsTcpClient;

			_stream = clientSocket.GetStream();
			_unpacker = Unpacker.Create(_stream, false);
			_packer = Packer.Create(_stream, false);
			_datagramEmitter = datagramEmitter;

			if (_datagramEmitter != null)
			{
				_receiveWorker = new Task((t) => Receive(_cancelTokenSource.Token), _cancelTokenSource.Token, TaskCreationOptions.LongRunning);
				_receiveWorker.Start();
			}

			if (ownsTcpClient)
				_clientSocket = clientSocket;
		}

		/// <summary>
		/// Gets a <see cref="bool"/> indicating whether the underlyiong socket is in a faulted state or not.
		/// </summary>
		public bool IsFaulted { get; private set; }

		/// <summary>
		/// Gets a <see cref="string"/> representing the name of this <see cref="Connection"/>. The name consists of the local and remote <see cref="IPEndPoint"/>.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The local <see cref="IPEndPoint"/> that this connection is using.
		/// </summary>
		public IPEndPoint LocalEndPoint { get; }

		/// <summary>
		/// The remote <see cref="IPEndPoint"/> that this connection is connected to.
		/// </summary>
		public IPEndPoint RemoteEndPoint { get; }

		/// <summary>
		/// Sends a datagram to the remote host.
		/// </summary>
		/// <param name="datagram">The <see cref="Datagram"/> to send.</param>
		public async Task SendDatagramAsync(Datagram datagram)
		{
			if (_disposed == 1)
				throw new ObjectDisposedException(nameof(Connection));

			if (datagram == null)
				return;

			var data = datagram.Copy();

			try
			{
				if (IsFaulted)
					return;

				_logger?.LogInformation("Sending datagram:{0}{1}", Environment.NewLine, datagram.ToString(1));
				await _packer.PackAsync(data, _serialContext).ConfigureAwait(false);
			}
			catch (TargetInvocationException tiex)
			{
				_logger?.LogError(tiex, "Socket fault while sending data");
				OnConnectionFaulted(tiex);
				throw;
			}
			catch (IOException ioex)
			{
				_logger?.LogError(ioex, "Socket fault while sending data");
				OnConnectionFaulted(ioex);
				throw;
			}
			catch (SocketException sexc)
			{
				_logger?.LogError(sexc, "Socket fault while sending data");
				OnConnectionFaulted(sexc);
				throw;
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error while sending data");
				throw;
			}
		}

		private void OnConnectionFaulted(Exception ex)
		{
			IsFaulted = true;
			ConnectionFaulted?.Invoke(this, new ConnectionFaultedEventArgs(ex));
		}

		private async void Receive(CancellationToken cancelToken)
		{
			try
			{
				while (_disposed == 0 && !IsFaulted && !cancelToken.IsCancellationRequested)
				{
					_ = await _unpacker.ReadAsync(cancelToken).ConfigureAwait(false);
					var nextMsg = await _unpacker.UnpackAsync<Datagram>(cancelToken).ConfigureAwait(false);

					var receivedTime = DateTime.Now;
					if (nextMsg == null)
						continue;

					try
					{
						_datagramEmitter?.DatagramReceived(this, nextMsg, receivedTime);
					}
					catch (Exception ex)
					{
						_logger?.LogError(ex, "Error emitting received datagram");
					}
				}
			}
			catch (TargetInvocationException tiex)
			{
				_logger?.LogError(tiex, "Socket fault while receiving data");
				OnConnectionFaulted(tiex);
			}
			catch (IOException ioex)
			{
				_logger?.LogError(ioex, "Socket fault while receiving data");
				OnConnectionFaulted(ioex);
			}
			catch (SocketException sexc)
			{
				_logger?.LogError(sexc, "Socket fault while receiving data");
				OnConnectionFaulted(sexc);
			}
			catch (TaskCanceledException)
			{
				_logger?.LogInformation("Receiving was stopped");
			}
			catch (OperationCanceledException)
			{
				_logger?.LogInformation("Receiving was stopped");
			}
			catch (AggregateException ex) when (ex.InnerExceptions.Any(p => p is OperationCanceledException))
			{
				_logger?.LogInformation("Receiving was stopped");
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error while receiving data");
			}
		}
	}
}
