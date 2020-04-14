using System;
using System.Collections.Concurrent;

namespace UCSimpleSocket.Emitters
{
	/// <summary>
	/// A class representing a datagram emitter that queues the emitted datagrams and can be peeked and dequeued.
	/// </summary>
	public class QueueEmitter : IDatagramEmitter
	{
		private readonly ConcurrentQueue<Tuple<Connection, byte[], DateTime>> _items = new ConcurrentQueue<Tuple<Connection, byte[], DateTime>>();

		/// <summary>
		/// Called when a datagram is received from the remote endpoint. Enqueues the received datagram.
		/// </summary>
		/// <param name="source">The <see cref="Connection"/> which received the datagram.</param>
		/// <param name="datagram">The datagram that was received.</param>
		/// <param name="receivedTime">The exact date and time the connection received the datagram.</param>
		public void DatagramReceived(Connection source, byte[] datagram, DateTime receivedTime)
		{
			_items.Enqueue(new Tuple<Connection, byte[], DateTime>(source, datagram, receivedTime));
		}

		/// <summary>
		/// Tries to return a datagram from the beginning of the queue without removing it.
		/// </summary>
		/// <param name="source">When this method returns, <paramref name="source"/> contains the connection that received the datagram, if a datagram exists.</param>
		/// <param name="datagram">When this method returns, <paramref name="datagram"/> contains the datagram, if it exists.</param>
		/// <param name="receivedTime">When this method returns, <paramref name="receivedTime"/> contains the date and time that the datagram was received, if a datagram exists.</param>
		/// <returns><code>true</code> if a datagram was returned successfully; otherwise, <code>false</code>.</returns>
		public bool TryPeek(out Connection source, out byte[] datagram, out DateTime? receivedTime)
		{
			source = null;
			datagram = null;
			receivedTime = null;

			var result = _items.TryPeek(out var tuple);
			if (result)
			{
				source = tuple.Item1;
				datagram = tuple.Item2;
				receivedTime = tuple.Item3;
			}

			return result;
		}

		/// <summary>
		/// Tries to return and remove a datagram from the beginning of the queue.
		/// </summary>
		/// <param name="source">When this method returns, <paramref name="source"/> contains the connection that received the datagram, if a datagram exists.</param>
		/// <param name="datagram">When this method returns, <paramref name="datagram"/> contains the datagram, if it exists.</param>
		/// <param name="receivedTime">When this method returns, <paramref name="receivedTime"/> contains the date and time that the datagram was received, if a datagram exists.</param>
		/// <returns><code>true</code> if a datagram was returned successfully; otherwise, <code>false</code>.</returns>
		public bool TryDequeue(out Connection source, out byte[] datagram, out DateTime? receivedTime)
		{
			source = null;
			datagram = null;
			receivedTime = null;

			var result = _items.TryDequeue(out var tuple);
			if (result)
			{
				source = tuple.Item1;
				datagram = tuple.Item2;
				receivedTime = tuple.Item3;
			}

			return result;
		}

		/// <summary>
		/// Gets the number of datagram objects that are available in the queue.
		/// </summary>
		public int Count => _items.Count;
	}
}
