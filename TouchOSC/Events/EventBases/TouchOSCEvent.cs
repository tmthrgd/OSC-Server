using System;

namespace TouchOSC
{
	public abstract class TouchOSCEvent
	{
		internal TouchOSCEvent()
		{
			this.Time = DateTime.Now;
		}

		public DateTime Time { get; protected set; }
	}
}