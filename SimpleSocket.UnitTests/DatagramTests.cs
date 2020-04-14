using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UCSimpleSocket.UnitTests
{
	[TestClass]
	public class DatagramTests
	{
		[TestMethod]
		public void Tabs()
		{
			var dg = new Datagram
			{
				CommandType = 1,
				Data = new byte[50]
			};

			var dgStr = dg.ToString(2);
			Assert.IsNotNull(dgStr);
			var lines = dgStr.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual(2, lines.Length);
			Assert.IsTrue(lines[0].StartsWith("\t\t"));
			Assert.IsTrue(lines[1].StartsWith("\t\t"));
		}
	}
}
