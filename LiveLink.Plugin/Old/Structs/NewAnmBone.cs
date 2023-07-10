using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin.Structs
{
	public class NewAnmBone
	{
		public string boneName { get; set; }
		public string bonePath { get; set; }
		public List<float> coordinates { get; set; }
		public List<string> childPaths { get; set; }

		public NewAnmBone()
		{
			boneName = "";
			bonePath = "";
			coordinates = new List<float>();
			childPaths = new List<string>();
		}
	}

}
