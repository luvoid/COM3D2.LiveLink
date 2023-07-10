using System;
using System.Linq;

namespace COM3D2.LiveLink.CLI.Commands
{
	internal class WaitFor : Command
	{
		public override string Name => "waitfor";

		private LiveLinkCore m_Core;

		public WaitFor(LiveLinkCore core)
		{
			m_Core = core;
		}

		public override int Run(in string[] args)
		{
			if (!Parse.NamedInt(args, out int timeout, "--time", "-t"))
			{
				timeout = 1000;
			}

			if (Parse.NamedFlag(args, "--connection", "-c"))
			{
				try
				{
					m_Core.WaitForConnection(timeout);
				}
				catch (Exception e)
				{
					ErrorConsole.WriteLine(e.Message);
				}
			}

			else
			{
				System.Threading.Thread.Sleep(timeout);
				return 1;
			}

			return 0;
		}
	}
}
