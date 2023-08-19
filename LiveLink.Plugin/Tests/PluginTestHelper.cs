using BepInEx.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.LiveLink.Plugin.Tests
{
	internal abstract class PluginTestHelper : MonoBehaviour
	{
		protected static int ActiveTestHelperCount { get; private set; } = 0;
		private bool isFinished = false;

		protected static LiveLinkPlugin Plugin => LiveLinkPlugin.Instance;
		protected static ManualLogSource Logger => LiveLinkPlugin.Logger;
		private static PluginTestLogListener LogListener => PluginTests.TestLogListener;
		protected static string TestAddress => PluginTests.TestAddress;

		protected virtual IEnumerable<string> ValidUpdateScenes => new string[]
		{
			"SceneTitle",
			"SceneEdit"
		};

		private void Awake()
		{
			ActiveTestHelperCount += 1;
		}

		protected void Start()
		{
			LogListener.Enabled = true;
			try
			{
				SafeStart();
			}
			catch (System.Exception e)
			{
				Logger.LogError(e);
				Finish(-1);
			}
			finally
			{
				LogListener.Enabled = false;
			}
		}

		protected void Update()
		{
			LogListener.Enabled = true;
			try
			{
				var gameMain = GameMain.Instance;
				if (gameMain
					&& ValidUpdateScenes.Contains(gameMain.GetNowSceneName())
					&& !gameMain.MainCamera.IsFadeProc())
				{
					SafeUpdate();
				}
			}
			catch (System.Exception e)
			{
				Logger.LogError(e);
				Finish(-1);
			}
			finally
			{
				LogListener.Enabled = false;
			}
		}

		protected virtual void SafeStart() { }
		protected abstract void SafeUpdate();

		protected void Finish(int exitCode = 0)
		{
			if (isFinished) return;
			isFinished = true;
			ActiveTestHelperCount -= 1;
			if (exitCode != 0)
			{
				PluginTests.SafeExit(exitCode);
			}
			Object.Destroy(this);
		}
	}
}
