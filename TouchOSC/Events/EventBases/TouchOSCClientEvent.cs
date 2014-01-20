using System;

namespace TouchOSC
{
	public abstract class TouchOSCClientEvent : TouchOSCEvent
	{
		public TouchOSCClient Client { get; protected set; }
	}
}