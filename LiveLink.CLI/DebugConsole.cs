using System;

namespace COM3D2.LiveLink.CLI
{
	public static class DebugConsole
	{
		public static bool IsDebugEnabled = false;

		public static void Write(object value)
		{
			if (!IsDebugEnabled) return;
			Console.Error.Write(value);
		}

		public static void WriteLine(object value)
		{
			if (!IsDebugEnabled) return;
			Console.Error.WriteLine(value);
		}

	}
}
