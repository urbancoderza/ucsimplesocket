using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UCSimpleSocket.IntegrationTests
{
	internal class TestEmitter : IDatagramEmitter
	{
		public void DatagramReceived(Connection sourceConnection, byte[] datagram, DateTime receivedTime)
		{
			NumReceived++;
			LastDatagram = datagram;
			LastConnection = sourceConnection;
			LastReceivedDateTime = receivedTime;

			All.Add(new Tuple<byte[], Connection, DateTime>(datagram, sourceConnection, receivedTime));
		}

		public int NumReceived { get; private set; }

		public byte[] LastDatagram { get; private set; }

		public Connection LastConnection { get; private set; }

		public DateTime LastReceivedDateTime { get; private set; }

		internal void Reset()
		{
			NumReceived = 0;
			LastDatagram = null;
			LastConnection = null;
		}

		public List<Tuple<byte[], Connection, DateTime>> All { get; } = new List<Tuple<byte[], Connection, DateTime>>();
	}
}
