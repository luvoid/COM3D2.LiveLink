using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace COM3D2.LiveLink.Tests
{
	[TestClass]
	public class TestCore : LiveLinkTestBase
	{
		public const string RELATIVE_CLI_PATH = "COM3D2.LiveLink.CLI.exe";

		public string CLIPath => Path.GetFullPath(Environment.CurrentDirectory + '/' + RELATIVE_CLI_PATH);

		public Process CreateServerProcess(out string address)
		{
			Process server = new Process();
			address = GetProcessUniqueAddress();
			server.StartInfo = new ProcessStartInfo(CLIPath, $"--server {address} --debug");
			server.StartInfo.UseShellExecute = false;
			server.StartInfo.RedirectStandardInput = true;
			server.StartInfo.RedirectStandardOutput = true;
			server.StartInfo.RedirectStandardError = true;
			server.Start();

			return server;

			const string ADDRESS_LABEL = "Started server at address ";
			int maxLines = 10;
			Console.WriteLine("Server Output - - - - - - - - -");
			while (!server.HasExited && maxLines > 0)
			{
				string line = server.StandardOutput.ReadLine();
				Console.WriteLine(line);
				if (line != null && line.StartsWith(ADDRESS_LABEL))
				{
					address = line.Replace(ADDRESS_LABEL, "");
					break;
				}
				maxLines--;
			}
			Console.WriteLine("- - - - - - - - - - - - - - - -");

			return server;
		}

		public Process CreateClientProcess(string pipeHandle)
		{
			Process client = new Process();
			client.StartInfo = new ProcessStartInfo(CLIPath, $"--client {pipeHandle} --debug");
			client.StartInfo.UseShellExecute = false;
			client.StartInfo.RedirectStandardInput = true;
			client.StartInfo.RedirectStandardOutput = true;
			client.StartInfo.RedirectStandardError = true;
			client.Start();
			//client.WaitForInputIdle();
			return client;
		}
		
		public int StopCLIProcess(Process cliProcess)
		{
			//cliProcess.WaitForInputIdle();
			cliProcess.StandardInput.WriteLine("exit");
			cliProcess.WaitForExit(5000);
			if (!cliProcess.HasExited)
			{
				cliProcess.Kill();
			}
			return cliProcess.ExitCode;
		}

		public void StartClientCoreAndServerCLI(out LiveLinkCore clientCore, out Process serverCLI, out string address)
		{
			serverCLI = CreateServerProcess(out address);
			Console.WriteLine($"address = {address}");

			clientCore = new LiveLinkCore();
			Assert.IsTrue(clientCore.StartClient(address), "Failed to connect client to server.");

			serverCLI.StandardInput.WriteLine($"waitfor --connection");// --time 1");
			Assert.IsTrue(clientCore.IsConnected);
		}

		public void StartClientCoreAndServerCLI(out LiveLinkCore clientCore, out Process serverCLI)
		{
			StartClientCoreAndServerCLI(out clientCore, out serverCLI, out string address);
		}

		public void StartServerCoreAndClientCLI(out LiveLinkCore serverCore, out Process clientCLI)
		{
			serverCore = CreateServer();
			clientCLI = CreateClientProcess(serverCore.Address);

			Assert.IsTrue(serverCore.WaitForConnection(1000), "Client did not connect to server.");
		}

		[TestMethod]
		public void TestClientCLI()
		{
			LiveLinkCore serverCore = CreateServer();
			Process clientCLI = CreateClientProcess(serverCore.Address);

			string testString = "Hello World!";
			string output = null;
			int exitCode = -1;
			try
			{
				serverCore.WaitForConnection(1000);
				serverCore.SendString(testString);
				serverCore.Flush();

				//System.Threading.Thread.Sleep(1000);

				clientCLI.StandardInput.WriteLine("readall");

				clientCLI.StandardInput.WriteLine("help");

				exitCode = StopCLIProcess(clientCLI);
				serverCore.Dispose();
			}
			finally  
			{
				output = clientCLI.StandardOutput.ReadAllAvailable();
				Console.WriteLine("Client Output - - - - - - - - -");
				Console.WriteLine(output);
				Console.WriteLine(clientCLI.StandardError.ReadAllAvailable());
				Console.WriteLine("- - - - - - - - - - - - - - - -");
			}
			
			Assert.That.ExitZero(clientCLI);
			AssertContains(output, testString);
		}

		[TestMethod]
		public void TestServerCLI()
		{
			StartClientCoreAndServerCLI(out LiveLinkCore clientCore, out Process serverCLI);

			string testString = "Hello World!";
			serverCLI.StandardInput.WriteLine($"send --string {testString}");
			serverCLI.StandardInput.WriteLine($"flush");

			Thread.Sleep(100);

			string recieved = clientCore.ReadString();

			//clientCore.Dispose();
			int exitCode = StopCLIProcess(serverCLI);
			Console.WriteLine("Server Output - - - - - - - - -");
			Console.Write(serverCLI.StandardOutput.ReadAllAvailable());
			Debug.Write(serverCLI.StandardError.ReadAllAvailable());
			Console.WriteLine("- - - - - - - - - - - - - - - -");
			Assert.That.ExitZero(serverCLI);

			Assert.IsTrue(recieved == testString, $"recieved \"{recieved}\" != testString \"{testString}\"");
		}

		[TestMethod]
		public void TestMessageLength()
		{
			StartClientCoreAndServerCLI(out LiveLinkCore clientCore, out Process serverCLI);

			string testString = "Hello World!";
			serverCLI.StandardInput.WriteLine($"send --string {testString}");
			//serverCLI.StandardInput.WriteLine($"flush");

			System.Threading.Thread.Sleep(100);

			string recieved = clientCore.ReadAll();

			clientCore.Dispose();
			int exitCode = StopCLIProcess(serverCLI);
			Console.WriteLine("Server Output - - - - - - - - -");
			Console.Write(serverCLI.StandardOutput.ReadAllAvailable());
			Debug.Write(serverCLI.StandardError.ReadAllAvailable());
			Console.WriteLine("- - - - - - - - - - - - - - - -");
			Assert.That.ExitZero(serverCLI);


			Assert.IsTrue(recieved.Length == testString.Length + 1, $"recieved \"{recieved}\" != testString \"\fHello World!\"");
		}

		[TestMethod]
		public void TestClientNoReadBlock()
		{
			LiveLinkCore serverCore = CreateServer();
			Process clientCLI = CreateClientProcess(serverCore.Address);

			string testString = "Hello World!";
			string firstOutput = "";
			string output = null;
			int exitCode = -1;
			try
			{
				serverCore.WaitForConnection();

				clientCLI.StandardInput.WriteLine("readall");

				System.Threading.Thread.Sleep(10);

				//firstOutput = clientCLI.StandardOutput.ReadAllAvailable();

				serverCore.SendString(testString);
				serverCore.Flush();

				System.Threading.Thread.Sleep(10);

				clientCLI.StandardInput.WriteLine("readall");

				exitCode = StopCLIProcess(clientCLI);
				serverCore.Dispose();
			}
			finally
			{
				output = firstOutput + clientCLI.StandardOutput.ReadAllAvailable();
				Console.WriteLine("Client Output - - - - - - - - -");
				Console.WriteLine(output);
				Console.WriteLine(clientCLI.StandardError.ReadAllAvailable());
				Console.WriteLine("- - - - - - - - - - - - - - - -");
			}

			Assert.That.ExitZero(clientCLI);
			Assert.IsTrue(output.Contains("ReadAll - - - - - -\n\n- - - - - - - - - -"), "Client thread is blocked");
			AssertContains(output, testString);
		}

		[TestMethod]
		public void TestServerWaitForConnectionTimeout()
		{
			LiveLinkCore serverCore = CreateServer();

			var task = Task.Run(() => serverCore.WaitForConnection(10));
			Assert.IsTrue(task.Wait(20));
		}

		[TestMethod]
		public void TestServerListenForConnection()
		{
			LiveLinkCore serverCore = CreateServer();
			Process clientCLI = CreateClientProcess(serverCore.Address);

			Thread.Sleep(1000);

			try
			{
				Assert.IsTrue(serverCore.IsConnected, "serverCore.IsConnected = false");
			}
			finally
			{
				StopCLIProcess(clientCLI);
			}
		}


		[TestMethod]
		public void TestSendFile()
		{
			LiveLinkCore serverCore = CreateServer();
			Process clientCLI = CreateClientProcess(serverCore.Address);

			byte[] fileBytes;
			using (var fileStream = new FileStream("Resources/T-Pose.anm", FileMode.Open, FileAccess.Read))
			{
				fileBytes = new byte[(int)fileStream.Length];
				fileStream.Read(fileBytes, 0, fileBytes.Length);
			}

			int exitCode = -1;
			try
			{
				serverCore.WaitForConnection();

				serverCore.SendBytes(fileBytes);
				serverCore.Flush();

				System.Threading.Thread.Sleep(10);

				clientCLI.StandardInput.WriteLine("readall");

				clientCLI.StandardInput.WriteLine("exit");

				clientCLI.StandardInput.Flush();

				exitCode = StopCLIProcess(clientCLI);
				serverCore.Dispose();
			}
			finally
			{
				Console.WriteLine("Client Output - - - - - - - - -");
				Console.WriteLine(clientCLI.StandardOutput.ReadAllAvailable());
				Console.WriteLine(clientCLI.StandardError.ReadAllAvailable());
				Console.WriteLine("- - - - - - - - - - - - - - - -");
			}

			//Assert.That.ExitZero(clientCLI);
		}

		[TestMethod]
		public void TestFileCommand()
		{
			StartClientCoreAndServerCLI(out LiveLinkCore clientCore, out Process serverCLI);

			string filePath = "Resources/T-Pose.anm";
			serverCLI.StandardInput.WriteLine($"send --file {filePath}");
			serverCLI.StandardInput.WriteLine($"flush");

			Thread.Sleep(100);

			bool recieved = clientCore.TryReadMessage(out MemoryStream message);

			clientCore.Dispose();
			int exitCode = StopCLIProcess(serverCLI);
			Console.WriteLine("Server Output - - - - - - - - -");
			Console.WriteLine(serverCLI.StandardOutput.ReadAllAvailable());
			Debug.Write(serverCLI.StandardError.ReadAllAvailable());
			Console.WriteLine("- - - - - - - - - - - - - - - -");
			Assert.That.ExitZero(serverCLI);

			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				Assert.That.StreamsAreEqual(fileStream, message);
			}
		}
		
		[TestMethod]
		public void TestServerIsConnected()
		{
			StartServerCoreAndClientCLI(out LiveLinkCore serverCore, out Process clientCLI);

			Console.WriteLine($"serverCore.IsConnected = {serverCore.IsConnected}");

			clientCLI.StandardInput.WriteLine("disconnect");

			Thread.Sleep(1000);

			Console.WriteLine("Client Output - - - - - - - - -");
			Console.WriteLine(clientCLI.StandardOutput.ReadAllAvailable());
			Console.WriteLine("- - - - - - - - - - - - - - - -");

			StopCLIProcess(clientCLI);
			Assert.That.ExitZero(clientCLI);

			Console.WriteLine($"serverCore.IsConnected = {serverCore.IsConnected}");
			Assert.IsFalse(serverCore.IsConnected, "serverCore.IsConnected is still still true.");


		}

		[TestMethod]
		public void TestServerCLIUnexpectedDisconnect()
		{
			StartClientCoreAndServerCLI(out LiveLinkCore clientCore, out Process serverCLI);

			StopCLIProcess(serverCLI);

			Assert.That.ExitZero(serverCLI);
			Assert.IsFalse(clientCore.IsConnected);
		}

		[TestMethod]
		public void TestClientIsConnectedCLI()
		{
			StartServerCoreAndClientCLI(out LiveLinkCore serverCore, out Process clientCLI);

			clientCLI.StandardInput.WriteLine("isconnected");

			//System.Threading.Thread.Sleep(100);

			serverCore.Disconnect();

			clientCLI.StandardInput.WriteLine("isconnected");

			StopCLIProcess(clientCLI);

			Console.WriteLine("Client Output - - - - - - - - -");
			Console.WriteLine(clientCLI.StandardOutput.ReadAllAvailable());
			Console.WriteLine(clientCLI.StandardError.ReadAllAvailable());
			Console.WriteLine("- - - - - - - - - - - - - - - -");

			Assert.That.ExitZero(clientCLI);
		}

		[TestMethod]
		public void TestServerCrashing()
		{
			StartClientCoreAndServerCLI(out LiveLinkCore clientCore, out Process serverCLI, out string address);

			for (int i = 0; i < 1; i++)
			{
				Console.WriteLine($"Attempt {i}");

				// Crash area
				serverCLI.StandardInput.WriteLine("disconnect");
				while (clientCore.IsConnected)
				{
					Thread.Sleep(0);
				}
				if (serverCLI.HasExited) break;

				// Reconnect for next attempt
				clientCore.Disconnect();
				serverCLI.StandardInput.WriteLine($"start --server {address}");
				serverCLI.StandardInput.WriteLine($"waitfor --connection");
				
				bool connected = false;
				for (int j = 0; j < 10; j++)
				{
					if (connected = clientCore.StartClient(address)) break;
					Thread.Sleep(0);
				}
				if (serverCLI.HasExited) break;
				Assert.IsTrue(connected, "Failed to connect client to server.");
			}
			StopCLIProcess(serverCLI);
			Assert.That.ExitZero(serverCLI);
		}

		[TestMethod]
		public void TestClientCrashingCLI()
		{
			StartServerCoreAndClientCLI(out LiveLinkCore serverCore, out Process clientCLI);

			for (int i = 0; i < 100; i++)
			{
				Console.WriteLine($"Attempt {i}");

				// Crash area
				clientCLI.StandardInput.WriteLine("disconnect");
				int j = 0;
				while (serverCore.IsConnected && !clientCLI.HasExited)
				{
					if (j++ >= 300) break;
					Thread.Sleep(1);
				}
				if (clientCLI.HasExited) break;
				Assert.IsFalse(serverCore.IsConnected, "Client never disconnected");

				// Reconnect for next attempt
				serverCore.Disconnect();
				serverCore.StartServer(serverCore.Address);
				clientCLI.StandardInput.WriteLine($"start --client {serverCore.Address}");
				Assert.IsTrue(serverCore.WaitForConnection(1000), "Client did not reconnect.");
			}
			StopCLIProcess(clientCLI);
			Assert.That.ExitZero(clientCLI);
		}



		void AssertContains(string output, string mustContain)
		{
			Assert.IsTrue(output.Contains(mustContain), $"Expected output to contain \"{mustContain}\"");
		}
	}
}
