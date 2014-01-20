using System;

using Bespoke.Common.Osc;

namespace TouchOSC
{
	public class TouchOSCPacket
	{
		internal TouchOSCPacket(TouchOSCClient Client, OscPacket Packet)
		{
			this.Address = new Uri(Client.BaseUrl, Packet.Address);
			this.Data    = new object[Packet.Data.Count];
			Packet.Data.CopyTo(this.Data, 0);
		}

		#region Members
		public      Uri Address { get; protected set; }
		public object[] Data    { get; protected set; }
		public     bool IsDown
		{
			get
			{
				if (this.Data.Length != 1 || !(this.Data[0] is Single))
					throw new ArgumentException();

				return (Single)this.Data[0] == 1;
			}
		}
		#endregion
	}
}