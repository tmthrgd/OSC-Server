using System;

namespace TouchOSC
{
	public delegate void TouchOSCPacketReceivedHandler(object sender, TouchOSCPacketReceivedEventArgs e);

	public class TouchOSCPacketReceivedEventArgs : TouchOSCPacketEvent
	{
		internal TouchOSCPacketReceivedEventArgs(TouchOSCClient Client, TouchOSCPacket Packet)
		{
			this.Client = Client;
			this.Packet = Packet;
		}
	}
}
