using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.IO;
using CM3D2.Serialization;

namespace COM3D2.LiveLink
{
	public class LiveLinkCore : IDisposable
	{
		public bool IsServer => m_ServerConnection != null;
		public bool IsClient => m_ClientConnection != null;
		public string Address { get; private set; }
		public bool IsConnected => (IsServer && m_ServerConnection.IsConnected) || (IsClient && m_ClientConnection.IsConnected);

		private ClientConnection m_ClientConnection;
		private ServerConnection m_ServerConnection;

		private CM3D2Serializer m_Serializer = new CM3D2Serializer();

		//private PipeStream NetworkPipe => IsServer ? m_ServerPipe as PipeStream : IsClient ? m_ClientConnection as PipeStream : null;

		private IAsyncResult m_AsyncConnect;

		public void StartServer(string address = "com3d2.livelink")
		{
			Address = address;
			Console.WriteLine($"Starting LiveLink server at address {Address}");
			m_ServerConnection = ServerConnection.OpenConnection(Address);
		}

		public bool StartClient(string address = "com3d2.livelink")
		{
			Address = address;
			Console.WriteLine("Start LiveLink client");
			if (ClientConnection.TryConnect(".", Address, out ClientConnection newConnection))
			{
				m_ClientConnection = newConnection;
				return true;
			}
			else
			{
				return false;
			}
		}

		public void WaitForConnection(int timeout = 1000)
		{
			if (IsServer)
			{
				m_ServerConnection.WaitForConnection();
			}
		}

		public void SendString(string value)
		{
			if (!AssertCanWrite(nameof(SendString))) return;

			MemoryStream tempStream = new MemoryStream();
			m_Serializer.Serialize(tempStream, value);
			tempStream.Position = 0;
			m_ServerConnection.WriteMessage(tempStream);
			//m_ServerConnection.BeginWrite(tempStream.GetBuffer(), 0, (int)tempStream.Position, (x) => { }, null);
		}

		public void Flush()
		{
			m_ServerConnection.Flush();
		}

		public void SendBytes(byte[] bytes)
		{
			SendBytes(bytes, bytes.Length);
		}

		public void SendBytes(byte[] bytes, int count)
		{
			if (!AssertCanWrite(nameof(SendBytes))) return;
			byte[] messageBytes = new byte[count];
			Array.Copy(bytes, 0, messageBytes, 0, count);
			m_ServerConnection.WriteMessage(new MemoryStream(messageBytes, false));
		}

		public bool TryReadMessage(out MemoryStream message)
		{
			message = null;
			if (!AssertCanRead(nameof(TryReadMessage))) return false;
			return m_ClientConnection.TryReadMessage(out message);
		}

		public string ReadString()
		{
			if (!AssertCanRead(nameof(ReadString))) return null;
			if (!m_ClientConnection.TryReadMessage(out MemoryStream message)) return null;
			return m_Serializer.Deserialize<string>(message);
		}

		public string ReadAll()
		{
			if (!AssertCanRead(nameof(ReadAll))) return null;
			return m_ClientConnection.ReadAll();
		}


		public void Disconnect()
		{
			m_ClientConnection?.Dispose();
			m_ClientConnection = null;

			m_ServerConnection?.Dispose();
			m_ServerConnection = null;
		}
	
	
		private bool AssertCanWrite(string methodName, bool printError = true)
		{
			if (m_ServerConnection == null)
			{
				if (printError) Console.Error.WriteLine($"{nameof(LiveLinkCore)}.{methodName} error: m_ServerPipe is null");
				return false;
			}
			else if (!m_ServerConnection.CanWrite)
			{
				if (printError) Console.Error.WriteLine($"{nameof(LiveLinkCore)}.{methodName} error: m_ServerPipe.CanWrite is false");
				return false;
			}

			return true;
		}

		private bool AssertCanRead(string methodName, bool printError = true)
		{
			if (m_ClientConnection == null)
			{
				if (printError) Console.Error.WriteLine($"{nameof(LiveLinkCore)}.{methodName} error: m_ClientConnection is null");
				return false;
			}
			else if (!m_ClientConnection.CanRead)
			{
				if (printError) Console.Error.WriteLine($"{nameof(LiveLinkCore)}.{methodName} error: m_ClientConnection.CanRead is false");
				return false;
			}

			return true;
		}




		private bool m_IsDisposed;
		protected virtual void Dispose(bool disposing)
		{
			if (!m_IsDisposed)
			{
				if (disposing)
				{
					// Free managed resources
					Disconnect();
					m_Serializer = null;
				}

				// Free unmanaged resources

				m_IsDisposed = true;
			}
		}

		~LiveLinkCore()
		{
		    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		    Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
