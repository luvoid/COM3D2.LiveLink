using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin.Structs
{
	public class NewAnmSks
	{
		public MPN mpn { get; set; }
		public string name { get; set; }
		public float absoluteValue { get; set; }
		public float additiveValue { get; set; }
		public float percentValue { get; set; }

		public NewAnmSks()
		{
			mpn = MPN.null_mpn;
			name = "";
		}
	}

}
