using BepInEx.Logging;
using System.IO;

namespace COM3D2.LiveLink.Plugin.Tests
{
	public class PluginTestLogListener : ILogListener
	{
		public bool Enabled = true;

		protected TextWriter Out;
		protected TextWriter Error;

		private Stream _outStream;
		private Stream _errStream;

		private bool _disposedValue;

		public PluginTestLogListener()
		{
			//_outStream = System.Console.OpenStandardOutput(256);
			//_errStream = System.Console.OpenStandardError (256);

			var bepinRoot = BepInEx.Paths.BepInExRootPath;
			var outFile = bepinRoot + $@"\{PluginTests.STD_OUT_FILE}";
			var errFile = bepinRoot + $@"\{PluginTests.STD_ERR_FILE}";
			if (File.Exists(outFile)) File.Delete(outFile);
			if (File.Exists(errFile)) File.Delete(errFile);
			_outStream = new FileStream(outFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 256, FileOptions.WriteThrough);
			_errStream = new FileStream(errFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, 256, FileOptions.WriteThrough);

			Out = new StreamWriter(_outStream);
			Error = new StreamWriter(_errStream);

			BepInEx.Logging.Logger.Listeners.Add(this);
		}

		public void LogEvent(object sender, LogEventArgs eventArgs)
		{
			if (!Enabled) return;
			switch (eventArgs.Level)
			{
				default:
				case LogLevel.Info:
				case LogLevel.Message:
				case LogLevel.Debug:
					Out.WriteLine(eventArgs.ToString());
					break;
				case LogLevel.Warning:
				case LogLevel.Error:
				case LogLevel.Fatal:
					Error.WriteLine(eventArgs.ToString());
					break;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					// Dispose managed resources
					BepInEx.Logging.Logger.Listeners.Remove(this);
					_outStream.Dispose();
					_errStream.Dispose();
				}
				// Dispose unmanaged resources
				// ...
				_disposedValue = true;
			}
		}

		~PluginTestLogListener()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			System.GC.SuppressFinalize(this);
		}
	}
}
