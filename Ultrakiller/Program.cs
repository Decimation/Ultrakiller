using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Novus.Memory;
using Novus.Win32;
using Novus.Win32.Structures;

namespace Ultrakiller
{
	class Program
	{
		static void Main(string[] args)
		{
			//\x02\x16\x7D\x62\x0C\x00\x04\x1F\x0C

			var p = Process.GetProcessesByName("ULTRAKILL");

			foreach (var process in p) {
				Console.WriteLine(process.ProcessName);
			}

			var ultrakill  = p[0];
			var moduleName = ultrakill.MainModule;

			foreach (ProcessModule module in ultrakill.Modules) {
				Console.WriteLine($"{module.ModuleName}");

				if (module.ModuleName.Contains("UnityPlayer")) {
					moduleName = module;
					break;
				}
			}


			SystemInfo systemInformation = default;
			Native.GetSystemInfo(ref systemInformation);

			MemoryBasicInformation m = default;

			var lpMem = 0L;

			Console.WriteLine(systemInformation.lpMaximumApplicationAddress);

			while (lpMem < systemInformation.lpMaximumApplicationAddress.ToInt64()) {


				int result = Native.VirtualQueryEx(ultrakill.Handle, (IntPtr) lpMem, ref m,
					(uint) Marshal.SizeOf(typeof(MemoryBasicInformation)));

				if (m.State == AllocationType.Commit) {
					//Console.WriteLine("{0:X}-{1:X} : {2} bytes result={3} | {4} {5}", m.BaseAddress,
					//	(ulong)m.BaseAddress + (ulong)m.RegionSize - 1, m.RegionSize, result, m.Protect, m.AllocationProtect);

					var rg1 = new byte[m.RegionSize.ToInt64()];

					var ok = Native.ReadProcessMemory(ultrakill.Handle, m.BaseAddress, rg1, m.RegionSize,
						out var result1);

					Debug.WriteLine(
						$"{m.BaseAddress:X} read {ok} {result1.ToInt64()} {m.AllocationProtect} {m.Protect} {m.Protect} {m.State}");


					var ss = new SigScanner(m.BaseAddress, (ulong) m.RegionSize, rg1);

					//var addr = ss.FindSignature("1F 18 0A 02 7B 51 0C 00 04 22 00 00 40 3F 22 9A 99 59 3F 28 E2 00 00 0A 6F E3 00 00 0A 2B 1C");
					var addr = ss.FindSignature("02 16 7D 62 0C 00 04 1F 0C");

					if (!addr.IsNull) {
						Console.WriteLine($"!!! {addr}");
					}

					//var line = addr + 0x54;

					var line = addr + 0x35;
					//1F

					Mem.WriteProcessMemory(ultrakill, line + 1, new byte[] {127});

					//0x76
					//for (long i = addr.ToInt64(); i < (addr.ToInt64() + 0x76) - 4; i++)
					//{
					//	// if (rg1[i]==0x22&&rg1.Skip(i).Take(4).SequenceEqual(new byte[] {  0x00, 0x00, 0x00, 0x40 })) {
					//	// 	Console.WriteLine(i);
					//	// 	Mem.WriteProcessMemory(ultrakill, m.BaseAddress+i, new byte[] { 0x22,0x00, 0x00, 0x00, 0x00 });
					//	//
					//	// }
					//	//if (rg1[i] == 0x22 && rg1[i+4]==0x40)
					//	//{
					//	//	Console.WriteLine(i);
					//	//	Mem.WriteProcessMemory(ultrakill, (Pointer<byte>) (m.BaseAddress.ToInt64() + i +4), new byte[] {0x00 });

					//	//}
					//}
				}


				//var sig  = new SigScanner(m.BaseAddress, m.RegionSize);
				//var addr = sig.FindSignature("02 16 7D 62 0C 00 04 1F 0C");

				if (lpMem == (long) m.BaseAddress + (long) m.RegionSize)
					break;

				lpMem = (long) m.BaseAddress + (long) m.RegionSize;

			}


			// Console.WriteLine(moduleName.ModuleName+$" {moduleName.BaseAddress:F} ");
			//
			// var sig = new SigScanner(ultrakill,moduleName);
			//
			// //1F 18 0A 02 7B 51 0C 00 04 22 00 00 40 3F 22 9A 99 59 3F 28 E2 00 00 0A 6F E3 00 00 0A 2B 1C
			// var addr = sig.FindSignature("02 16 7D 62 0C 00 04 1F 0C");
			//
			// Console.WriteLine(addr);
		}
	}
}