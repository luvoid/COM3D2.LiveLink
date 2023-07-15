using System;
using System.ComponentModel.Design;
using COM3D2.LiveLink.CLI.Commands;

namespace COM3D2.LiveLink.CLI
{
	public class Program
	{
		public string Agent
		{
			get
			{
				if (Core.IsServer)
				{
					return $"server@{Core.Address}";
				}
				else if (Core.IsClient)
				{
					return $"client@{Core.Address}";
				}
				else
				{
					return "unknown";
				}
			}
		}

		public LiveLinkCore Core;
		
		public Command[] Commands;

		public bool KeepRunning = true;

		public Program() : this(new LiveLinkCore()) { }

		public Program(LiveLinkCore core)
		{
			Core = core;

			Commands = new Command[]
			{
				new HelpCommand(),

				new StartCommand(Core),

				new SendCommand(Core),

				new WaitFor(Core),

				new LambdaCommand("readall", () => Console.WriteLine($"ReadAll - - - - - -\n{Core.ReadAll()}\n- - - - - - - - - -")),

				new LambdaCommand("flush", () => Core.Flush()),

				new LambdaCommand("isconnected", () =>
				{
					Console.WriteLine(Core.IsConnected);
				}),

				new LambdaCommand("disconnect", () =>
				{
					Console.WriteLine("Disconnecting LiveLink...");
					Core.Disconnect();
					Console.WriteLine("Disconnected.");
				}),

				new LambdaCommand("exit", () =>
				{
					Console.WriteLine("Exiting COM3D2 LiveLink CLI");
					Environment.Exit(0);
				}),
			};
			(Commands[0] as HelpCommand).Commands = Commands;
		}

		public static void Main(string[] args)
		{
			Console.WriteLine("LiveLink CLI");                   // These MUST be here otherwise the test cases will block indefinately.
			Console.Error.WriteLine("LiveLink CLI Version 0.1"); // This is due to a .NET Framework bug: https://stackoverflow.com/a/6689406/2642204

			Program program = new Program();

			DebugConsole.IsDebugEnabled = Parse.NamedFlag(args, "--debug");
			(new StartCommand(program.Core) { IsSilent = true }).Run(args);

			program.Run();
		}
	
		public void Run()
		{
			while (KeepRunning)
			{
				Console.Write($"LL {Agent}> ");
				string commandline = Console.ReadLine();
				if (DebugConsole.IsDebugEnabled) Console.WriteLine(commandline);
				bool matched = false;
				foreach (var command in Commands)
				{
					if (command.Match(commandline))
					{
						command.Run(commandline);
						matched = true;
						break;
					}
				}
				if (!matched)
				{
					Console.Error.WriteLine($"Unknown command: {commandline}");
				}
			}
		}
	}
}
