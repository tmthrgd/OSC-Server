using System;

namespace TouchOSC
{
	public delegate void TouchOSCClientLostHandler(object sender, TouchOSCClientLostEventArgs e);

	public class TouchOSCClientLostEventArgs : TouchOSCClientEvent
	{
		internal TouchOSCClientLostEventArgs(TouchOSCClient Client)
		{
			this.Client = Client;
		}
	}
}