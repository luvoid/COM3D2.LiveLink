using BepInEx.Logging;
using COM3D2.LiveLink.CLI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin.Tests
{
	public static class PluginTests
	{
		static LiveLinkPlugin Plugin => LiveLinkPlugin.Instance;
		static ManualLogSource Logger => LiveLinkPlugin.Logger;

		public static void RunTestsInCommandline()
		{
			string[] commandLineArgs = System.Environment.GetCommandLineArgs();
			foreach (string arg in commandLineArgs)
			{
				if (arg.StartsWith("-livelink:"))
				{
					string address = arg.Replace("-livelink:", "");
					RunCLI(address);
					break;
				}
				else if (arg == $"-livelinktest:{nameof(TestHandleMessage)}")
				{
					TestHandleMessage();
				}
				else if (arg == "-livelinkquit")
				{
					Application.Quit();
				}
			}
		}
			
		private static void RunCLI(string address)
		{
			LiveLinkCore core = new LiveLinkCore();

			CLI.Program cliProgram = new CLI.Program(core);
			CLI.DebugConsole.IsDebugEnabled = true;

			Logger.LogMessage($"Connecting LiveLink client to {address}");
			core.StartClient(address);

			cliProgram.Commands[6] = new LambdaCommand("exit", () =>
			{
				Logger.LogInfo("Exiting LiveLink CLI. The game will continue running.");
				cliProgram.KeepRunning = false;
				core.Disconnect();
			});

			cliProgram.Run();
		}
	
		public static void TestHandleMessage()
		{
			byte[] testMessage =
			{
				12,
				(byte)'H', (byte)'E', (byte)'L', (byte)'L', (byte)'O', (byte)' ', (byte)'W', (byte)'O', (byte)'R', (byte)'L', (byte)'D', (byte)'!'
			};
			Plugin.HandleMessage(new System.IO.MemoryStream(testMessage));
		}
	}
}
