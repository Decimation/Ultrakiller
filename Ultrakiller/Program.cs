using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Novus.Memory;
using Novus.Runtime;
using Novus.Win32;
using Novus.Win32.Structures;
using SimpleCore.Utilities;

namespace Ultrakiller
{
	class Program
	{
		static void Main(string[] args)
		{

			var p = Process.GetProcessesByName("ULTRAKILL");

			foreach (var process in p) {
				Console.WriteLine(process.ProcessName);
			}

			var ultrakill = p[0];

			List<ProcessModule> modules =
				ultrakill.Modules.Cast<ProcessModule>().OrderBy(module => module.BaseAddress).ToList();

			foreach (var module in modules) {
				Console.WriteLine(
					$"{module.ModuleName} {module.BaseAddress:X} {module.EntryPointAddress:X} {module.ModuleMemorySize}");
			}

			SystemInfo systemInformation = default;
			Native.GetSystemInfo(ref systemInformation);

			MemoryBasicInformation m = default;

			var lpMem = 0L;

			Console.WriteLine(systemInformation.lpMaximumApplicationAddress);

			var sw = Stopwatch.StartNew();

			while (lpMem < systemInformation.lpMaximumApplicationAddress.ToInt64()) {

				int result = Native.VirtualQueryEx(ultrakill.Handle, (IntPtr) lpMem, ref m,
					(uint) Marshal.SizeOf(typeof(MemoryBasicInformation)));

				if (m.State == AllocationType.Commit &&
				    (m.Type == TypeEnum.MEM_MAPPED || m.Type == TypeEnum.MEM_PRIVATE)) {

					var rg1 = new byte[m.RegionSize.ToInt64()];

					var ok = Native.ReadProcessMemory(ultrakill.Handle, m.BaseAddress, rg1, m.RegionSize,
						out var result1);

					Debug.WriteLine(
						$"{m.BaseAddress:X} read {ok} {result1.ToInt64()} {m.AllocationProtect} {m.Protect} {m.Protect} {m.State}");


					var ss = new SigScanner(m.BaseAddress, (ulong) m.RegionSize, rg1);


					//var addr = ss.FindSignature("1F 18 0A 02 7B 51 0C 00 04 22 00 00 40 3F 22 9A 99 59 3F 28 E2 00 00 0A 6F E3 00 00 0A 2B 1C");

					//0x48170
					//seg000:48170
					var addr = ss.FindSignature("02 16 7D 62 0C 00 04 1F 0C");

					if (!addr.IsNull) {
						Console.WriteLine($"!!! {addr}");

						//var line = addr + 0x54;

						//481a5


						var line = addr + 0x35;
						var ofs  = (addr - m.BaseAddress).ToInt32();

						Console.WriteLine($"{ofs:X}");

						var body = rg1.Skip(ofs).Take(0x768).ToList();
						var ops=InspectIL.GetInstructions(body.ToArray());
						Console.WriteLine(ops.Length);
						foreach (var op in ops) {
							Console.WriteLine(op);
						}
						//22 00 00 C0 3F
						body.ReplaceAllSequences(new List<byte>() {0x22, 00, 00, 0xC0, 0x3F},
							new List<byte>() {0x22, 00, 00, 0x20, 0x41});


						body[0x7  + 1] = 50;
						body[0x54 + 1] = 100;
						body[0x35 + 1] = 127;

						//Console.WriteLine($"{body[^1]:X}");

						//Mem.WriteProcessMemory(ultrakill, line + 1, new byte[] {127});

						Mem.WriteProcessMemory(ultrakill, addr, body.ToArray());

						//0x48511
						//var line2 = addr + 0x70;
						//3a1
						//(line2+1).WriteAll(BitConverter.GetBytes(3f));

						//0x488d7

						Console.WriteLine("written");
					}


				}

				if (lpMem == (long) m.BaseAddress + (long) m.RegionSize)
					break;

				lpMem = (long) m.BaseAddress + (long) m.RegionSize;

			}

			sw.Stop();
			Console.WriteLine($"{sw.Elapsed.Seconds} sec");
		}
	}
}