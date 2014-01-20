using System;

namespace TouchOSC
{
	public delegate void TouchOSCPacketSentHandler(object sender, TouchOSCPacketSentEventArgs e);

	public class TouchOSCPacketSentEventArgs : TouchOSCPacketEvent
	{
		internal TouchOSCPacketSentEventArgs(TouchOSCClient Client, TouchOSCPacket Packet)
		{
			this.Client = Client;
			this.Packet = Packet;
		}
	}
}
