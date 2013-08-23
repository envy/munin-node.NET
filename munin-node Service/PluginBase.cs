using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace munin_node_Service
{
	public abstract class PluginBase
	{
		[DllImport("pdh.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern UInt32 PdhLookupPerfNameByIndex(string szMachineName, uint dwNameIndex, System.Text.StringBuilder szNameBuffer, ref uint pcchNameBufferSize);

		public enum PerformanceCounterNames
		{
			_1847 = 1,
			System = 2,
			Memory = 4,
			PercentProcessorTime = 6,
			FileReadOperationsPerSec = 10,
			FileWriteOperationsPerSec = 12,
			FileControlOperationsPerSec = 14,
			FileReadBytesPerSec = 16,
			FileWriteBytesPerSec = 18,
			FileControlBytesPerSec = 20,
			AvailableBytes = 24,
			CommittedBytes = 26,
			PageFaultsPerSec = 28,
			CommitLimit = 30,
			WriteCopiesPerSec = 32,
			TransitionFaultsPerSec = 34,
			CacheFaultsPerSec = 36,

			SystemUpTime = 674,
		}

		/// <summary>
		/// Helper method to get localized performance counter values.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public string GetPerformanceCounterNameById(PerformanceCounterNames id)
		{
			var stringBuilder = new StringBuilder(1024);
			var pcchNameBufferSize = (uint) stringBuilder.Capacity;
			PdhLookupPerfNameByIndex(null, (uint)id, stringBuilder, ref pcchNameBufferSize);
			return stringBuilder.ToString();
		}

		public PerformanceCounter GetPerformanceCounter(PerformanceCounterNames categoryName,
		                                                PerformanceCounterNames counterName)
		{
			return new PerformanceCounter(GetPerformanceCounterNameById(categoryName), GetPerformanceCounterNameById(counterName));
		}

		public PerformanceCounter GetPerformanceCounter(PerformanceCounterNames categoryName,
														PerformanceCounterNames counterName,
														PerformanceCounterNames instanceName)
		{
			return new PerformanceCounter(GetPerformanceCounterNameById(categoryName), GetPerformanceCounterNameById(counterName), GetPerformanceCounterNameById(instanceName));
		}

		public abstract void Initialize();
		public abstract string GetConfig();
		public abstract string GetValues();
		public abstract string GetName();
	}
}
