using System;
using System.Net.Sockets;

namespace SimpleSocket
{
	internal sealed class ConnectionFaultedEventArgs : EventArgs
	{
		public SocketException Exception { get; }

		public ConnectionFaultedEventArgs(SocketException ex)
		{
			Exception = ex;
		}
	}
}