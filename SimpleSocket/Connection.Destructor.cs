using System;
using System.Threading;

namespace UCSimpleSocket
{
	public sealed partial class Connection
	{
		private volatile int _disposed;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
			{
				if (_cancelTokenSource != null)
					_cancelTokenSource.Cancel(true);

				if (_receiveWorker != null)
				{
					while (!_receiveWorker.IsCompleted) ;
					_receiveWorker.Dispose();
				}

				if (_packer != null)
					_packer.Dispose();
				if (_unpacker != null)
					_unpacker.Dispose();

				if (_ownsTcpClient)
				{
					if (_stream != null)
					{
						_stream.Close();
						_stream.Dispose();
					}
					if (_clientSocket != null)
					{
						_clientSocket.Close();
						_clientSocket.Dispose();
					}
				}

				if (_cancelTokenSource != null)
					_cancelTokenSource.Dispose();
			}

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Default destructor.
		/// </summary>
		~Connection()
		{
			Dispose();
		}
	}
}
