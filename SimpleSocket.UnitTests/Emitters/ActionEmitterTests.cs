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
			(Connection con, byte[] dg, DateTime dt) tuple1 = (null, new byte[1024], DateTime.Now);
			Thread.Sleep(1100);
			(Connection con, byte[] dg, DateTime dt) tuple2 = (null, new byte[512], DateTime.Now);

			var emitter = new ActionEmitter((con, dg, dt) =>
			{
				Assert.IsNull(con);
				Assert.IsNotNull(dg);

				if (dg.Length == 1024)
				{
					Assert.AreEqual(dg.Length, tuple1.dg.Length);
					Assert.AreEqual(dt, tuple1.dt);
				}
				else if (dg.Length == 512)
				{
					Assert.AreEqual(dg.Length, tuple2.dg.Length);
					Assert.AreEqual(dt, tuple2.dt);
				}
				else
					Assert.Fail("Unexpected datagram.");
			});
			
			emitter.DatagramReceived(tuple1.con, tuple1.dg, tuple1.dt);
			emitter.DatagramReceived(tuple2.con, tuple2.dg, tuple2.dt);
		}
	}
}
