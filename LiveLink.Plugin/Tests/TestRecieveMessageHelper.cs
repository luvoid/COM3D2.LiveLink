using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin.Tests
{
	internal class TestRecieveMessageHelper : PluginTestHelper
	{
		protected override void SafeStart()
		{
			Plugin.Initialize();
			Plugin.enabled = false;
		}

		protected override void SafeUpdate()
		{
			if (Plugin.HandledMessageCount > 0)
			{
				Finish();
				Plugin.DisconnectClient();
			}
			else if (!Plugin.IsConnected)
			{
				if (Plugin.StartClient(TestAddress))
				{
					Logger.LogMessage($"{nameof(TestRecieveMessageHelper)}: Client started. Waiting for message...");
				}
			}
			else if (Plugin.IsConnected)
			{
				Plugin.ClientUpdate();
			}
		}
	}
}
