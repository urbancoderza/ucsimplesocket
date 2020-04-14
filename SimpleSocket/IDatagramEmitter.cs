using System;

namespace UCSimpleSocket
{
	/// <summary>
	/// An interface that rerpesents a datagram emitter to use for received datagrams.
	/// </summary>
	public interface IDatagramEmitter
	{
		/// <summary>
		/// Called when a <see cref="Datagram"/> is received from the remote endpoint.
		/// </summary>
		/// <param name="source">The <see cref="Connection"/> which received the datagram.</param>
		/// <param name="datagram">The <see cref="Datagram"/> that was received.</param>
		/// <param name="receivedTime">The exact date and time the connection received the datagram.</param>
		void DatagramReceived(Connection source, byte[] datagram, DateTime receivedTime);
	}
}
