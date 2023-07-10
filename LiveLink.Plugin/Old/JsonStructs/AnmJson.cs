using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin.JsonStructs
{
	public class AnmJson
	{
		string name { get; set; }
		float boobStiffnessL { get; set; } // 0 mean lots of jiggle
		float boobStiffnessRL { get; set; } // 0 mean lots of jiggle

		public AnmJson()
		{

		}
	}
}
