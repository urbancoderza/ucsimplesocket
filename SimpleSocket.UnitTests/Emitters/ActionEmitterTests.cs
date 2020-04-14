using Microsoft.VisualStudio.TestTools.UnitTesting;
using UCSimpleSocket.Emitters;
using System;
using System.Threading;

namespace UCSimpleSocket.UnitTests.Emitters
{
	[TestClass]
	public class ActionEmitterTests
	{
		[TestMethod]
		public void Emit()
		{
			(Connection con, Datagram dg, DateTime dt) tuple1 = (null, new Datagram { CommandType = 1, Data = new byte[1024]}, DateTime.Now);
			Thread.Sleep(1100);
			(Connection con, Datagram dg, DateTime dt) tuple2 = (null, new Datagram { CommandType = 2, Data = new byte[512] }, DateTime.Now);

			var emitter = new ActionEmitter((con, dg, dt) =>
			{
				Assert.IsNull(con);
				Assert.IsNotNull(dg);

				if (dg.CommandType == 1)
				{
					Assert.AreEqual(dg.Data.Length, tuple1.dg.Data.Length);
					Assert.AreEqual(dt, tuple1.dt);
				}

				if (dg.CommandType == 2)
				{
					Assert.AreEqual(dg.Data.Length, tuple2.dg.Data.Length);
					Assert.AreEqual(dt, tuple2.dt);
				}
			});
			
			emitter.DatagramReceived(tuple1.con, tuple1.dg, tuple1.dt);
			emitter.DatagramReceived(tuple2.con, tuple2.dg, tuple2.dt);
		}
	}
}
