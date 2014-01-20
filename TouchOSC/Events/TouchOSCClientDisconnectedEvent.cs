using System;

namespace TouchOSC
{
	public delegate void TouchOSCClientDisconnectedHandler(object sender, TouchOSCClientDisconnectedEventArgs e);

	public class TouchOSCClientDisconnectedEventArgs : TouchOSCClientEvent
	{
		internal TouchOSCClientDisconnectedEventArgs(TouchOSCClient Client)
		{
			this.Client = Client;
		}
	}
}