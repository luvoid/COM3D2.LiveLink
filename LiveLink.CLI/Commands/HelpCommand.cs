using System;
using System.Collections.Generic;

namespace COM3D2.LiveLink.CLI.Commands
{
	internal class HelpCommand : Command
	{
		public override string Name => "help";

		public IEnumerable<Command> Commands;

		public HelpCommand()
		{
			Commands = null;
		}

		public HelpCommand(IEnumerable<Command> commands)
		{
			Commands = commands;
		}

		public override int Run(in string[] args)
		{
			foreach (var command in Commands)
			{
				if (command != null)
				{
					Console.WriteLine(command.Name);
				}
			}

			return 0;
		}
	}
}
