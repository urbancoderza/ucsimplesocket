﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static SimpleSocket.IntegrationTests.ConsoleLoggerFactory;

namespace SimpleSocket.IntegrationTests
{
	[TestClass]
	public class ConnectionTests
	{
		private Connection _con1;
		private Connection _con2;
		private TestEmitter _emitter;
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
			listener.Stop();
			listener = null;

			var con1Logger = new VSDebugLogger();
			var con2Logger = new VSDebugLogger();

			_emitter = new TestEmitter();

			_con1 = new Connection(client1, true, _emitter, con1Logger);
			con1Logger.Name = "con1";
			_con2 = new Connection(client2, datagramEmitter: _emitter, logger: con2Logger);
			con2Logger.Name = "con2";
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
		public void SendDatagram1()
		{
			Assert.AreEqual(_emitter.NumReceived, 0);
			Assert.IsNull(_emitter.LastDatagram);
			Assert.IsNull(_emitter.LastConnection);

			_ = _con1.SendDatagramAsync(null);
			Assert.AreEqual(_emitter.NumReceived, 0);
			Assert.IsNull(_emitter.LastDatagram);
			Assert.IsNull(_emitter.LastConnection);

			var dg = new Datagram
			{
				CommandType = 1,
				Data = new byte[1234]
			};
			_rand.NextBytes(dg.Data);
			_ = _con1.SendDatagramAsync(dg);
			Thread.Sleep(5000);
			Assert.AreEqual(_emitter.NumReceived, 1);
			Assert.IsNotNull(_emitter.LastDatagram);
			Assert.IsNotNull(_emitter.LastConnection);
			Assert.AreEqual(_emitter.LastDatagram.CommandType, dg.CommandType);
			Assert.AreEqual(_emitter.LastDatagram.Data.Length, dg.Data.Length);
			Assert.AreEqual(_emitter.LastDatagram.Data[0], dg.Data[0]);
			Assert.AreEqual(_emitter.LastDatagram.Data[^1], dg.Data[^1]);
			Assert.AreEqual(_emitter.LastConnection, _con2);
		}

		[TestMethod]
		public void SendDatagram2()
		{
			Assert.AreEqual(_emitter.NumReceived, 0);
			Assert.IsNull(_emitter.LastDatagram);
			Assert.IsNull(_emitter.LastConnection);

			_ = _con2.SendDatagramAsync(null);
			Assert.AreEqual(_emitter.NumReceived, 0);
			Assert.IsNull(_emitter.LastDatagram);
			Assert.IsNull(_emitter.LastConnection);

			var dg = new Datagram
			{
				CommandType = 1,
				Data = new byte[1234]
			};
			_rand.NextBytes(dg.Data);
			_ = _con2.SendDatagramAsync(dg);
			Thread.Sleep(5000);
			Assert.AreEqual(_emitter.NumReceived, 1);
			Assert.IsNotNull(_emitter.LastDatagram);
			Assert.IsNotNull(_emitter.LastConnection);
			Assert.AreEqual(_emitter.LastDatagram.CommandType, dg.CommandType);
			Assert.AreEqual(_emitter.LastDatagram.Data.Length, dg.Data.Length);
			Assert.AreEqual(_emitter.LastDatagram.Data[0], dg.Data[0]);
			Assert.AreEqual(_emitter.LastDatagram.Data[^1], dg.Data[^1]);
			Assert.AreEqual(_emitter.LastConnection, _con1);
		}

		[TestMethod]
		public void ConnectionFaulted()
		{
			_con1.Dispose();

			var dg = new Datagram
			{
				CommandType = 1,
				Data = new byte[1234]
			};
			_rand.NextBytes(dg.Data);

			var task = _con2.SendDatagramAsync(dg);
			Assert.IsTrue(_con2.IsFaulted);
			Assert.IsFalse(task.IsCompletedSuccessfully);
		}

		[TestMethod]
		public void MultipleSends()
		{
			var dgs = new Datagram[50];
			for (var i = 0; i < 50; i++)
			{
				dgs[i] = new Datagram
				{
					CommandType = (byte)i,
					Data = new byte[i + 10]
				};
				_rand.NextBytes(dgs[i].Data);
			}

			for (var i = 0; i < 50; i++)
				_ = _con2.SendDatagramAsync(dgs[i]);

			Thread.Sleep(2000);
			Assert.AreEqual(_emitter.NumReceived, 50);

			for (var i = 0; i < 50; i++)
			{
				var info = _emitter.All[i];

				Assert.IsNotNull(info.Item1);
				Assert.IsNotNull(info.Item2);
				Assert.AreEqual(info.Item1.CommandType, dgs[i].CommandType);
				Assert.AreEqual(info.Item1.Data.Length, dgs[i].Data.Length);
				Assert.AreEqual(info.Item1.Data[0], dgs[i].Data[0]);
				Assert.AreEqual(info.Item1.Data[^1], dgs[i].Data[^1]);
				Assert.AreEqual(info.Item2, _con1);
			}
		}
	}
}
