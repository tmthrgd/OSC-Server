using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;

using Dns = System.Net.Dns;
using Regex = System.Text.RegularExpressions.Regex;

using TouchOSC;

namespace TouchOSC_Server
{
	class Program
	{
		static string DBConnString = "Data Source=osc.s3db";
		static    int ServerPort   = TouchOSCServer.DefaultPort;
		static string ServerName   = string.Format("{0}: {1}", ((AssemblyTitleAttribute)Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), true)[0]).Title, Dns.GetHostName());

		static   TouchOSCServer Server;
		static SQLiteConnection SQLiteDB;
		
		static Dictionary<string, object[]> Values = new Dictionary<string, object[]>();

		/*TEMPORARY CODE*/
		static object ConsoleLock = new object();
		/*TEMPORARY CODE*/

		static void Main(string[] args)
		{
			Console.Title = ServerName;

			ConsoleColor StartColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Client Found");
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Client Lost");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Client Accepted");
			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.WriteLine("Client Rejected");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("Packet Received");
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine("Packet Sent");
			Console.ForegroundColor = StartColor;
			Console.WriteLine();

			using (SQLiteDB = new SQLiteConnection(DBConnString))
			using (Server = new TouchOSCServer(ServerPort, ServerName))
			{
				SQLiteDB.Open();
				
				using (SQLiteCommand Command = SQLiteDB.CreateCommand())
				{
					Command.CommandText = "CREATE TABLE IF NOT EXISTS osc_auth (mac TEXT UNIQUE NOT NULL PRIMARY KEY)";
					Command.ExecuteNonQuery();
				}

				Server.ClientAccepted  += new TouchOSCClientAcceptedHandler(Server_ClientAccepted);
				Server.ClientRejected  += new TouchOSCClientRejectedHandler(Server_ClientRejected);

				Server.ClientFound     += new TouchOSCClientFoundHandler(Server_ClientFound);
				Server.ClientLost      += new TouchOSCClientLostHandler(Server_ClientLost);

				Server.PacketReceived  += new TouchOSCPacketReceivedHandler(Server_PacketReceived);
				Server.PacketSent      += new TouchOSCPacketSentHandler(Server_PacketSent);

				Server.Start();

				while (Console.ReadLine() == null) ;

				Server.Stop();
				SQLiteDB.Close();
			}
		}

		static void Server_ClientAccepted(object sender, TouchOSCClientAcceptedEventArgs e)
		{
			/*TEMPORARY CODE*/
			lock (ConsoleLock)
			{
				ConsoleColor StartColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("{3} {0}:{1} {2}", e.Client.Destination.Address, e.Client.Destination.Port, e.Client.FullName, e.Time.ToString("HH:mm:ss"));
				Console.ForegroundColor = StartColor;
			}
			/*TEMPORARY CODE*/
			
			foreach (KeyValuePair<string, object[]> Value in Values)
				e.Client.Send(Value.Key, Value.Value);
		}

		static void Server_ClientRejected(object sender, TouchOSCClientRejectedEventArgs e)
		{
			/*TEMPORARY CODE*/
			lock (ConsoleLock)
			{
				ConsoleColor StartColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.WriteLine("{3} {0}:{1} {2}", e.Client.Destination.Address, e.Client.Destination.Port, e.Client.FullName, e.Time.ToString("HH:mm:ss"));
				Console.ForegroundColor = StartColor;
			}
			/*TEMPORARY CODE*/
		}

		static bool Server_ClientFound(object sender, TouchOSCClientFoundEventArgs e)
		{
			/*TEMPORARY CODE*/
			lock (ConsoleLock)
			{
				ConsoleColor StartColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("{3} {0}:{1} {2}", e.Client.Destination.Address, e.Client.Destination.Port, e.Client.FullName, e.Time.ToString("HH:mm:ss"));
				Console.ForegroundColor = StartColor;
			}
			/*TEMPORARY CODE*/

			using (SQLiteCommand Command = SQLiteDB.CreateCommand())
			{
				Command.CommandText = "SELECT mac FROM osc_auth WHERE mac = @mac";
				Command.Parameters.Add(new SQLiteParameter("@mac", e.Client.MACAddress));
				return Command.ExecuteScalar() as string == e.Client.MACAddress;
			}
		}

		static void Server_ClientLost(object sender, TouchOSCClientLostEventArgs e)
		{
			/*TEMPORARY CODE*/
			lock (ConsoleLock)
			{
				ConsoleColor StartColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("{3} {0}:{1} {2}", e.Client.Destination.Address, e.Client.Destination.Port, e.Client.FullName, e.Time.ToString("HH:mm:ss"));
				Console.ForegroundColor = StartColor;
			}
			/*TEMPORARY CODE*/
		}

		static void Server_PacketReceived(object sender, TouchOSCPacketReceivedEventArgs e)
		{
			/*TEMPORARY CODE*/
			lock (ConsoleLock)
			{
				ConsoleColor StartColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("{3} {0}:{1} {2}", e.Client.Destination.Address, e.Client.Destination.Port, e.Packet.Address, e.Time.ToString("HH:mm:ss"));

				for (int i = 0; i < e.Packet.Data.Length; i++)
					Console.Write(" " + e.Packet.Data[i]);

				Console.WriteLine();
				Console.ForegroundColor = StartColor;
			}
			/*TEMPORARY CODE*/

			if (e.Packet.Address.AbsolutePath == "/ping")
				;
			else if (e.Packet.Address.AbsolutePath == "/accxyz")
			{
				
			}
			else if (e.Packet.Address.AbsolutePath.EndsWith("/z"))
			{
				
			}
			else if (Regex.IsMatch(e.Packet.Address.AbsolutePath, "^/[0-9]+$"))
			{
				foreach (KeyValuePair<string, object[]> Value in Values.Where(a => { return a.Key.StartsWith(e.Packet.Address.AbsolutePath + "/"); }))
					e.Client.Send(Value.Key, Value.Value);
			}
			else if (e.Packet.Data.Length != 0)
			{
				Values[e.Packet.Address.AbsolutePath] = e.Packet.Data;
				Server.Broadcast(e.Packet.Address.AbsolutePath, e.Packet.Data);
			}
		}

		static void Server_PacketSent(object sender, TouchOSCPacketSentEventArgs e)
		{
			/*TEMPORARY CODE*/
			lock (ConsoleLock)
			{
				ConsoleColor StartColor = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.Write("{3} {0}:{1} {2}", e.Client.Destination.Address, e.Client.Destination.Port, e.Packet.Address, e.Time.ToString("HH:mm:ss"));

				for (int i = 0; i < e.Packet.Data.Length; i++)
					Console.Write(" " + e.Packet.Data[i]);

				Console.WriteLine();
				Console.ForegroundColor = StartColor;
			}
			/*TEMPORARY CODE*/
		}
	}
}