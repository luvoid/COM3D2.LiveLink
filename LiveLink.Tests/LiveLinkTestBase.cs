using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COM3D2.LiveLink.Tests
{
	public abstract class LiveLinkTestBase
	{
		private static int addressCounter = 0;
		public static string GetProcessUniqueAddress(string baseAddress = "com3d2.livelink")
		{
			return $"{baseAddress}.{Process.GetCurrentProcess().Id}.{addressCounter++}";
		}

		public static LiveLinkCore CreateServer()
		{
			string address = GetProcessUniqueAddress();
			LiveLinkCore serverCore = new LiveLinkCore();
			serverCore.StartServer(address);
			//Console.WriteLine($"address = {serverCore.Address}");
			return serverCore;
		}

	}
}
