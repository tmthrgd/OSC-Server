using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Regex = System.Text.RegularExpressions.Regex;
using StringBuilder = System.Text.StringBuilder;
using Win32Exception = System.ComponentModel.Win32Exception;

using Bespoke.Common.Osc;
using Bonjour;

namespace TouchOSC
{
	public class TouchOSCServer : IDisposable
	{
		static TouchOSCServer()
		{
			OscPacket.LittleEndianByteOrder = false;
		}

		#region Static Members
		protected const string RegType = "_osc._udp.";

		public const int DefaultPort = 8000;
		#endregion

		#region Constructor
		public TouchOSCServer()
			: this(DefaultPort)
		{
		}

		public TouchOSCServer(int Port)
			: this(Port, string.Format("{0}:{1}", Dns.GetHostName(), Port))
		{
		}

		public TouchOSCServer(string Name)
			: this(DefaultPort, Name)
		{
		}

		public TouchOSCServer(int Port, string Name)
		{
			this.Name = Name;
			this.Port = Port;

			this.Server = new OscServer(TransportType.Udp, IPAddress.Any, this.Port);
			this.Server.PacketReceived += new EventHandler<OscPacketReceivedEventArgs>(this.Server_PacketReceived);
			
			this.EventManager.ServiceFound      += new _IDNSSDEvents_ServiceFoundEventHandler(this.EventManager_ServiceFound);
			this.EventManager.ServiceLost       += new _IDNSSDEvents_ServiceLostEventHandler(this.EventManager_ServiceLost);
			this.EventManager.ServiceRegistered += new _IDNSSDEvents_ServiceRegisteredEventHandler(this.EventManager_ServiceRegistered);
			this.EventManager.ServiceResolved   += new _IDNSSDEvents_ServiceResolvedEventHandler(this.EventManager_ServiceResolved);
		}
		#endregion

		#region Members
		protected Dictionary<uint, TouchOSCClient> Clients      = new Dictionary<uint, TouchOSCClient>();
		protected                        OscServer Server;
		protected                DNSSDEventManager EventManager = new DNSSDEventManager();
		/**/
		protected                     DNSSDService Service = new DNSSDService();
		protected                     DNSSDService Registrar;
		protected                     DNSSDService Browser;
		/**/

		public string Name { get; protected set; }
		public    int Port { get; protected set; }
		#endregion

		#region Events
		public event     TouchOSCClientAcceptedHandler ClientAccepted;
		public event     TouchOSCClientRejectedHandler ClientRejected;

		public event    TouchOSCClientConnectedHandler ClientConnected;
		public event TouchOSCClientDisconnectedHandler ClientDisconnected;

		public event        TouchOSCClientFoundHandler ClientFound;
		public event         TouchOSCClientLostHandler ClientLost;

		public event     TouchOSCPacketReceivedHandler PacketReceived;
		public event         TouchOSCPacketSentHandler PacketSent;
		#endregion

		#region Methods
		public void Start()
		{
			this.Server.Start();
			this.Registrar = this.Service.Register(0, 0, this.Name, RegType, null, null, (ushort)this.Port, null, this.EventManager);
		}

		public void Stop()
		{
			this.Server.Stop();
			this.Clients.Clear();

			/*if (this.Browser != null)
				this.Browser.Stop();

			if (this.Registrar != null)
				this.Registrar.Stop();

			this.Service.Stop();*/
		}

		public void Dispose()
		{
			this.Stop();

			this.EventManager.ServiceFound      -= new _IDNSSDEvents_ServiceFoundEventHandler(this.EventManager_ServiceFound);
			this.EventManager.ServiceLost       -= new _IDNSSDEvents_ServiceLostEventHandler(this.EventManager_ServiceLost);
			this.EventManager.ServiceRegistered -= new _IDNSSDEvents_ServiceRegisteredEventHandler(this.EventManager_ServiceRegistered);
			this.EventManager.ServiceResolved   -= new _IDNSSDEvents_ServiceResolvedEventHandler(this.EventManager_ServiceResolved);

			this.Clients.Clear();
		}

		public void Broadcast(string Address)
		{
			foreach (KeyValuePair<uint, TouchOSCClient> Client in this.Clients)
				Client.Value.Send(Address);
		}

		public void Broadcast(string Address, params object[] Data)
		{
			foreach (KeyValuePair<uint, TouchOSCClient> Client in this.Clients)
				Client.Value.Send(Address, Data);
		}

		protected void EventManager_ServiceRegistered(DNSSDService service, DNSSDFlags flags, string name, string regtype, string domain)
		{
			this.Browser = service.Browse(0, 0, RegType, null, this.EventManager);
		}

		protected void EventManager_ServiceFound(DNSSDService browser, DNSSDFlags flags, uint ifIndex, string serviceName, string regtype, string domain)
		{
			if (Regex.IsMatch(serviceName, @"^[^ \n\r]+ \[(iPad|iPhone|iPod touch)\] \(TouchOSC\)$"))
				browser.Resolve(flags, ifIndex, serviceName, regtype, domain, this.EventManager);
		}

		protected void EventManager_ServiceLost(DNSSDService browser, DNSSDFlags flags, uint ifIndex, string serviceName, string regtype, string domain)
		{
			if (!this.Clients.ContainsKey(ifIndex))
				return;

			if (this.Clients[ifIndex].IsConnected)
			{
				TouchOSCClientDisconnectedEventArgs ed = new TouchOSCClientDisconnectedEventArgs(this.Clients[ifIndex]);

				if (this.ClientDisconnected != null)
					this.ClientDisconnected(this, ed);

				this.Clients[ifIndex].InvokeClientDisconnected(this, ed);
			}

			TouchOSCClientLostEventArgs el = new TouchOSCClientLostEventArgs(this.Clients[ifIndex]);

			if (this.ClientLost != null)
				this.ClientLost(this, el);

			this.Clients[ifIndex].InvokeClientLost(this, el);
			this.Clients.Remove(ifIndex);
		}

		protected void EventManager_ServiceResolved(DNSSDService service, DNSSDFlags flags, uint ifIndex, string fullname, string hostname, ushort port, TXTRecord record)
		{
			IPAddress[] Address = Dns.GetHostAddresses(hostname)
				.Where(a => { return !(a.IsIPv6LinkLocal || a.IsIPv6Multicast || a.IsIPv6SiteLocal || a.IsIPv6Teredo); })
				.ToArray();

			if (Address.Length == 0)
				return;

			TouchOSCClient Client = this.Clients.FirstOrDefault(a => { return a.Value.Source.Address.Equals(Address[0]); }).Value;

			if (Client != default(TouchOSCClient))
				return;

			/* MAC Address */
			                   byte[] pMacAddr   = new byte[6];
			                     uint PhyAddrLen = (uint)pMacAddr.Length;
			Win32.SendARPReturnValues result     = Win32.SendARP((uint)Address[0].Address, Win32.INADDR_ANY, pMacAddr, ref PhyAddrLen);
			            StringBuilder MAC        = new StringBuilder((int)pMacAddr.Length * 3, (int)pMacAddr.Length * 3);

			System.Diagnostics.Debug.WriteLineIf(result != Win32.SendARPReturnValues.NO_ERROR, string.Format("SendARP failed {0}", Enum.GetName(typeof(Win32.SendARPReturnValues), result) ?? result.ToString()));

			for (int i = 0; i < pMacAddr.Length; i++)
				MAC.AppendFormat(":{0:X2}", pMacAddr[i]);

			MAC.Remove(0, 1);
			/* MAC Address */
			
			fullname = fullname.Replace(@"\032", " ");
			string Type = Regex.Match(fullname, @"[^ \n\r]+ \[(iPad|iPhone|iPod touch)\] \(TouchOSC\)")
					.Groups[1]
					.Value;
			this.Clients[ifIndex] = Client = new TouchOSCClient
			{
				BaseUrl     = new UriBuilder("osc", Address[0].ToString(), port).Uri,
				Destination = new IPEndPoint(Address[0], port),
				FullName    = fullname,
				HostName    = hostname,
				MACAddress  = MAC.ToString(),
				Server      = this,
				Source      = new IPEndPoint(Address[0], this.Port),
				Type        = Enum.IsDefined(typeof(TouchOSCType), Type)
								? (TouchOSCType)Enum.Parse(typeof(TouchOSCType), Type)
								: (Type == "iPod touch")
									? TouchOSCType.iPod
									: 0
			};

			if (this.ClientFound != null)
			{
				TouchOSCClientFoundEventArgs e = new TouchOSCClientFoundEventArgs(Client);
				bool Accepted = true;

				foreach (Delegate d in this.ClientFound.GetInvocationList())
					Accepted &= (bool)d.DynamicInvoke(this, e);

				Client.ConnectionState = Accepted
					? TouchOSCClientState.Accepted
					: TouchOSCClientState.Rejected;
			}
			else
				Client.ConnectionState = TouchOSCClientState.Accepted;
		}

		protected void Server_PacketReceived(object sender, OscPacketReceivedEventArgs e)
		{
			TouchOSCClient Client = this.Clients.FirstOrDefault(a => { return a.Value.Source.Address.Equals(e.Packet.SourceEndPoint.Address); }).Value;

			if (Client == default(TouchOSCClient) || Client.ConnectionState != TouchOSCClientState.Accepted)
				return;

			if (!Client.IsConnected)
			{
				TouchOSCClientConnectedEventArgs ec = new TouchOSCClientConnectedEventArgs(Client);

				if (this.ClientConnected != null)
					this.ClientConnected(this, ec);

				Client.InvokeClientConnected(this, ec);
				Client.IsConnected = true;
			}

			TouchOSCPacketReceivedEventArgs er = new TouchOSCPacketReceivedEventArgs(Client, new TouchOSCPacket(Client, e.Packet));

			if (this.PacketReceived != null)
				this.PacketReceived(this, er);

			Client.InvokePacketReceived(this, er);
		}

		internal void InvokeClientAccepted(object sender, TouchOSCClientAcceptedEventArgs e)
		{
			if (this.ClientAccepted != null)
				this.ClientAccepted(sender, e);
		}

		internal void InvokeClientRejected(object sender, TouchOSCClientRejectedEventArgs e)
		{
			if (this.ClientRejected != null)
				this.ClientRejected(sender, e);
		}

		internal void InvokePacketSent(object sender, TouchOSCPacketSentEventArgs e)
		{
			if (this.PacketSent != null)
				this.PacketSent(sender, e);
		}
		#endregion
	}
}
