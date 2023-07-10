using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin.JsonStructs
{
	public class AnmJsonBoneData
	{
		string path { get; set; }
		public AnmJsonChannelData channels { get; set; }

		public AnmJsonBoneData()
		{
			path = "";
			channels = new AnmJsonChannelData();
		}
	}

}
