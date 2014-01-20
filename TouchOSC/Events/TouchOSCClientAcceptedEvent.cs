using System;

namespace TouchOSC
{
	public delegate void TouchOSCClientAcceptedHandler(object sender, TouchOSCClientAcceptedEventArgs e);

	public class TouchOSCClientAcceptedEventArgs : TouchOSCClientEvent
	{
		internal TouchOSCClientAcceptedEventArgs(TouchOSCClient Client)
		{
			this.Client = Client;
		}
	}
}