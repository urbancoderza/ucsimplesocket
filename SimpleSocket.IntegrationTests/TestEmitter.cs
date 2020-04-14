using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SimpleSocket.IntegrationTests
{
	internal class TestEmitter : IDatagramEmitter
	{
		public void DatagramReceived(Connection sourceConnection, Datagram datagram, DateTime receivedTime)
		{
			NumReceived++;
			LastDatagram = datagram;
			LastConnection = sourceConnection;
			LastReceivedDateTime = receivedTime;
		}

		public int NumReceived { get; private set; }

		public Datagram LastDatagram { get; private set; }

		public Connection LastConnection { get; private set; }

		public DateTime LastReceivedDateTime { get; private set; }
	}
}
