using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COM3D2.LiveLink.Plugin.Tests;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;
using HarmonyLib;
using COM3D2.LiveLink.Plugin;

namespace COM3D2.LiveLink.Tests
{
	[TestClass]
	public class TestPlugin : LiveLinkTestBase
	{
		public const string COM3D2_ROOT = @"C:\DJN\KISS\COM3D2\";
		public const string COM3D2_EXE_PATH = $@"{COM3D2_ROOT}\COM3D2x64.exe";
		public const string PLUGIN_OUT_PATH = $@"{COM3D2_ROOT}\BepInEx\{PluginTests.STD_OUT_FILE}";
		public const string PLUGIN_ERR_PATH = $@"{COM3D2_ROOT}\BepInEx\{PluginTests.STD_ERR_FILE}";


		public Process CreateCOM3D2Process(string commands, bool quit = true)
		{
			if (quit)
			{
				commands += " -livelinkquit";
			}
			commands += " -logfile";

			Process client = new Process();
			client.StartInfo = new ProcessStartInfo(COM3D2_EXE_PATH, commands);
			client.StartInfo.UseShellExecute = false;
			client.StartInfo.RedirectStandardInput = true;

			// The game redirects all logging, so there's no need for this
			client.StartInfo.RedirectStandardOutput = false; 
			client.StartInfo.RedirectStandardError = false;

			client.Start();
			//client.WaitForInputIdle();
			return client;
		}

		public void WaitForConnectionOrFail(LiveLinkCore serverCore, Process game)
		{
			while (!serverCore.WaitForConnection(10))
			{
				if (game.HasExited)
				{
					Assert.That.ExitZero(game, PLUGIN_OUT_PATH, PLUGIN_ERR_PATH);
					throw new AssertFailedException("The process exited without connecting to the server");
				}
			}
		}

		[TestMethod]
		public void GameCLI()
		{
			LiveLinkCore serverCore = CreateServer();
			Process game = CreateCOM3D2Process($"-livelink:{serverCore.Address} -livelinkcli");
			WaitForConnectionOrFail(serverCore, game);

			game.StandardInput.WriteLine("help");

			game.StandardInput.WriteLine("exit");

			serverCore.Dispose();
			Assert.That.ExitZero(game, PLUGIN_OUT_PATH, PLUGIN_ERR_PATH);
		}

		[TestMethod]
		public void RecieveString()
		{
			LiveLinkCore serverCore = CreateServer();
			Process game = CreateCOM3D2Process($"-livelink:{serverCore.Address} -livelinkcli");
			WaitForConnectionOrFail(serverCore, game);

			serverCore.SendString("Hello COM3D2!");

			game.StandardInput.WriteLine("readall");

			game.StandardInput.WriteLine("exit");

			serverCore.Dispose();
			Assert.That.ExitZero(game, PLUGIN_OUT_PATH, PLUGIN_ERR_PATH);
		}

		[TestMethod]
		public void RecieveFile()
		{
			LiveLinkCore serverCore = CreateServer();
			Process game = CreateCOM3D2Process($"-livelink:{serverCore.Address} -livelinkcli");
			WaitForConnectionOrFail(serverCore, game);

			serverCore.SendBytes(File.ReadAllBytes("Resources/T-Pose.anm"));
			serverCore.Flush();

			game.StandardInput.WriteLine("readall");

			game.StandardInput.WriteLine("exit");

			serverCore.Dispose();
			Assert.That.ExitZero(game, PLUGIN_OUT_PATH, PLUGIN_ERR_PATH);
		}

		[TestMethod]
		public void PluginInitialize()
		{
			Process game = CreateCOM3D2Process($"-livelinktest:{nameof(PluginTests.TestInitialize)}");
			Assert.That.ExitZero(game, PLUGIN_OUT_PATH, PLUGIN_ERR_PATH);
		}

		[TestMethod]
		public void HandleMessage()
		{
			Process game = CreateCOM3D2Process($"-livelinktest:{nameof(PluginTests.TestHandleMessage)}");
			Assert.That.ExitZero(game, PLUGIN_OUT_PATH, PLUGIN_ERR_PATH);
		}

		[TestMethod]
		public void RecieveModel()
		{
			LiveLinkCore serverCore = CreateServer();
			Process game = CreateCOM3D2Process($"-livelink:{serverCore.Address} -livelinktest:{nameof(PluginTests.TestRecieveMessageWithMaid)}", quit: true);
			WaitForConnectionOrFail(serverCore, game);

			serverCore.SendBytes(File.ReadAllBytes("Resources/body001.model"));

			// Do it twice to test the refresh as well
			serverCore.SendBytes(File.ReadAllBytes("Resources/body001.model"));

			Assert.That.ExitZero(game, PLUGIN_OUT_PATH, PLUGIN_ERR_PATH);
			//Assert.That.ExitZero(game, -1073741510, PLUGIN_OUT_PATH, PLUGIN_ERR_PATH);
		}

		[TestMethod]
		public void TestImportCMPatch()
		{
			Harmony harmony = new(nameof(TestImportCMPatch));
			//Console.WriteLine(ImportCMExtensions.isV3);
			ImportCMExtensions.Patch(harmony);
		}
	}
}
