using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CM3D2.Serialization;
using COM3D2.LiveLink.CLI.Commands;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using CM3D2.UGUI;


// If there are errors in the above using statements, restore the NuGet packages:
// 1. Right-click on the COM3D2.LiveLink Solution in the Solution Explorer (not a project or a .cs file)
// 2. In the pop-up context menu, click on "Restore NuGet Packages"


// This is the major & minor version with an asterisk (*) appended to auto increment numbers.
[assembly: AssemblyVersion(COM3D2.LiveLink.Plugin.PluginInfo.PLUGIN_VERSION + ".*")]
[assembly: AssemblyFileVersion(COM3D2.LiveLink.Plugin.PluginInfo.PLUGIN_VERSION)]

// These two lines tell the compiler not worry about accessing private variables/classes.
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]


namespace COM3D2.LiveLink.Plugin
{
	public static class PluginInfo
	{
		// The name of this assembly.
		internal const string PLUGIN_GUID = "COM3D2.LiveLink.Plugin";
		// The name of this plugin.
		internal const string PLUGIN_NAME = "LiveLink";
		// The version of this plugin.
		internal const string PLUGIN_VERSION = "1.1.0";

		public static readonly string Guid = PLUGIN_GUID;
		public static readonly string Name = PLUGIN_NAME;
		public static readonly string Varsion = PLUGIN_VERSION;
	}
}



namespace COM3D2.LiveLink.Plugin
{
	// This is the metadata set for your plugin.
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public sealed partial class LiveLinkPlugin : BaseUnityPlugin
	{
		// Static saving of the main instance. (Singleton design pattern)
		// This makes it easier to run stuff like coroutines from static methods or accessing non-static vars.
		public static LiveLinkPlugin Instance { get; private set; }

		// Static property for the logger so you can log from other classes.
		internal static new ManualLogSource Logger => Instance?._Logger;
		private ManualLogSource _Logger => base.Logger;

		private LiveLinkCore m_Core = new LiveLinkCore();

		internal new LiveLinkPluginConfig Config = new LiveLinkPluginConfig();
		public ConfigFile ConfigFile => base.Config;

		private KeyboardShortcut m_StartClientShortcut = new KeyboardShortcut(KeyCode.L, KeyCode.LeftControl);
		private KeyboardShortcut m_DisconnectShortcut = new KeyboardShortcut(KeyCode.L, KeyCode.LeftControl, KeyCode.LeftAlt);

		private bool m_IsExpectConnected = false;

		public bool IsConnected => m_Core != null && m_Core.IsConnected;

		internal UIBase PluginUIBase;

		private void Awake()
		{
			Instance = this;
			if (!Tests.PluginTests.RunTestsInCommandline())
			{
				Initialize();
			}
		}

		private bool _isInitalized = false;
		internal void Initialize()
		{
			if (_isInitalized) return;

			Config.BindToConfigFile(ConfigFile);

			// Installs the patches in the BlenderLiveLink class.
			Harmony.CreateAndPatchAll(typeof(LiveLinkPlugin));
			Harmony.CreateAndPatchAll(typeof(ImportCMExtensions));

			new GameObject("LiveLinkDirector", typeof(LiveLinkDirector)).transform.SetParent(this.transform, false);

			CM3D2Universe.Init(OnUniverseInit);

			Logger.LogInfo("LiveLink Plugin Loaded");
		}

		private void OnUniverseInit()
		{
			PluginUIBase = CM3D2UniversalUI.RegisterUI(PluginInfo.PLUGIN_GUID);
			PluginUIBase.Enabled = false;

			LiveLinkDirector.Instance.InitializePanel(PluginUIBase);
		}

		internal bool StartClient(string address = null)
		{
			address ??= Config.ServerAddress;

			Logger.LogMessage($"Attempting to connect to LiveLink server at {address}");
			bool didConnect = m_Core.StartClient(address);
			if (didConnect)
			{
				m_IsExpectConnected = true;
				GameMain.Instance.SysDlg.ShowEnqueue("Connected to LiveLink server.");
			}
			else
			{
				GameMain.Instance.SysDlg.ShowEnqueue("Could not connect to LiveLink server.");
			}
			return didConnect;
		}

		internal void DisconnectClient()
		{
			if (!IsConnected) return;
			Logger.LogMessage("Disconnecting LiveLink");
			m_Core.Disconnect();
			GameMain.Instance.SysDlg.ShowEnqueue("Disconnected from LiveLink server.");
			m_IsExpectConnected = false;
		}

		private void Update()
		{
			CheckUnexpectedDisconnect();

			if (m_Core.IsConnected)
			{
				ClientUpdate();
			}

			if (m_StartClientShortcut.IsDown())
			{
				PluginUIBase.Enabled = true;
			}

			if (GameMain.Instance.SysDlg.IsDecided)
			{
				if (!m_Core.IsConnected && m_StartClientShortcut.IsDown())
				{
					StartClient();
				}
				else if (m_Core.IsConnected && m_DisconnectShortcut.IsDown())
				{
					GameMain.Instance.SysDlg.Show(
						"⚠️ WARNING ⚠️\n" +
						"Disconnecting client may crash the server, the client, or both. " +
						"This action is only safe if the server is already disconnected.\n\n" +
						"Are you sure you want to try disconnecting?", 
						SystemDialog.TYPE.YES_NO, DisconnectClient);
				}
			}
		}

		private void CheckUnexpectedDisconnect()
		{
			//Logger.LogDebug($"{m_IsExpectConnected} && !{m_Core.IsConnected}");
			if (m_IsExpectConnected && !m_Core.IsConnected)
			{
				GameMain.Instance.SysDlg.Show("Connection to LiveLink server has been lost.", SystemDialog.TYPE.OK);
				m_Core.Disconnect();
				m_IsExpectConnected = false;
			}
		}

		internal int HandledMessageCount { get; private set; } = 0;
		internal void ClientUpdate()
		{
			while (m_Core.TryReadMessage(out MemoryStream message))
			{
				HandleMessage(message);
				HandledMessageCount += 1;
			}
		}

		public T DummyMethod<T>(T obj)
			where T : class
		{
			return obj;
		}

		internal void HandleMessage(MemoryStream message)
		{
			CM3D2Serializer serializer = new CM3D2Serializer();
			long pos = message.Position;
			string command = serializer.Deserialize<string>(message);
			message.Position = pos;
			Logger.LogDebug($"HandleMessage {command}");
			if (command == "CM3D2_ANIM")
			{
				LiveLinkDirector.Instance.SetAnimation(message.GetBuffer());
			}
			else if (command == "CM3D2_MESH")
			{
				LiveLinkDirector.Instance.LoadModel(message);
			}
		}

	}
}
