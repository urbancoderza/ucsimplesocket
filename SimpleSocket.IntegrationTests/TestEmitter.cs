using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UCSimpleSocket.IntegrationTests
{
	internal class TestEmitter : IDatagramEmitter
	{
		public void DatagramReceived(Connection sourceConnection, Datagram datagram, DateTime receivedTime)
		{
			NumReceived++;
			LastDatagram = datagram;
			LastConnection = sourceConnection;
			LastReceivedDateTime = receivedTime;

			All.Add(new Tuple<Datagram, Connection, DateTime>(datagram, sourceConnection, receivedTime));
		}

		public int NumReceived { get; private set; }

		public Datagram LastDatagram { get; private set; }

		public Connection LastConnection { get; private set; }

		public DateTime LastReceivedDateTime { get; private set; }

		internal void Reset()
		{
			NumReceived = 0;
			LastDatagram = null;
			LastConnection = null;
		}

		public List<Tuple<Datagram, Connection, DateTime>> All { get; } = new List<Tuple<Datagram, Connection, DateTime>>();
	}
}
