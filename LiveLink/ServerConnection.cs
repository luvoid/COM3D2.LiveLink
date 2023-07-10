using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace COM3D2.LiveLink
{
	internal class ServerConnection : Connection
	{
		public static int MaxConnections = 1;

		protected override PipeStream Pipe => m_ServerPipe;
		public override bool CanRead => m_ServerPipe != null && m_ServerPipe.CanRead;
		public override bool CanWrite => m_ServerPipe != null && m_ServerPipe.CanWrite;

		private NamedPipeServerStream m_ServerPipe;
		Thread m_WriteThread;
		ConcurrentQueue<MemoryStream> m_OutMessageQueue;

		public static ServerConnection OpenConnection(string pipeName)
		{
			ServerConnection connection = new ServerConnection(pipeName);
			Console.WriteLine($"m_ServerPipe.TransmissionMode = {connection.m_ServerPipe.TransmissionMode}");
			return connection;
		}

		ServerConnection(string pipeName)
		{
			m_ServerPipe = new NamedPipeServerStream(
				pipeName,
				PipeDirection.Out,
				MaxConnections,
				PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous
			);
		}

		public bool WaitForConnection()
		{
			m_ServerPipe.WaitForConnection();
			if (m_ServerPipe.IsConnected)
			{
				Initialize();
				return true;
			}
			else
			{
				return false;
			}
		}

		void Initialize()
		{
			m_WriteThread = new Thread(WriteThread);
			m_OutMessageQueue = new ConcurrentQueue<MemoryStream>();
			m_WriteThread.Start();
		}

		private bool m_WriteFlush = false;
		void WriteThread()
		{
			while (true)
			{
				if (m_OutMessageQueue.TryDequeue(out MemoryStream message))
				{
					m_ServerPipe.Write(BitConverter.GetBytes((int)message.Length), 0, 4);
					message.WriteTo(m_ServerPipe);
				}
				else
				{
					if (m_WriteFlush)
					{
						m_ServerPipe.Flush();
						break;
					}
					Thread.Sleep(0);
				}
			}
		}

		public void WriteMessage(MemoryStream message)
		{
			m_OutMessageQueue.Enqueue(message);
		}

		public void Flush()
		{
			m_WriteFlush = true;
			m_WriteThread.Join();
			m_WriteFlush = false;
			m_WriteThread = new Thread(WriteThread);
			m_WriteThread.Start();
		}

		protected override void OnDispose(bool disposing)
		{
			if (disposing)
			{
				// Free managed resources
				m_ServerPipe.Dispose();
				m_ServerPipe = null;
				m_OutMessageQueue = null;
			}

			// Stop threads
			m_WriteThread?.Abort();

			// Free unmanaged resources
			// ...
		}
	}
}
