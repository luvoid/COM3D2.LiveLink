using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin.Tests
{
	internal class TestRecieveMessageWithMaidHelper : TestRecieveMessageHelper
	{
		protected override void SafeStart()
		{
			Plugin.Initialize();
			Plugin.enabled = false;
		}

		private static bool addedMaid = false;
		protected override void SafeUpdate()
		{
			if (!addedMaid)
			{
				var maid = GameMain.Instance.CharacterMgr.AddStockMaid();
				GameMain.Instance.CharacterMgr.SetActiveMaid(maid, 0);
				maid.Visible = true;
				addedMaid = true;
			}
			else
			{
				base.SafeUpdate();
			}
		}
	}
}
