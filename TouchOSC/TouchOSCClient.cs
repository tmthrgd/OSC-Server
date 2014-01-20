using System;
using System.Collections.Generic;
using System.Net;

using Bespoke.Common.Osc;

namespace TouchOSC
{
	public enum TouchOSCClientState
	{
		Accepted = 1,
		Rejected = 2
	}

	public enum TouchOSCType
	{
		Android,
		iPad,
		iPhone,
		iPod
	}

	public class TouchOSCClient
	{
		internal TouchOSCClient()
		{
			this.Properties = new Dictionary<string, object>();
		}

		#region Members
		internal TouchOSCServer Server;
		internal           bool IsConnected = false;

		public   IPEndPoint Destination { get; internal set; }
		public       string FullName    { get; internal set; }
		public       string HostName    { get; internal set; }
		public       string MACAddress  { get; internal set; }
		public   IPEndPoint Source      { get; internal set; }
		public TouchOSCType Type        { get; internal set; }
		public          Uri BaseUrl     { get; internal set; }

		protected TouchOSCClientState connectionState;
		public TouchOSCClientState ConnectionState
		{
			get
			{
				return this.connectionState;
			}
			set
			{
				if (this.connectionState != value)
				{
					this.connectionState = value;

					if (value == TouchOSCClientState.Accepted)
					{
						TouchOSCClientAcceptedEventArgs e = new TouchOSCClientAcceptedEventArgs(this);
						this.Server.InvokeClientAccepted(this, e);

						if (this.ClientAccepted != null)
							this.ClientAccepted(this, e);
					}
					else if (value == TouchOSCClientState.Rejected)
					{
						TouchOSCClientRejectedEventArgs e = new TouchOSCClientRejectedEventArgs(this);
						this.Server.InvokeClientRejected(this, e);

						if (this.ClientRejected != null)
							this.ClientRejected(this, e);
					}
				}
			}
		}

		public Dictionary<string, object> Properties { get; internal set; }
		#endregion

		#region Events
		public event     TouchOSCClientAcceptedHandler ClientAccepted;
		public event     TouchOSCClientRejectedHandler ClientRejected;
		
		public event    TouchOSCClientConnectedHandler ClientConnected;
		public event TouchOSCClientDisconnectedHandler ClientDisconnected;

		public event         TouchOSCClientLostHandler ClientLost;

		public event     TouchOSCPacketReceivedHandler PacketReceived;
		public event         TouchOSCPacketSentHandler PacketSent;
		#endregion

		#region Methods
		public void Send(string Address)
		{
			this.Send(new OscMessage(null, Address));
		}

		public void Send(string Address, params object[] Data)
		{
			OscMessage Message = new OscMessage(null, Address);
			
			foreach (object o in Data)
			{
				if (o is bool)
					Message.Append((bool)o ? 1 : 0);
				else
					Message.Append(o);
			}

			this.Send(Message);
		}

		protected void Send(OscMessage Message)
		{
			Message.Send(this.Destination);

			TouchOSCPacketSentEventArgs e = new TouchOSCPacketSentEventArgs(this, new TouchOSCPacket(this, Message));
			this.Server.InvokePacketSent(this, e);

			if (this.PacketSent != null)
				this.PacketSent(this, e);
		}

		internal void InvokeClientConnected(object sender, TouchOSCClientConnectedEventArgs e)
		{
			if (this.ClientConnected != null)
				this.ClientConnected.Invoke(sender, e);
		}

		internal void InvokeClientDisconnected(object sender, TouchOSCClientDisconnectedEventArgs e)
		{
			if (this.ClientDisconnected != null)
				this.ClientDisconnected(sender, e);
		}

		internal void InvokeClientLost(object sender, TouchOSCClientLostEventArgs e)
		{
			if (this.ClientLost != null)
				this.ClientLost(sender, e);
		}

		internal void InvokePacketReceived(object sender, TouchOSCPacketReceivedEventArgs e)
		{
			if (this.PacketReceived != null)
				this.PacketReceived(sender, e);
		}
		#endregion
	}
}