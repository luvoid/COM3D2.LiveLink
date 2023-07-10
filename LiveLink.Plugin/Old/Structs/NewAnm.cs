using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static COM3D2.LiveLink.Plugin.AnmMaker;

namespace COM3D2.LiveLink.Plugin.Structs
{
	public class NewAnm
	{
		public string name { get; set; } //fill in when exporting
		public List<NewAnmFrame> frames { get; set; }
		float boobStiffnessL { get; set; } // 0 mean lots of jiggle -- boo using the built in reader can only set 0 and 1
		float boobStiffnessR { get; set; } // 0 mean lots of jiggle -- boo using the built in reader can only set 0 and 1

		private static HashSet<string> _save_bone_path_set_ = null;
		private static HashSet<string> save_bone_path_set_
		{
			get
			{
				if (_save_bone_path_set_ == null)
				{
					Type type = typeof(CacheBoneDataArray);
					FieldInfo info = type.GetField("save_bone_path_set_", BindingFlags.NonPublic | BindingFlags.Static);
					_save_bone_path_set_ = (HashSet<string>)info.GetValue(null);
				}

				return _save_bone_path_set_;
			}
		}

		public NewAnm()
		{
			name = "";
			frames = new List<NewAnmFrame>();
			boobStiffnessL = 0f;
			boobStiffnessR = 0f;
		}

		public bool WriteToBinary()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter((Stream)memoryStream);

			//Get the object that is used to write the .anm file
			NewAnmWriteBone bip01 = NewAnmWriteBone.getWriteAnmData(this);

			//Write CM3D2_ANIM
			binaryWriter.Write("CM3D2_ANIM");
			//Write 1001 to ensure boob jiggle can be read
			binaryWriter.Write(1001);

			//Start Writing bone data
			writeBoneToBinary(bip01, binaryWriter);

			//Write buffer
			binaryWriter.Write((byte)0);
			//Write if booble jiggles enabled
			binaryWriter.Write(boobStiffnessL > 0f ? (byte)0 : (byte)1);
			binaryWriter.Write(boobStiffnessR > 0f ? (byte)0 : (byte)1);

			//Cleanup
			binaryWriter.Close();
			memoryStream.Close();
			byte[] array = memoryStream.ToArray();
			memoryStream.Dispose();

			//Write the actual file
			string filePath = null; // Path.Combine(pathAnm, (name + ".anm"));
			if (!File.Exists(filePath))
			{
				File.WriteAllBytes(filePath, array);

				if (File.Exists(filePath))
				{
					//infoMessage = ".anm File Saved";

					PhotoMotionData photoMotionData = PhotoMotionData.AddMyPose(filePath);
					if (photoMotionData != null)
					{
						PhotoWindowManager mgr = (UnityEngine.GameObject.Find("PhotoWindowManager").GetComponent<PhotoWindowManager>());
						if (mgr != null)
						{
							MotionWindow motionWindow = mgr.GetWindow(PhotoWindowManager.WindowType.Motion) as MotionWindow;
							if (motionWindow != null)
							{
								motionWindow.PopupAndTabList.AddData("マイポーズ", new KeyValuePair<string, object>(photoMotionData.name, (object)photoMotionData));
							}
						}
					}
				}
			}
			else
			{
				//infoMessage = "Save Failed: .anm File Already Exists";
				return false;
			}

			return true;
		}

		private void writeBoneToBinary(NewAnmWriteBone bone, BinaryWriter bw)
		{
			if (NewAnm.save_bone_path_set_.Contains(bone.path) || bone.path.Contains("chinko") || bone.path.Contains("tamabukuro"))
			{
				//Write byte that indicates bone path
				bw.Write((byte)1);
				//Write bone path
				bw.Write(bone.path);
				UnityEngine.Debug.Log("BONE:" + bone.path);

				//Loop Channels - index = 1 bc first entry is not a channel
				for (int index1 = 1; index1 < bone.numArray[0].Count; index1++)
				{
					//Write the channel
					bw.Write((byte)(100 + index1 - 1));
					UnityEngine.Debug.Log("CHANNEL:" + (100 + index1 - 1));

					//Sort the frames by time
					bone.numArray.Sort(delegate (List<float> x, List<float> y)
					{
						return x[0].CompareTo(y[0]);
					});

					//Improve Data
					float lastVal = 0f;
					float lastInTan = 0f;
					float lastOutTan = 0f;
					List<int> indexesToRemove = new List<int>();
					for (int index2 = bone.numArray.Count - 1; index2 >= 0; index2--)
					{
						float currentVal = bone.numArray[index2][index1];

						//In-tangent and out-tangent (always 0 for rotations, maybe can be calculated for locations)
						float currentInTan = 0f;
						float currentOutTan = 0f;

						//Check for duplicate frames, we always need first and last tho
						if (index2 != 0 && index2 != (bone.numArray.Count - 1))
						{
							//If no change from the previous frame, no need to write
							if (currentVal == lastVal && currentInTan == lastInTan && currentOutTan == lastOutTan)
							{
								bone.numArray.RemoveAt(index2);
								continue;
							}
						}

						//Save values for next
						lastVal = currentVal;
						lastInTan = currentInTan;
						lastOutTan = currentOutTan;
					}

					//Write the number of frames for this bone
					bw.Write(bone.numArray.Count);
					UnityEngine.Debug.Log("FRAMES:" + bone.numArray.Count);

					//Write the data for coordinates
					for (int index2 = 0; index2 < bone.numArray.Count; ++index2)
					{
						float currentVal = bone.numArray[index2][index1];

						//In-tangent and out-tangent (always 0 for rotations, maybe can be calculated for locations)
						float currentInTan = 0f;
						float currentOutTan = 0f;

						//Write the frame time
						bw.Write(bone.numArray[index2][0]);
						UnityEngine.Debug.Log("FRAME:" + index2 + " TIME: " + bone.numArray[index2][0]);

						//Write the channel's value for this frame
						bw.Write(currentVal); UnityEngine.Debug.Log("f0:" + currentVal);
						bw.Write(currentInTan); UnityEngine.Debug.Log("f1:" + currentInTan);
						bw.Write(currentOutTan); UnityEngine.Debug.Log("f2:" + currentOutTan);
					}
				}

				//if(bone.path.Trim().ToLower().Equals("bip01"))
				//{
				//    //Write the channel
				//    bw.Write((byte)(121));

				//    //Write the number of frames for this bone
				//    bw.Write(bone.numArray.Count);

				//    //Write the data for coordinates
				//    bool val = false;
				//    for (int index2 = 0; index2 < bone.numArray.Count; ++index2)
				//    {
				//        //Write the frame time
				//        bw.Write(bone.numArray[index2][0]);
				//        bw.Write(val? 1f:0f);
				//        bw.Write(0f);
				//        bw.Write(0f);

				//        val = !val;
				//    }
				//}
			}

			//Process Children
			for (int index = 0; index < bone.children.Count; ++index)
			{
				writeBoneToBinary(bone.children[index], bw);
			}

			if (bone.path.Trim().ToLower().Equals("bip01"))
			{
				//Write byte that indicates bone path
				bw.Write((byte)1);
				//Write bone path
				bw.Write("SK_munel");

				//Write the channel
				bw.Write((byte)(201));

				float increment = bone.numArray[bone.numArray.Count - 1][0] / 10f;

				//Write the number of frames for this bone
				bw.Write(10);//bone.numArray.Count);

				//Write the data for coordinates
				bool val = false;
				for (int index2 = 0; index2 < 10; ++index2)
				{
					//Write the frame time
					bw.Write(increment * index2);
					bw.Write(val ? 1f : 0f);
					bw.Write(0f);
					bw.Write(0f);

					val = !val;
				}
			}
		}
	}
}


