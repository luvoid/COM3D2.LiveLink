using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace COM3D2.LiveLink
{
	internal class ClientConnection : Connection, IDisposable
	{
		protected override PipeStream Pipe => m_ClientPipe;
		public override bool CanRead => m_ClientPipe != null && m_ClientPipe.CanRead;
		public override bool CanWrite => m_ClientPipe != null && m_ClientPipe.CanWrite;

		NamedPipeClientStream m_ClientPipe;
		Thread m_ReadThread;
		ConcurrentQueue<MemoryStream> m_InMessageQueue;

		public static bool TryConnect(string serverName, string pipeName, out ClientConnection clientConnection, int timeout = 1000)
		{
			clientConnection = new ClientConnection(serverName, pipeName);
			return clientConnection.TryConnect(timeout);
		}

		ClientConnection(string serverName, string pipeName)
		{
			// Must use the long constructor due to a .NET bug.
			// See https://stackoverflow.com/a/32739225

			m_ClientPipe = new NamedPipeClientStream(
				serverName,
				pipeName,
				PipeAccessRights.ReadData | PipeAccessRights.WriteAttributes,
				PipeOptions.Asynchronous,
				System.Security.Principal.TokenImpersonationLevel.None,
				HandleInheritability.None
			);
		}

		bool TryConnect(int timeout = 1000)
		{
			try
			{
				m_ClientPipe.Connect(timeout);
			}
			catch (TimeoutException ex)
			{
				Console.Error.WriteLine($"Failed to connect to LiveLink server. (Timeout)\n{ex.Message}");
				return false;
			}
			catch (Win32Exception ex)
			{
				Console.Error.WriteLine($"Failed to connect to LiveLink server. (OS Error)\n{ex.Message}");
			}

			if (m_ClientPipe.IsConnected)
			{
				Console.WriteLine("Connected to LiveLink server!");
				Console.WriteLine($"m_ClientPipe.ReadMode = {m_ClientPipe.ReadMode}");
				Initialize();
				return true;
			}
			else
			{
				Console.WriteLine("Failed to connect to LiveLink server!");
				return false;
			}
		}

		void Initialize()
		{
			m_ReadThread = new Thread(ReadThread);
			m_InMessageQueue = new ConcurrentQueue<MemoryStream>();
			m_ReadThread.Start();
		}

		object m_ReadLockWait = new object();
		object m_ReadLockContinue = new object();
		object m_TryReadLockWait = new object();
		object m_TryReadLockContinue = new object();
		void ReadThread()
		{
			//int initialBufferSize = 4096;
			//byte[] buffer = new byte[initialBufferSize];
			byte[] lengthBuffer = new byte[4];
			while (true)
			{
				if (4 > m_ClientPipe.Read(lengthBuffer, 0, 4)) continue;
				
				int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
				byte[] buffer = new byte[messageLength];
				int readCount = 0;
				while (readCount < messageLength)
				{
					readCount += m_ClientPipe.Read(buffer, readCount, messageLength - readCount);
				}
				//int readCount = m_ClientPipe.Read(message.GetBuffer(), 0, (int)message.Length);
				MemoryStream message = new MemoryStream(buffer, 0, messageLength, false, true);
				m_InMessageQueue.Enqueue(message);
			}
		}

		public string ReadAll()
		{
			string result = "";

			if (m_ReadThread.ThreadState != ThreadState.Running)
			{
				Console.WriteLine($"Warning: m_ReadThread.ThreadState = {m_ReadThread.ThreadState}");
			}

			byte[] buffer = new byte[4096];

			while (m_InMessageQueue.TryDequeue(out MemoryStream message))
			{
				using (StreamReader reader = new StreamReader(message, Encoding.UTF8))
				{
					result += reader.ReadToEnd();
				}
			}

			return result;
		}

		public bool TryReadMessage(out MemoryStream message)
		{
			return m_InMessageQueue.TryDequeue(out message);
		}


		protected override void OnDispose(bool disposing)
		{
			// Stop threads
			m_ReadThread?.Abort();

			if (disposing)
			{
				// Free managed resources
				m_ClientPipe.Dispose();
				m_ClientPipe = null;
				m_InMessageQueue = null;
			}

			// Free unmanaged resources
			// ...
		}
	}
}
