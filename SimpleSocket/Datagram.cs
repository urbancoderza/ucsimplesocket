using System;
using System.Globalization;
using System.Text;

namespace UCSimpleSocket
{
	/// <summary>
	/// A class representing a datagram that was received by a <see cref="Connection"/>.
	/// </summary>
	public sealed class Datagram
	{
		/// <summary>
		/// The command type of the datagram. The specific value of <see cref="CommandType"/> is application specific and should be consistent between applications communicating with eachother.
		/// </summary>
		public byte CommandType { get; set; }

		/// <summary>
		/// The user defined data that was sent or received.
		/// </summary>
		public byte[] Data { get; set; }

		private string ToString(string linePrefix)
		{
			var sb = new StringBuilder();
			sb.AppendFormat("{1}CommandType:\t\t{0}{2}", CommandType, linePrefix, Environment.NewLine);
			sb.AppendFormat("{1}CommendDataLength:\t{0}", Data.Length, linePrefix);
			return sb.ToString();
		}

		/// <summary>
		/// A string that represents the current object.
		/// </summary>
		/// <returns>A <see cref="string"/> that represents the current object.</returns>
		public override string ToString()
		{
			return ToString(string.Empty);
		}

		/// <summary>
		/// A string that represents the current object with the properties indented by <paramref name="numTabs"/> number of tabs."/>
		/// </summary>
		/// <param name="numTabs">The number of tab characters that each property line should be indented with.</param>
		/// <returns>A <see cref="string"/> that represents the current object with the properties indented by <paramref name="numTabs"/> number of tabs."/></returns>
		public string ToString(byte numTabs)
		{
			return ToString(new string('\t', numTabs));
		}

		internal Datagram Copy()
		{
			var toReturn = new Datagram
			{
				CommandType = CommandType,
				Data = new byte[Data.Length]
			};
			Data.CopyTo(toReturn.Data, 0);

			return toReturn;
		}
	}
}
