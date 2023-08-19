using BepInEx.Logging;
using COM3D2.LiveLink.CLI.Commands;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin.Tests
{
	public static class PluginTests
	{
		public const string STD_OUT_FILE = $"{PluginInfo.PLUGIN_GUID}.Tests.out.log";
		public const string STD_ERR_FILE = $"{PluginInfo.PLUGIN_GUID}.Tests.err.log";

		static LiveLinkPlugin Plugin => LiveLinkPlugin.Instance;
		static ManualLogSource Logger => LiveLinkPlugin.Logger;

		internal static PluginTestLogListener TestLogListener;

		internal static string TestAddress = null;

		private static bool allowQuit = false;

		/// <summary>
		/// Check the commandline for test commands, and run them if found.
		/// </summary>
		/// <returns>
		/// Returns true if tests were run and the plugin should not be initalized.
		/// </returns>
		public static bool RunTestsInCommandline()
		{
			TestLogListener = new PluginTestLogListener();
			bool shouldNotInitalize = false;
			string[] commandLineArgs = System.Environment.GetCommandLineArgs();
			foreach (string arg in commandLineArgs)
			{
				if (arg.StartsWith("-livelink:"))
				{
					TestAddress = arg.Substring("-livelink:".Length);
				}
				else if (arg.StartsWith("-livelinkcli"))
				{
					RunCLI(TestAddress);
					shouldNotInitalize = true;
				}
				else if (arg.StartsWith("-livelinktest:"))
				{
					try
					{
						string methodName = arg.Substring("-livelinktest:".Length);
						if (!methodName.StartsWith("Test"))
							throw new System.InvalidOperationException("Invoked test methods must start with \"Test\"");
						var method = typeof(PluginTests).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
						if (method == null)
							throw new System.InvalidOperationException($"Cannot find test method \"{methodName}\"");
						method.Invoke(null, null);
					}
					catch (System.Exception e)
					{
						Logger.LogError(e);
						TestLogListener.Dispose();
						Process.GetCurrentProcess().Kill();
					}
					shouldNotInitalize = true;
				}
				else if (arg == "-livelinkquit")
				{
					allowQuit = true;
					SafeExitAfterTests();
					break;
				}
			}
			TestLogListener.Enabled = false;
			return shouldNotInitalize;
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
		

		internal static void SafeExit(int exitCode = 0)
		{
			if (!allowQuit) return;
			var helper = Plugin.gameObject.AddComponent<SafeExitHelper>();
			helper.ExitCode = exitCode;
		}

		private static bool isWaitingForTests = false;
		internal static void SafeExitAfterTests(int exitCode = 0)
		{
			if (!allowQuit) return;

			// If there is more than one SafeExitHelper waiting for tests
			// then they will both be waiting forever on the other to finish.
			if (isWaitingForTests) return;
			isWaitingForTests = true;

			var helper = Plugin.gameObject.AddComponent<SafeExitHelper>();
			helper.ExitCode = exitCode;
			helper.WaitForTests = true;
		}


		public static void TestHandleMessage()
		{
			byte[] testMessage =
			{
				12,
				(byte)'H', (byte)'E', (byte)'L', (byte)'L', (byte)'O', (byte)' ', (byte)'W', (byte)'O', (byte)'R', (byte)'L', (byte)'D', (byte)'!'
			};
			Plugin.HandleMessage(new MemoryStream(testMessage));
		}


		public static void TestInitialize()
		{
			Plugin.Initialize();
		}


		public static void TestRecieveMessage()
		{
			Plugin.gameObject.AddComponent<TestRecieveMessageHelper>();
		}
	}
}
