using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace munin_node_Service
{
	public class MuninNode : IDisposable
	{
		private readonly MuninService _parentService;
		private Socket _serverSocket;
		private readonly ManualResetEvent _connectionEstablished = new ManualResetEvent(false);
		private bool _listening;

		/// <summary>
		/// All munin server which are allowed to access this munin-node.
		/// </summary>
		public HashSet<IPAddress> AllowedServers { private set; get; }

		/// <summary>
		/// Specifies which IP munin-node should bind to and wait for connections
		/// </summary>
		public IPAddress ListenOn { private set; get; }

		/// <summary>
		/// The hostname which will be reported to munin. If null, munin-node will try to dertermine the hostname.
		/// </summary>
		public String Hostname { private set; get; }

		/// <summary>
		/// Contains all registered plugins.
		/// </summary>
		public HashSet<PluginBase> RegisteredPlugins { private set; get; }

		public MuninNode(MuninService service)
		{
			_parentService = service;
			Log("Loading Config");
			LoadConfig();
			Log("Config loaded, ready to start");
		}

		/// <summary>
		/// Log a message to console and maybe eventlog
		/// </summary>
		/// <param name="message">Message to log</param>
		/// <param name="type">Type of message</param>
		/// <param name="debug">If true, then only log to console, else log to eventlog too</param>
		private void Log(String message, EventLogEntryType type = EventLogEntryType.Information, bool debug = true)
		{
			if (_parentService != null && !debug)
			{
				_parentService.Log(message, type);
			}
			Console.WriteLine(message);
		}

		public void Dispose()
		{
			//TODO: dispose everything
		}

		/// <summary>
		/// Start munin-node
		/// </summary>
		public void Start()
		{
			_serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
			_serverSocket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
			_serverSocket.Bind(new IPEndPoint(ListenOn, 4949));
			_serverSocket.Listen(10);
			_listening = true;
			Log(String.Format("munin-node Service ready on {0}", ListenOn), EventLogEntryType.Information, false);
			while (_listening)
			{
				_connectionEstablished.Reset();
				_serverSocket.BeginAccept(HandleConnection, _serverSocket);
				_connectionEstablished.WaitOne();
			}
		}

		/// <summary>
		/// Stop munin-node
		/// </summary>
		public void Stop()
		{
			_listening = false;
			try
			{
				_serverSocket.Shutdown(SocketShutdown.Both);
				_serverSocket.Close();
			}
			catch (Exception e)
			{
				Log(String.Format("Exception during stop: {0}", e.Message), EventLogEntryType.Error, false);
			}

		}

		/// <summary>
		/// Loads the config file
		/// </summary>
		public void LoadConfig()
		{
			// Search for plugins and load them
			RegisteredPlugins = new HashSet<PluginBase>();

			string[] files = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins"), "*.dll");
			foreach (var file in files)
			{
				Assembly possiblePlugin = Assembly.LoadFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", file));
				foreach (var type in possiblePlugin.GetTypes())
				{
					if (!typeof(PluginBase).IsAssignableFrom(type)) continue;

					var plugin = (PluginBase)Activator.CreateInstance(type);
					plugin.Initialize();
					RegisteredPlugins.Add(plugin);

				}

			}
			Log(String.Format("{0} Plugin(s) loaded: {1}", RegisteredPlugins.Count, RegisteredPlugins.Select(_ => _.GetName()).Aggregate((plugins, next) => plugins + " " + next)));

			AllowedServers = new HashSet<IPAddress>();
			if (true) // TODO: check whether config speficied allowed hosts
			{
				AllowedServers.Add(IPAddress.IPv6Any);
			}

			ListenOn = String.IsNullOrWhiteSpace(Properties.Settings.Default.BindTo) ? IPAddress.IPv6Any : IPAddress.Parse(Properties.Settings.Default.BindTo);
			Hostname = String.IsNullOrWhiteSpace(Properties.Settings.Default.Hostname) ? Dns.GetHostName() : Properties.Settings.Default.Hostname;
		}

		/// <summary>
		/// Helper class for async receive calls
		/// </summary>
		private class ReadState
		{
			public readonly byte[] Buffer = new byte[1024];
			public Socket ClientSocket;
			public readonly StringBuilder StringBuilder = new StringBuilder();
			public PluginBase.Capabilities Capabilities = PluginBase.Capabilities.None;
		}

		/// <summary>
		/// Handles an incoming connection.
		/// </summary>
		/// <param name="result"></param>
		private void HandleConnection(IAsyncResult result)
		{
			_connectionEstablished.Set();
			var clientSocket = ((Socket)result.AsyncState).EndAccept(result);
			Log(String.Format("Connection from {0}", clientSocket.RemoteEndPoint));
			// TODO: check if server is allowed to connect
			var readState = new ReadState { ClientSocket = clientSocket };
			Send(String.Format("# munin node at {0}\n", Hostname), clientSocket);
			clientSocket.BeginReceive(readState.Buffer, 0, readState.Buffer.Length, SocketFlags.None, ReceiveCallback, readState);
		}

		/// <summary>
		/// Handles incoming data
		/// </summary>
		/// <param name="result"></param>
		private void ReceiveCallback(IAsyncResult result)
		{
			var readState = (ReadState)result.AsyncState;
			SocketError error;
			var readBytes = readState.ClientSocket.EndReceive(result, out error);
			Log(String.Format("Received {0} bytes", readBytes));
			switch (error)
			{
				case SocketError.ConnectionAborted:
				case SocketError.ConnectionReset:
				case SocketError.Disconnecting:
				case SocketError.Interrupted:
				case SocketError.Shutdown:
				case SocketError.Fault:
				case SocketError.TimedOut:
					Log(String.Format("SocketError: {0}", error));
					return; // On error/socket close, don't go any further
			}

			//check if connection is dead:
			if (readState.ClientSocket.Poll(1000, SelectMode.SelectRead) && readState.ClientSocket.Available == 0)
			{
				//socket is dead
				readState.ClientSocket.Close();
				Log("Client Disconnected");
				return;
			}

			// if there is something to read
			if (readBytes > 0)
			{
				readState.StringBuilder.Append(Encoding.ASCII.GetString(readState.Buffer, 0, readBytes));
				if (TryHandleCommand(readState.StringBuilder.ToString(), readState)) // try to execute
				{
					readState.StringBuilder.Clear();
				}
			}

			// start next receive
			try
			{
				readState.ClientSocket.BeginReceive(readState.Buffer, 0, readState.Buffer.Length, SocketFlags.None, ReceiveCallback, readState);
			}
			catch (ObjectDisposedException)
			{
				Log("Client Disconnected");
			}
		}

		/// <summary>
		/// Send data to a munin server
		/// </summary>
		/// <param name="message"></param>
		/// <param name="socket"></param>
		private void Send(String message, Socket socket)
		{
			Log(String.Format("Sending answer: {0}", message));
			byte[] byteData = Encoding.ASCII.GetBytes(message);
			socket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, socket);
		}

		/// <summary>
		/// Handles finished sends
		/// </summary>
		/// <param name="result"></param>
		private void SendCallback(IAsyncResult result)
		{
			((Socket)result.AsyncState).EndSend(result);
		}

		/// <summary>
		/// Tries to parse a command and execute it.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="state"></param>
		/// <returns>True, if command was succesfully executed, False if command was not completly received</returns>
		private bool TryHandleCommand(string command, ReadState state)
		{
			//munin-update ends every command with a \n, so we can test for that
			if (!command.EndsWith("\n"))
				return false; // command was not ended with a \n so command was not complete, receive more bytes

			var answerSocket = state.ClientSocket;
			command = command.TrimEnd(new[] { '\n' });

			Log(String.Format("Got command: {0}", command));
			var cmd = command;
			var args = new List<string>();

			// if command contains arguments, separate them.
			if (command.Contains(" "))
			{
				var split = command.Split(' ');
				cmd = split[0];
				for (var i = 1; i < split.Length; i++)
					args.Add(split[i]);
			}

			// execute command
			PluginBase plugin;
			switch (cmd)
			{
				case "cap":
					if (args.Contains("multigraph"))
						state.Capabilities |= PluginBase.Capabilities.Multigraph;
					if (args.Contains("dirtyconfig"))
						state.Capabilities |= PluginBase.Capabilities.DirtyConfig;
					Send("cap multigraph dirtyconfig\n", answerSocket); // We are capable of multigraph and dirtyconfig
					break;
				case "list":
					// Return a list of all available plugins
					var plugins = RegisteredPlugins.Where(_ => !_.GetCapabilities().HasFlag(PluginBase.Capabilities.Multigraph) || (_.GetCapabilities() & state.Capabilities).HasFlag(PluginBase.Capabilities.Multigraph))
						.Select(_ => _.GetName()).Aggregate((regplugins, next) => regplugins + " " + next) + "\n";
					Send(plugins, answerSocket);
					break;
				case "config":
					// Return config parameters for a specific plugin
					plugin = RegisteredPlugins.FirstOrDefault(_ => _.GetName().Equals(args[0]));
					if (plugin == null)
					{
						Send("# Unknown service\n.\n", answerSocket);
						break;
					}
					Send(FormatForMunin(plugin.GetConfig(state.Capabilities)), answerSocket);
					break;
				case "fetch":
					// Return values for a specific plugin
					plugin = RegisteredPlugins.FirstOrDefault(_ => _.GetName().Equals(args[0]));
					if (plugin == null)
					{
						Send("# Unknown service\n.\n", answerSocket);
						break;
					}
					Send(FormatForMunin(plugin.GetValues(state.Capabilities)), answerSocket);
					break;
				case "nodes":
					// Return alle nodes this munin-node queries (only this node)
					Send(String.Format("{0}\n.\n", Hostname), answerSocket);
					break;
				case "version":
					// Return the munin-node version
					Send(String.Format("munin node on {0} version: 2.0.0\n", Hostname), answerSocket);
					break;
				case "quit":
					answerSocket.Close();
					break;
				default:
					Send("# Unknown command. Try cap, list, nodes, config, fetch, version oder quit\n", answerSocket);
					break;
			}
			return true;
		}

		/// <summary>
		/// Formats fetch and config output so that they always contain an \n.\n at the end.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private static string FormatForMunin(string input)
		{
			if (input.EndsWith("\n") && !input.EndsWith("\n.\n"))
				return input + ".\n";
			if (!input.EndsWith("\n") && !input.EndsWith("\n.\n"))
				return input + "\n.\n";

			return input;
		}
	}
}
