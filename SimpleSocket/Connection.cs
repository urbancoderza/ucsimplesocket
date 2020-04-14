using Microsoft.Extensions.Logging;
using MsgPack;
using MsgPack.Serialization;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSocket
{
	/// <summary>
	/// A class representing a TCP connection between a local IP endpoint and a remote IP endpoint.
	/// </summary>
	public sealed partial class Connection : IDisposable
	{
		private const int CancellationTokenSourceDelayMs = 2000;

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
		private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource(CancellationTokenSourceDelayMs);

		/// <summary>
		/// Initialized a new instance of the <see cref="Connection"/> class.
		/// </summary>
		/// <param name="clientSocket">The <see cref="TcpClient"/> to use as the underlying connection.</param>
		/// <param name="datagramEmitter">A <see cref="IDatagramEmitter"/> that is used to emit received datagrams.</param>
		/// <param name="logger">The <see cref="ILogger{TCategoryName}"/> to use for logging information.</param>
		public Connection(TcpClient clientSocket, IDatagramEmitter datagramEmitter = null, ILogger<Connection> logger = null)
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

			_stream = clientSocket.GetStream();
			_unpacker = Unpacker.Create(_stream, false);
			_packer = Packer.Create(_stream, false);
			_datagramEmitter = datagramEmitter;

			if (_datagramEmitter != null)
			{
				_receiveWorker = new Task((t) => Receive(_cancelTokenSource.Token), _cancelTokenSource.Token, TaskCreationOptions.LongRunning);
				_receiveWorker.Start();
			}
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
			if (datagram == null)
				return;
			_logger?.LogInformation("Sending datagram:{0}{1}", Environment.NewLine, datagram.ToString(1));

			var data = datagram.Copy();

			try
			{
				if (IsFaulted)
					return;
				if (IsFaulted)
					return;

				await _packer.PackAsync(data, _serialContext).ConfigureAwait(false);
			}
			catch (SocketException sexc)
			{
				IsFaulted = true;
				_logger?.LogError(sexc, "Socket fault while sending data");
				OnConnectionFaulted(sexc);
				throw;
			}
		}

		private void OnConnectionFaulted(SocketException ex)
		{
			ConnectionFaulted?.Invoke(this, new ConnectionFaultedEventArgs(ex));
		}

		private async void Receive(CancellationToken cancelToken)
		{
			try
			{
				while (_disposed == 0 && !IsFaulted && !cancelToken.IsCancellationRequested)
				{
					var nextMsg = await _unpacker.UnpackAsync<Datagram>(_serialContext, cancelToken).ConfigureAwait(false);
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
						throw;
					}
				}
			}
			catch (SocketException sexc)
			{
				IsFaulted = true;
				_logger?.LogError(sexc, "Socket fault while receiving data");
				OnConnectionFaulted(sexc);
				throw;
			}
			catch (AggregateException ex)// when (ex.InnerExceptions.Any(p => p is TaskCanceledException))
			{
				_logger?.LogError(ex, "Socket fault while receiving data");
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Socket fault while receiving data");
				throw;
			}
		}
	}
}
