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
using System.Threading.Tasks;
using COM3D2.LiveLink.Concurrent;

namespace COM3D2.LiveLink
{
	internal class ServerConnection : Connection
	{
		public static int MaxConnections = 1;

		protected override PipeStream Pipe => m_ServerPipe;
		public override bool IsConnected => base.IsConnected && !m_IsEndOfPipe;


		private NamedPipeServerStream m_ServerPipe;

		private Task<bool> m_ListenForConnectionTask;

		private Thread m_WriteThread;
		private ConcurrentQueue<MemoryStream> m_OutMessageQueue;
		//private ConcurrentDictionary<IAsyncResult, bool> m_WriteOperations = new ConcurrentDictionary<IAsyncResult, bool>();

		private Thread m_ReadThread;
		private Concurrent<bool> m_IsEndOfPipe = new Concurrent<bool>(false);

		private bool m_IsStopListening = false;

		public event Action<ServerConnection> OnClientConnected;

		public static ServerConnection OpenConnection(string pipeName)
		{
			ServerConnection connection = new ServerConnection(pipeName);
			connection.ListenForConnection();
			//Console.WriteLine($"m_ServerPipe.TransmissionMode = {connection.m_ServerPipe.TransmissionMode}");
			return connection;
		}

		ServerConnection(string pipeName)
		{
			m_ServerPipe = new NamedPipeServerStream(
				pipeName,
				PipeDirection.InOut,
				MaxConnections,
				PipeTransmissionMode.Byte,
				PipeOptions.WriteThrough | PipeOptions.Asynchronous
			);
		}

		private void ListenForConnection()
		{
			if (m_ServerPipe == null) return;
			if (m_ListenForConnectionTask != null) return;
			m_ListenForConnectionTask = new Task<bool>(() =>
			{
				var r = m_ServerPipe.BeginWaitForConnection(null, null);
				while (!r.IsCompleted)// && !m_IsStopListening)
				{
					if (m_IsStopListening)
					{
						//Console.WriteLine($"m_IsStopListening = {m_IsStopListening}");
						r.AsyncWaitHandle.Close();
						break;
					}
					if (r.AsyncWaitHandle.WaitOne(50))
					{
						m_ServerPipe.EndWaitForConnection(r);
					}
					//Console.WriteLine(r.IsCompleted);
				}

				if (!m_ServerPipe.IsConnected)
				{
					// I have no idea why, but calling it again
					// fixes a bug that happens sometimes, where
					// m_ServerPipe.IsConnected = False,
					// even after a client has connected.
					m_ServerPipe.WaitForConnection();
				}

				//Console.WriteLine($"m_ServerPipe.IsConnected = {m_ServerPipe.IsConnected}; r.IsCompleted = {r.IsCompleted}");
				if (m_ServerPipe.IsConnected)
				{
					Initialize();
					OnClientConnected?.Invoke(this);
					return true;
				}
				else
				{
					// Find out why the connection failed
					try
					{
						m_ServerPipe.WaitForConnection();
						Console.Error.WriteLine($"m_ServerPipe.IsConnected = {m_ServerPipe.IsConnected}");
						Console.Error.WriteLine($"{this} failed to wait for connection for an unknown reason.");
					}
					catch (Exception e) 
					{
						Console.Error.WriteLine($"{this} failed to wait for connection: {e}");
					}
				}
				return false;
			});
			m_ListenForConnectionTask.Start();
		}

		public bool WaitForConnection(int timeout = Timeout.Infinite)
		{
			if (m_ListenForConnectionTask == null) 
				ListenForConnection();

			if (m_ListenForConnectionTask.IsCompleted)
				return m_ListenForConnectionTask.Result;

			if (timeout < 0)
			{
				m_ListenForConnectionTask.Wait();
			}
			else if (!m_ListenForConnectionTask.Wait(timeout))
				return false;

			return m_ListenForConnectionTask.Result;
		}

		void Initialize()
		{
			m_WriteThread = new Thread(WriteThread);
			m_OutMessageQueue = new ConcurrentQueue<MemoryStream>();
			m_WriteThread.Start();

			m_ReadThread = new Thread(ReadThread);
			m_ReadThread.Start();
		}

		private bool m_WriteFlush = false;
		private int m_NextHeartbeat = int.MinValue;
		private int m_HeartbeatInterval = 1000;
		void WriteThread()
		{
			while (true)
			{
				if (Environment.TickCount > m_NextHeartbeat)
				{
					m_ServerPipe.Write(BitConverter.GetBytes(0), 0, 4);
					m_NextHeartbeat = Environment.TickCount + m_HeartbeatInterval;
				}

				while (m_OutMessageQueue.TryDequeue(out MemoryStream message))
				{
					m_ServerPipe.Write(BitConverter.GetBytes((int)message.Length), 0, 4);
					message.WriteTo(m_ServerPipe);

					//byte[] buffer = new byte[(int)message.Length + 4];
					//BitConverter.GetBytes((int)message.Length).CopyTo(buffer, 0);
					//message.Read(buffer, 4, buffer.Length - 4);
					//var r = m_ServerPipe.BeginWrite(buffer, 0, buffer.Length,
					//	(x) => m_WriteOperations.TryRemove(x, out _), null);
					//m_WriteOperations.TryAdd(r, true);
				}
				if (m_WriteFlush)
				{
					m_ServerPipe.Flush();
					break;
				}
				else
				{
					Thread.Sleep(0);
				}
			}
		}

		void ReadThread()
		{
			while (true)
			{
				int result = m_ServerPipe.ReadByte();
				if (result == -1)
				{
					m_IsEndOfPipe.Value = true;
					Console.WriteLine($"{this} lost connection");
					return;
				}
				else
				{
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
			if (IsDisposed) throw new System.InvalidOperationException($"The {this.GetType().Name} has been disposed");
			if (!IsConnected) throw new System.InvalidOperationException($"The {this.GetType().Name} has no connection");
			m_WriteFlush = true;
			m_WriteThread.Join();
			m_WriteFlush = false;
			m_WriteThread = new Thread(WriteThread);
			m_WriteThread.Start();
		}

		protected override void OnDispose(bool disposing)
		{
			// Stop threads
			m_IsStopListening = true;
			m_ListenForConnectionTask?.Wait(100);
			m_ListenForConnectionTask?.Dispose();
			m_WriteThread?.Abort();
			m_ReadThread?.Abort();

			if (disposing)
			{
				// Free managed resources
				m_ServerPipe?.Dispose();
				m_ServerPipe = null;
				m_OutMessageQueue = null;
			}

			// Free unmanaged resources
			// ...
		}
	}
}
