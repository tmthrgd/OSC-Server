using System;

namespace TouchOSC
{
	public delegate void TouchOSCClientConnectedHandler(object sender, TouchOSCClientConnectedEventArgs e);

	public class TouchOSCClientConnectedEventArgs : TouchOSCClientEvent
	{
		internal TouchOSCClientConnectedEventArgs(TouchOSCClient Client)
		{
			this.Client = Client;
		}
	}
}