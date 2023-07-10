using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static COM3D2.LiveLink.Plugin.AnmMaker;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin.Structs
{
	public class NewAnmFrame
	{
		public string timestamp { get; set; }
		public float frameTime { get; set; }
		public Dictionary<string, NewAnmBone> bones { get; set; } //path, obj
		public Dictionary<MPN, Dictionary<string, NewAnmSks>> sks { get; set; }

		public NewAnmFrame()
		{
			timestamp = "";
			frameTime = 0f;
			bones = new Dictionary<string, NewAnmBone>();
			//sks = new Dictionary<string, float>();
		}

		public NewAnmBone RecordBones(CacheBoneDataArray.BoneData target_bone_data)
		{
			Vector3 localPosition = target_bone_data.transform.localPosition;
			Quaternion localRotation = target_bone_data.transform.localRotation;

			//Create new bone for the target
			NewAnmBone anmBone = new NewAnmBone();
			anmBone.bonePath = target_bone_data.path;

			//Add the coordinate data
			anmBone.coordinates.AddRange(new float[] { localRotation.x, localRotation.y, localRotation.z, localRotation.w });
			anmBone.coordinates.AddRange(new float[] { localPosition.x, localPosition.y, localPosition.z });

			UnityEngine.Debug.Log(anmBone.coordinates[0] + ", " + anmBone.coordinates[1] + ", " + anmBone.coordinates[2] + ", " + anmBone.coordinates[3] + ", " +
								  anmBone.coordinates[4] + ", " + anmBone.coordinates[5] + ", " + anmBone.coordinates[6]);

			//Loop the children
			for (int index = 0; index < target_bone_data.child_bone_array.Length; ++index)
			{
				NewAnmBone childBone = RecordBones(target_bone_data.child_bone_array[index]);

				//Add heirarchy information -- should never be null unless in future decide to remove non-rexported bones
				if (childBone != null)
				{
					anmBone.childPaths.Add(childBone.bonePath);
				}
			}

			//Add to the frame's dictionary
			bones[anmBone.bonePath] = anmBone;

			//Return in case this was called from child loop
			return anmBone;
		}

		public void RecordSks(Maid maid, Dictionary<MPN, Dictionary<string, float>> sksDict)
		{
			foreach (MPN mpn in sksDict.Keys)
			{
				Dictionary<string, float> baseSks = sksDict[mpn];
				TBodySkin skin = maid.body0.goSlot.Find(slot => slot.m_ParentMPN.Equals(mpn));
				if (skin != null)
				{
					TMorph morph = skin.morph;

					foreach (string key in morph.hash.Keys)
					{
						int f_nIdx = (int)morph.hash[(object)key];

						float absoluteVal = morph.GetBlendValues(f_nIdx);
						float additiveDiff = absoluteVal - baseSks[key];
						float percentDiff = additiveDiff / baseSks[key];

						if (!this.sks.ContainsKey(mpn))
						{
							this.sks[mpn] = new Dictionary<string, NewAnmSks>();
						}
						this.sks[mpn][key] = new NewAnmSks();
						this.sks[mpn][key].name = key;
						this.sks[mpn][key].absoluteValue = absoluteVal;
						this.sks[mpn][key].additiveValue = additiveDiff;
						this.sks[mpn][key].percentValue = percentDiff;
					}
				}
			}
		}

		public void PlaybackBones(CacheBoneDataArray.BoneData target_bone_data, Maid maid)
		{

			string bonePath = target_bone_data.path;
			string[] boneNameSplit = bonePath.Split('/');
			string boneName = boneNameSplit[boneNameSplit.Length - 1];

			if (bones.ContainsKey(bonePath))
			{
				NewAnmBone anmBone = bones[bonePath];
				if (anmBone != null)
				{
					Transform transform = target_bone_data.transform;//(Transform)typeof(CacheBoneDataArray.BoneData).GetField("transform", BindingFlags.Public | BindingFlags.Instance).GetValue(target_bone_data);

					if (transform != null)
					{
						Transform transform2 = maid.body0.GetBone(boneName);
						if (transform2 != null)
						{
							transform.localRotation = new Quaternion(anmBone.coordinates[0], anmBone.coordinates[1], anmBone.coordinates[2], anmBone.coordinates[3]);
							transform2.localRotation = new Quaternion(anmBone.coordinates[0], anmBone.coordinates[1], anmBone.coordinates[2], anmBone.coordinates[3]);

							if (anmBone.coordinates.Count > 4)
							{
								transform.localPosition = new Vector3(anmBone.coordinates[4], anmBone.coordinates[5], anmBone.coordinates[6]);
								transform2.localPosition = new Vector3(anmBone.coordinates[4], anmBone.coordinates[5], anmBone.coordinates[6]);
							}

							typeof(CacheBoneDataArray.BoneData).GetProperty("transform", BindingFlags.Public | BindingFlags.Instance).SetValue(target_bone_data, transform, null);
						}
						else
						{
							UnityEngine.Debug.Log(boneName + " null");
						}
					}
					else
					{
						UnityEngine.Debug.Log("transform null");
					}
				}
				else
				{
					UnityEngine.Debug.Log("bone null");
				}
			}

			for (int index = 0; index < target_bone_data.child_bone_array.Length; ++index)
			{
				PlaybackBones(target_bone_data.child_bone_array[index], maid);
			}
		}
	}

}
