using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;

namespace COM3D2.LiveLink
{
	internal abstract class Connection : IDisposable
	{
		protected abstract PipeStream Pipe { get; }

		public virtual bool IsConnected => Pipe != null && Pipe.IsConnected;

		public abstract bool CanRead { get; }
		public abstract bool CanWrite { get; }


		protected bool IsDisposed { get; private set; } = false;
		protected void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				OnDispose(disposing);
				IsDisposed = true;
			}
		}
		protected abstract void OnDispose(bool disposing);
		
		~Connection()
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
