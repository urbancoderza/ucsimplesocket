using System;
using UCSimpleSocket;

namespace UCSimpleSocket.Emitters
{
	/// <summary>
	/// A class representing a datagram emitter that queues the emitted datagrams and can be peeked and dequeued.
	/// </summary>
	public class ActionEmitter : IDatagramEmitter
	{
		private readonly Action<Connection, byte[], DateTime> _action;

		/// <summary>
		/// Initializes a new instance of the <see cref="ActionEmitter"/> class.
		/// </summary>
		/// <param name="action">The action to invoke when a <see cref="Datagram"/> is received.</param>
		public ActionEmitter(Action<Connection, byte[], DateTime> action)
		{
			_action = action;
		}

		/// <summary>
		/// Called when a <see cref="Datagram"/> is received from the remote endpoint.
		/// </summary>
		/// <param name="source">The <see cref="Connection"/> which received the datagram.</param>
		/// <param name="datagram">The <see cref="Datagram"/> that was received.</param>
		/// <param name="receivedTime">The exact date and time the connection received the datagram.</param>
		public void DatagramReceived(Connection source, byte[] datagram, DateTime receivedTime)
		{
			_action?.Invoke(source, datagram, receivedTime);
		}		
	}
}
