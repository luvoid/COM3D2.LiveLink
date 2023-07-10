using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace COM3D2.LiveLink.Plugin.JsonStructs
{
	public class AnmJsonChannelData
	{
		[JsonProperty("100")]
		public List<AnmJsonFrameData> _100 { get; set; }
		[JsonProperty("101")]
		public List<AnmJsonFrameData> _101 { get; set; }
		[JsonProperty("102")]
		public List<AnmJsonFrameData> _102 { get; set; }
		[JsonProperty("103")]
		public List<AnmJsonFrameData> _103 { get; set; }
		[JsonProperty("104")]
		public List<AnmJsonFrameData> _104 { get; set; }
		[JsonProperty("105")]
		public List<AnmJsonFrameData> _105 { get; set; }
		[JsonProperty("106")]
		public List<AnmJsonFrameData> _106 { get; set; }

		public AnmJsonChannelData()
		{
			_100 = new List<AnmJsonFrameData>();
			_101 = new List<AnmJsonFrameData>();
			_102 = new List<AnmJsonFrameData>();
			_103 = new List<AnmJsonFrameData>();
			/*_104 = new List<AnmJsonFrameData>();
			_105 = new List<AnmJsonFrameData>();
			_106 = new List<AnmJsonFrameData>();*/
		}

		public AnmJsonFrameData getFrameOrNew(float frame, List<AnmJsonFrameData> channel)
		{
			for (int i = 0; i < channel.Count; i++)
			{
				if (channel[i].frame == frame)
				{
					return channel[i];
				}
			}

			return new AnmJsonFrameData();
		}
	}

}
