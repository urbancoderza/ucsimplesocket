using Microsoft.VisualStudio.TestTools.UnitTesting;
using UCSimpleSocket.Emitters;
using System;
using System.Threading;

namespace UCSimpleSocket.UnitTests.Emitters
{
	[TestClass]
	public class QueueEmitterTests
	{
		[TestMethod]
		public void EnqueueDequeuePeek()
		{
			(Connection con, byte[] dg, DateTime dt) tuple1 = (null, new byte[1024], DateTime.Now);
			Thread.Sleep(1100);
			(Connection con, byte[] dg, DateTime dt) tuple2 = (null, new byte[512], DateTime.Now);

			var emitter = new QueueEmitter();
			Assert.IsFalse(emitter.TryPeek(out _, out _, out _));
			Assert.IsFalse(emitter.TryDequeue(out _, out _, out _));

			Assert.AreEqual(emitter.Count, 0);

			emitter.DatagramReceived(tuple1.con, tuple1.dg, tuple1.dt);
			emitter.DatagramReceived(tuple2.con, tuple2.dg, tuple2.dt);

			Assert.AreEqual(emitter.Count, 2);
			Assert.IsTrue(emitter.TryPeek(out Connection con, out byte[] dg, out DateTime? dt));
			Assert.IsNull(con);
			Assert.IsNotNull(dg);
			Assert.IsNotNull(dt);
			Assert.AreEqual(con, tuple1.con);
			Assert.AreEqual(dt, tuple1.dt);

			Assert.IsTrue(emitter.TryDequeue(out con, out dg, out dt));
			Assert.IsNull(con);
			Assert.IsNotNull(dg);
			Assert.IsNotNull(dt);
			Assert.AreEqual(con, tuple1.con);
			Assert.AreEqual(dt, tuple1.dt);

			Assert.AreEqual(tuple1.dg.Length, dg.Length);

			Assert.AreEqual(emitter.Count, 1);

			Assert.IsTrue(emitter.TryPeek(out con, out dg, out dt));
			Assert.IsNull(con);
			Assert.IsNotNull(dg);
			Assert.IsNotNull(dt);
			Assert.AreEqual(con, tuple2.con);
			Assert.AreEqual(dt, tuple2.dt);

			Assert.IsTrue(emitter.TryDequeue(out con, out dg, out dt));
			Assert.IsNull(con);
			Assert.IsNotNull(dg);
			Assert.IsNotNull(dt);
			Assert.AreEqual(con, tuple2.con);
			Assert.AreEqual(dt, tuple2.dt);

			Assert.AreEqual(tuple2.dg.Length, dg.Length);

			Assert.AreEqual(emitter.Count, 0);
		}
	}
}
