using System;
using System.Net.Sockets;

namespace UCSimpleSocket
{
	internal sealed class ConnectionFaultedEventArgs : EventArgs
	{
		public Exception Exception { get; }

		public ConnectionFaultedEventArgs(Exception ex)
		{
			Exception = ex;
		}
	}
}