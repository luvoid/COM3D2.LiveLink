using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin.Tests
{
	internal class SafeExitHelper : PluginTestHelper
	{
		public int ExitCode = 0;
		public bool WaitForTests = false;

		protected override IEnumerable<string> ValidUpdateScenes => new string[]
		{
			"SceneWarning",
			"SceneTitle",
			"SceneEdit"
		};

		protected override void SafeUpdate()
		{
			if (WaitForTests && ActiveTestHelperCount > 1) return;

			if (ExitCode == 0)
			{
				Logger.LogMessage($"Test complete. Exiting with code 0.");
				Application.Quit();
			}
			else
			{
				//Logger.LogFatal($"Test failed. Exiting with code {ExitCode}");
				////Application.Unload();
				////System.Environment.Exit(ExitCode);
				//Application.ForceCrash(ExitCode);
				
				Logger.LogFatal($"Test failed. Exiting with code {ExitCode}.");
				PluginTests.TestLogListener.Dispose();
				System.Environment.ExitCode = ExitCode;
				System.Diagnostics.Process.GetCurrentProcess().Kill();

				//Application.Quit();
				//System.Environment.Exit(ExitCode);
			}
		}
	}
}
