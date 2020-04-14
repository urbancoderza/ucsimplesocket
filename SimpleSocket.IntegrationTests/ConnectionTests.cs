using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleSocket.IntegrationTests
{
	[TestClass]
	public class ConnectionTests
	{
		private Connection _con1;
		private Connection _con2;
		private readonly TestEmitter _emitter = new TestEmitter();
		private static readonly Random _rand = new Random();

		[TestInitialize]
		public void Setup()
		{
			var addressList = Dns.GetHostEntry("localhost").AddressList;
			var ipAddress = addressList.First(p => p.AddressFamily == AddressFamily.InterNetwork);
			var ipLocalEndPoint = new IPEndPoint(ipAddress, 7000);

			var listener = new TcpListener(ipLocalEndPoint);
			listener.Start();

			var client2 = new TcpClient(ipLocalEndPoint.AddressFamily);
			client2.ConnectAsync(ipLocalEndPoint.Address, ipLocalEndPoint.Port);

			var client1 = listener.AcceptTcpClient();

			_con1 = new Connection(client1, _emitter);
			_con2 = new Connection(client2, _emitter);
		}

		[TestCleanup]
		public void Cleanup()
		{
			if (_con2 != null)
				_con2.Dispose();
			if (_con1 != null)
				_con1.Dispose();
		}

		[TestMethod]
		public async void SendDatagram()
		{
			Assert.AreEqual(_emitter.NumReceived, 0);
			Assert.IsNull(_emitter.LastDatagram);
			Assert.IsNull(_emitter.LastConnection);

			await _con1.SendDatagramAsync(null);
			Thread.Sleep(1000);
			Assert.AreEqual(_emitter.NumReceived, 0);
			Assert.IsNull(_emitter.LastDatagram);
			Assert.IsNull(_emitter.LastConnection);

			var dg = new Datagram
			{
				CommandType = 1,
				Data = new byte[1234]
			};
			_rand.NextBytes(dg.Data);
			await _con1.SendDatagramAsync(dg);
			Thread.Sleep(6000);
			Assert.AreEqual(_emitter.NumReceived, 1);
			Assert.IsNotNull(_emitter.LastDatagram);
			Assert.IsNotNull(_emitter.LastConnection);
			Assert.AreEqual(_emitter.LastDatagram.CommandType, dg.CommandType);
			Assert.AreEqual(_emitter.LastDatagram.Data.Length, dg.Data.Length);
			Assert.AreEqual(_emitter.LastDatagram.Data.Length, dg.Data.Length);
			Assert.AreEqual(_emitter.LastDatagram.Data[0], dg.Data[0]);
			Assert.AreEqual(_emitter.LastDatagram.Data[^1], dg.Data[^1]);
		}
	}
}
