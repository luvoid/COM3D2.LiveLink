using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COM3D2.LiveLink.Plugin.Tests;

namespace COM3D2.LiveLink.Tests
{
	[TestClass]
	public class TestPlugin : LiveLinkTestBase
	{
		public const string COM3D2_EXE_PATH = "C:\\DJN\\KISS\\COM3D2\\COM3D2x64.exe";

		public Process CreateCOM3D2Process(string commands, bool quit = true)
		{
			if (quit)
			{
				commands += " -livelinkquit";
			}

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

		[TestMethod]
		public void TestGameCLI()
		{
			LiveLinkCore serverCore = CreateServer();
			Process game = CreateCOM3D2Process($"-livelink:{serverCore.Address}");
			serverCore.WaitForConnection();

			game.StandardInput.WriteLine("help");

			game.StandardInput.WriteLine("exit");

			serverCore.Dispose();
			Assert.That.ExitZero(game);
		}

		[TestMethod]
		public void TestRecieveString()
		{
			LiveLinkCore serverCore = CreateServer();
			Process game = CreateCOM3D2Process($"-livelink:{serverCore.Address}");
			serverCore.WaitForConnection();

			serverCore.SendString("Hello COM3D2!");

			game.StandardInput.WriteLine("readall");

			game.StandardInput.WriteLine("exit");

			serverCore.Dispose();
			Assert.That.ExitZero(game);
		}

		[TestMethod]
		public void TestRecieveFile()
		{
			LiveLinkCore serverCore = CreateServer();
			Process game = CreateCOM3D2Process($"-livelink:{serverCore.Address}");
			serverCore.WaitForConnection();

			byte[] fileBytes;
			using (var fileStream = new FileStream("Resources/T-Pose.anm", FileMode.Open, FileAccess.Read))
			{
				fileBytes = new byte[(int)fileStream.Length];
				fileStream.Read(fileBytes, 0, fileBytes.Length);
			}
			serverCore.SendBytes(fileBytes);
			serverCore.Flush();

			game.StandardInput.WriteLine("readall");

			game.StandardInput.WriteLine("exit");

			serverCore.Dispose();
			Assert.That.ExitZero(game);
		}

		[TestMethod]
		public void TestHandleMessage()
		{
			LiveLinkCore serverCore = CreateServer();
			Process game = CreateCOM3D2Process($"-livelinktest:{nameof(PluginTests.TestHandleMessage)}");
			Assert.That.ExitZero(game);
		}

	}
}
