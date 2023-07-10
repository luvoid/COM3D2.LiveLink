using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin.Structs
{
	public class NewAnmWriteBone
	{
		public string path { get; set; }
		public List<List<float>> numArray { get; set; }
		public List<NewAnmWriteBone> children { get; set; }

		public NewAnmWriteBone()
		{
			numArray = new List<List<float>>();
			children = new List<NewAnmWriteBone>();
		}

		public static NewAnmWriteBone getWriteAnmData(NewAnm newAnm)
		{
			//Get a sample
			newAnm.frames.Sort(delegate (NewAnmFrame x, NewAnmFrame y)
			{
				return x.frameTime.CompareTo(y.frameTime);
			});
			NewAnmFrame firstFrame = newAnm.frames[0];

			//Build out the saving object
			NewAnmBone bip01 = firstFrame.bones["Bip01"];

			//Setup the heirarchy
			return buildBoneHeirarchy(newAnm, firstFrame, bip01);
		}

		private static NewAnmWriteBone buildBoneHeirarchy(NewAnm newAnm, NewAnmFrame sampleFrame, NewAnmBone newAnmBone)
		{
			NewAnmWriteBone saveBone = new NewAnmWriteBone();
			saveBone.path = newAnmBone.bonePath;
			saveBone.numArray = new List<List<float>>();

			//Loop the frames
			for (int i = 0; i < newAnm.frames.Count; i++)
			{
				NewAnmFrame nextFrame = newAnm.frames[i];

				//Storage
				List<float> data = new List<float>();

				//Add the frame time
				data.Add(nextFrame.frameTime);

				//Add the coordinate values -- maybe add additional control values in the future (custom in/out tangent)
				data.AddRange(nextFrame.bones[saveBone.path].coordinates);

				saveBone.numArray.Add(data);
			}

			//Setup the children
			for (int i = 0; i < newAnmBone.childPaths.Count; i++)
			{
				NewAnmBone newAnmChildBone = sampleFrame.bones[newAnmBone.childPaths[i]];
				saveBone.children.Add(buildBoneHeirarchy(newAnm, sampleFrame, newAnmChildBone));
			}

			return saveBone;
		}
	}

}
