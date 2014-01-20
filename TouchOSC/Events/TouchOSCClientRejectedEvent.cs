using System;

namespace TouchOSC
{
	public delegate void TouchOSCClientRejectedHandler(object sender, TouchOSCClientRejectedEventArgs e);

	public class TouchOSCClientRejectedEventArgs : TouchOSCClientEvent
	{
		internal TouchOSCClientRejectedEventArgs(TouchOSCClient Client)
		{
			this.Client = Client;
		}
	}
}