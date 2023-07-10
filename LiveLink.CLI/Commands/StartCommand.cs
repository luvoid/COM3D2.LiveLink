using System;

namespace COM3D2.LiveLink.CLI.Commands
{
	internal class StartCommand : Command
	{
		public override string Name => "start";

		private LiveLinkCore m_Core;
		public bool Silent = false;

		public StartCommand(LiveLinkCore core)
		{
			m_Core = core;
		}

		public override int Run(in string[] args)
		{
			if (Parse.NamedString(args, out string address, "--server", "-s"))
			{
				m_Core.StartServer(address);
				Console.WriteLine($"Started server at address {m_Core.Address}");
			}
			else if (Parse.NamedString(args, out address, "--client", "-c"))
			{
				Console.WriteLine($"Connecting to address {address}");
				m_Core.StartClient(address);
			}
			else
			{
				ErrorConsole.WriteLine($"Usage: {Name} (--server OR --client) address");
				return 1;
			}

			return 0;
		}
	}
}
