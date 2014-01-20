using System;

namespace TouchOSC
{
	public delegate bool TouchOSCClientFoundHandler(object sender, TouchOSCClientFoundEventArgs e);

	public class TouchOSCClientFoundEventArgs : TouchOSCClientEvent
	{
		internal TouchOSCClientFoundEventArgs(TouchOSCClient Client)
		{
			this.Client = Client;
		}
	}
}