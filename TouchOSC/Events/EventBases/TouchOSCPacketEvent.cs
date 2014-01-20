using System;

namespace TouchOSC
{
	public abstract class TouchOSCPacketEvent : TouchOSCClientEvent
	{
		public TouchOSCPacket Packet { get; protected set; }
	}
}