﻿using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace munin_node_Service
{
	public sealed class MuninService : ServiceBase
	{
		private MuninNode _muninNode;

		public MuninService()
		{
			ServiceName = "munin-node Service";
			EventLog.Log = "Application";
			EventLog.Source = ServiceName;
			CanHandlePowerEvent = true;
			CanHandleSessionChangeEvent = true;
			CanPauseAndContinue = true;
			CanShutdown = true;
			CanStop = true;
		}

		static void Main()
		{
			Run(new MuninService());
		}

		/// <summary>
		/// Cleanup code goes here
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (_muninNode != null)
				_muninNode.Dispose();

			base.Dispose(disposing);
		}

		/// <summary>
		/// Logs an entry to the EventLog
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type"></param>
		internal void Log(string message, EventLogEntryType type)
		{
			EventLog.WriteEntry(message, type);
		}

		/// <summary>
		/// Startup code goes here
		/// </summary>
		/// <param name="args"></param>
		protected override void OnStart(string[] args)
		{
			ExitCode = 0;
			base.OnStart(args);

			_muninNode = new MuninNode(this);

			try
			{
				System.Threading.Tasks.Task.Factory.StartNew(_muninNode.Start);
			}
			catch (Exception e)
			{
				EventLog.WriteEntry(String.Format("Exception occured during startup: {0}", e.Message), EventLogEntryType.Error);
				_muninNode = null;
				ExitCode = -1;
				Stop();
			}

		}

		/// <summary>
		/// Stop code goes here
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop();
			if (_muninNode != null)
			{
				_muninNode.Stop();
			}
			_muninNode = null;
		}

		/// <summary>
		/// Pause code goes here
		/// </summary>
		protected override void OnPause()
		{
			base.OnPause();
			if (_muninNode != null)
			{
				_muninNode.Stop();
			}
		}

		/// <summary>
		/// Continue code goes here
		/// </summary>
		protected override void OnContinue()
		{
			base.OnContinue();
			try
			{
				_muninNode.Start();
			}
			catch (Exception e)
			{
				EventLog.WriteEntry(String.Format("Exception occured during startup: {0}", e.Message), EventLogEntryType.Error);
				_muninNode = null;
				ExitCode = -1;
				Stop();
			}
		}

		/// <summary>
		/// Shutdown code goes here
		/// </summary>
		protected override void OnShutdown()
		{
			base.OnShutdown();
			if (_muninNode != null)
			{
				_muninNode.Stop();
			}
			_muninNode = null;
		}
	}
}
