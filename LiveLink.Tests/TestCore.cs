using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;

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

		public void StartClientCoreAndServerCLI(out LiveLinkCore clientCore, out Process serverCLI)
		{
			serverCLI = CreateServerProcess(out string address);
			Console.WriteLine($"address = {address}");

			clientCore = new LiveLinkCore();
			Assert.IsTrue(clientCore.StartClient(address), "Failed to connect client to server.");
			clientCore.WaitForConnection();

			serverCLI.StandardInput.WriteLine($"waitfor --connection --time 1");
			Assert.IsTrue(clientCore.IsConnected);
		}

		public void StartServerCoreAndClientCLI(out LiveLinkCore serverCore, out Process clientCLI)
		{
			serverCore = CreateServer();
			clientCLI = CreateClientProcess(serverCore.Address);

			serverCore.WaitForConnection(1000);

			Assert.IsTrue(serverCore.IsConnected);
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
				serverCore.WaitForConnection();
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

			System.Threading.Thread.Sleep(100);

			string recieved = clientCore.ReadString();

			clientCore.Dispose();
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
			serverCLI.StandardInput.WriteLine($"flush");

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

			System.Threading.Thread.Sleep(100);

			bool recieved = clientCore.TryReadMessage(out MemoryStream message);

			clientCore.Dispose();
			int exitCode = StopCLIProcess(serverCLI);
			Console.WriteLine("Server Output - - - - - - - - -");
			Console.Write(serverCLI.StandardOutput.ReadAllAvailable());
			Debug.Write(serverCLI.StandardError.ReadAllAvailable());
			Console.WriteLine("- - - - - - - - - - - - - - - -");
			Assert.That.ExitZero(serverCLI);

			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				Assert.That.StreamsAreEqual(fileStream, message);
			}
		}


		[TestMethod]
		public void TestServerCLIDisconnect()
		{
			StartClientCoreAndServerCLI(out LiveLinkCore clientCore, out Process serverCLI);

			Console.WriteLine($"clientCore.IsConnected = {clientCore.IsConnected}");

			serverCLI.StandardInput.WriteLine("disconnect");

			System.Threading.Thread.Sleep(100);

			clientCore.ReadAll();

			Console.WriteLine($"clientCore.IsConnected = {clientCore.IsConnected}");
			Assert.IsFalse(clientCore.IsConnected);

			StopCLIProcess(serverCLI);
			Assert.That.ExitZero(serverCLI);
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
		public void TestServerCoreDisconnect()
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



		void AssertContains(string output, string mustContain)
		{
			Assert.IsTrue(output.Contains(mustContain), $"Expected output to contain \"{mustContain}\"");
		}
	}
}
