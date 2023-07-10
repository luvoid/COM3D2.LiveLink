using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin.JsonStructs
{
	public class AnmJsonFrameData
	{
		public float frame { get; set; }
		public float f0 { get; set; }
		public float f1 { get; set; }
		public float f2 { get; set; }

		public AnmJsonFrameData()
		{
			frame = 0f;
			f0 = 0f;
			f1 = 0f;
			f2 = 0f;
		}
	}
}
