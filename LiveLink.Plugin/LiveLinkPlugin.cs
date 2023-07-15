﻿using BepInEx;
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


// If there are errors in the above using statements, restore the NuGet packages:
// 1. Right-click on the COM3D2.LiveLink Solution in the Solution Explorer (not a project or a .cs file)
// 2. In the pop-up context menu, click on "Restore NuGet Packages"


// This is the major & minor version with an asterisk (*) appended to auto increment numbers.
[assembly: AssemblyVersion(COM3D2.LiveLink.Plugin.PluginInfo.PLUGIN_VERSION + ".*")]
[assembly: AssemblyFileVersion(COM3D2.LiveLink.Plugin.PluginInfo.PLUGIN_VERSION + ".0.0")]

// These two lines tell the compiler not worry about accessing private variables/classes.
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]


namespace COM3D2.LiveLink.Plugin
{
	public static class PluginInfo
	{
		// The name of this assembly.
		public const string PLUGIN_GUID = "COM3D2.LiveLink.Plugin";
		// The name of this plugin.
		public const string PLUGIN_NAME = "LiveLink";
		// The version of this plugin.
		public const string PLUGIN_VERSION = "0.1";
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

		private void Awake()
		{
			// Useful for engaging coroutines or accessing non-static variables. Completely optional though.
			Instance = this;

			Config.BindToConfigFile(ConfigFile);

			// Installs the patches in the BlenderLiveLink class.
			Harmony.CreateAndPatchAll(typeof(LiveLinkPlugin));

			this.gameObject.AddComponent<LiveLinkAnimator>();

			Logger.LogInfo("LiveLink Plugin Loaded");

			Tests.PluginTests.RunTestsInCommandline();
		}

		private void StartClient()
		{
			if (m_Core.StartClient(Config.ServerAddress))
			{
				GameMain.Instance.SysDlg.Show("Connected to LiveLink server.", SystemDialog.TYPE.OK);
				m_IsExpectConnected = true;
			}
			else
			{
				GameMain.Instance.SysDlg.Show("Could not connect to LiveLink server.", SystemDialog.TYPE.OK);
			}
		}

		private void DisconnectClient()
		{
			Logger.LogInfo("Disconnecting");
			m_Core.Disconnect();
			GameMain.Instance.SysDlg.Show("Disconnected from LiveLink server.", SystemDialog.TYPE.OK);
			m_IsExpectConnected = false;
		}

		private void Update()
		{
			CheckUnexpectedDisconnect();

			if (m_Core.IsConnected)
			{
				ClientUpdate();
			}

			if (GameMain.Instance.SysDlg.IsDecided)
			{
				if (!m_Core.IsConnected && m_StartClientShortcut.IsDown())
				{
					StartClient();
				}
				else if (m_Core.IsConnected && m_DisconnectShortcut.IsDown())
				{
					DisconnectClient();
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

		private void ClientUpdate()
		{
			while (m_Core.TryReadMessage(out MemoryStream message))
			{
				HandleMessage(message);
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
			string command = serializer.Deserialize<string>(message);
			Logger.LogDebug($"HandleMessage {command}");
			if (command == "CM3D2_ANIM")
			{
				LiveLinkAnimator.SetAnimation(message.GetBuffer());
			}
		}

	}
}
