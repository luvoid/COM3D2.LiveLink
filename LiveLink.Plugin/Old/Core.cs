using BepInEx;
using COM3D2API;
using MaidStatus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.LiveLink.Plugin
{
	//[BepInPlugin("org.guest4168.plugins.anmmakerplugin", "Plug-In", "1.0.0.0")]
	public class AnmMaker //: BaseUnityPlugin
	{
		private AnimationState getAnimState(Animation anim, string anmStateName)
		{
			if (anim != null && anmStateName != null && !anmStateName.Equals(""))
			{
				foreach (AnimationState anmState in anim)
				{
					//Match on state name
					if (anmState.name.Equals(anmStateName))
					{
						return anmState;
					}
				}
			}
			return null;
		}

		public static List<float> readBoneFromBinary(string fileName)
		{
			List<float> frameTimes = new List<float>();

			using (AFileBase afileBase = GameUty.FileOpen(fileName))
			{
				if (ImportCM.m_aniTempFile == null)
					ImportCM.m_aniTempFile = new byte[Math.Max(500000, afileBase.GetSize())];
				else if (ImportCM.m_aniTempFile.Length < afileBase.GetSize())
					ImportCM.m_aniTempFile = new byte[afileBase.GetSize()];
				afileBase.Read(ref ImportCM.m_aniTempFile, afileBase.GetSize());
			}

			BinaryReader br = new BinaryReader((Stream)new MemoryStream(ImportCM.m_aniTempFile), System.Text.Encoding.UTF8);
			br.ReadString();
			int int1001 = br.ReadInt32();

			//Read byte
			byte keepGoing = br.ReadByte();

			while (keepGoing == (byte)1)
			{
				//Write bone path
				string bonePath = br.ReadString();
				UnityEngine.Debug.Log("Path " + bonePath);

				while (true)//for (int index1 = 0; index1 < 7; index1++)
				{
					//Write the channel
					byte channel = br.ReadByte();
					if (channel == (byte)1)
					{
						break;
					}
					UnityEngine.Debug.Log("Channel " + channel);

					//Write the number of frames for this bone
					int frames = br.ReadInt32();
					UnityEngine.Debug.Log("Frames" + frames);

					//Write the data for coordinates
					for (int index2 = 0; index2 < frames; ++index2)
					{
						//Write the frame time
						float time = br.ReadSingle();
						UnityEngine.Debug.Log("Time " + time);

						//Write the channel's value for this frame
						float f0 = br.ReadSingle(); //UnityEngine.Debug.Log("f0:" + f0);
						float f1 = br.ReadSingle(); //UnityEngine.Debug.Log("f1:" + f1);
						float f2 = br.ReadSingle(); //UnityEngine.Debug.Log("f2:" + f2);

						if (!frameTimes.Contains(time))
						{
							frameTimes.Add(time);
						}
					}

					//Check if there is another channel
					byte nextChannel = br.ReadByte();
					br.BaseStream.Position = br.BaseStream.Position - 1;
					if (nextChannel == (byte)1)
					{
						break;
					}
				}

				keepGoing = br.ReadByte();
			}

			br.Close();

			frameTimes.Sort();
			return frameTimes;
		}


		private static string[] maidBonesPaths = {
			"ArmL",
			"Bip01",
			"Bip01/Bip01 Footsteps",
			"Bip01/Bip01 Pelvis",
			"Bip01/Bip01 Pelvis/_IK_anal",
			"Bip01/Bip01 Pelvis/_IK_hipL",
			"Bip01/Bip01 Pelvis/_IK_hipR",
			"Bip01/Bip01 Pelvis/_IK_hutanari",
			"Bip01/Bip01 Pelvis/_IK_vagina",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/_IK_thighL",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/_IK_calfL",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/_IK_footL",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe0",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe0/Bip01 L Toe01",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe0/Bip01 L Toe01/Bip01 L Toe0Nub",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe1",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe1/Bip01 L Toe11",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe1/Bip01 L Toe11/Bip01 L Toe1Nub",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe2",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe2/Bip01 L Toe21",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot/Bip01 L Toe2/Bip01 L Toe21/Bip01 L Toe2Nub",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L/momoniku_L",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L/momoniku_L/momoniku_L_nub",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L/momotwist2_L",
			"Bip01/Bip01 Pelvis/Bip01 L Thigh/momotwist_L/momotwist_L_nub",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/_IK_thighR",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/_IK_calfR",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/_IK_footR",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe0",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe0/Bip01 R Toe01",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe0/Bip01 R Toe01/Bip01 R Toe0Nub",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe1",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe1/Bip01 R Toe11",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe1/Bip01 R Toe11/Bip01 R Toe1Nub",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe2",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe2/Bip01 R Toe21",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot/Bip01 R Toe2/Bip01 R Toe21/Bip01 R Toe2Nub",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R/momoniku_R",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R/momoniku_R/momoniku_R_nub",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R/momotwist2_R",
			"Bip01/Bip01 Pelvis/Bip01 R Thigh/momotwist_R/momotwist_R_nub",
			"Bip01/Bip01 Pelvis/Hip_L",
			"Bip01/Bip01 Pelvis/Hip_L/Hip_L_nub",
			"Bip01/Bip01 Pelvis/Hip_R",
			"Bip01/Bip01 Pelvis/Hip_R/Hip_R_nub",
			"Bip01/Bip01 Spine",
			"Bip01/Bip01 Spine/_IK_hara",
			"Bip01/Bip01 Spine/Bip01 Spine0a",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/_IK_UpperArmL",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/_IK_ForeArmL",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/_IK_handL",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger0",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger0/Bip01 L Finger01",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger0/Bip01 L Finger01/Bip01 L Finger02",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger0/Bip01 L Finger01/Bip01 L Finger02/Bip01 L Finger0Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger1",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger1/Bip01 L Finger11",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger1/Bip01 L Finger11/Bip01 L Finger12",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger1/Bip01 L Finger11/Bip01 L Finger12/Bip01 L Finger1Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger2",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger2/Bip01 L Finger21",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger2/Bip01 L Finger21/Bip01 L Finger22",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger2/Bip01 L Finger21/Bip01 L Finger22/Bip01 L Finger2Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger3",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger3/Bip01 L Finger31",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger3/Bip01 L Finger31/Bip01 L Finger32",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger3/Bip01 L Finger31/Bip01 L Finger32/Bip01 L Finger3Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger4",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger4/Bip01 L Finger41",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger4/Bip01 L Finger41/Bip01 L Finger42",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Bip01 L Hand/Bip01 L Finger4/Bip01 L Finger41/Bip01 L Finger42/Bip01 L Finger4Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Foretwist1_L",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Bip01 L Forearm/Foretwist_L",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Uppertwist1_L",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Bip01 L UpperArm/Uppertwist_L",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Kata_L",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 L Clavicle/Kata_L/Kata_L_nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck/Bip01 Head",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck/Bip01 Head/_IK_hohoL",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck/Bip01 Head/_IK_hohoR",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 Neck/Bip01 Head/Bip01 HeadNub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/_IK_UpperArmR",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/_IK_ForeArmR",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/_IK_handR",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger0",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger0/Bip01 R Finger01",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger0/Bip01 R Finger01/Bip01 R Finger02",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger0/Bip01 R Finger01/Bip01 R Finger02/Bip01 R Finger0Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger1",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger1/Bip01 R Finger11",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger1/Bip01 R Finger11/Bip01 R Finger12",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger1/Bip01 R Finger11/Bip01 R Finger12/Bip01 R Finger1Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger2",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger2/Bip01 R Finger21",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger2/Bip01 R Finger21/Bip01 R Finger22",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger2/Bip01 R Finger21/Bip01 R Finger22/Bip01 R Finger2Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger3",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger3/Bip01 R Finger31",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger3/Bip01 R Finger31/Bip01 R Finger32",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger3/Bip01 R Finger31/Bip01 R Finger32/Bip01 R Finger3Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger4",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger4/Bip01 R Finger41",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger4/Bip01 R Finger41/Bip01 R Finger42",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/Bip01 R Finger4/Bip01 R Finger41/Bip01 R Finger42/Bip01 R Finger4Nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Foretwist1_R",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Foretwist_R",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Uppertwist1_R",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Bip01 R UpperArm/Uppertwist_R",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Kata_R",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Bip01 R Clavicle/Kata_R/Kata_R_nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_L",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_L/_IK_muneL",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_L/Mune_L_sub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_L/Mune_L_sub/Mune_L_nub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_R",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_R/_IK_muneR",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_R/Mune_R_sub",
			"Bip01/Bip01 Spine/Bip01 Spine0a/Bip01 Spine1/Bip01 Spine1a/Mune_R/Mune_R_sub/Mune_R_nub",
			"body",
			"center",
			"Hara",
			"MuneL",
			"MuneS",
			"MuneTare",
			"RegFat",
			"RegMeet",

		};

		private static string[] maidBones = {
			"ArmL",
			"Bip01",
			"Bip01 Footsteps",
			"Bip01 Pelvis",
			"_IK_anal",
			"_IK_hipL",
			"_IK_hipR",
			"_IK_hutanari",
			"_IK_vagina",
			"Bip01 L Thigh",
			"_IK_thighL",
			"Bip01 L Calf",
			"_IK_calfL",
			"Bip01 L Foot",
			"_IK_footL",
			"Bip01 L Toe0",
			"Bip01 L Toe01",
			"Bip01 L Toe0Nub",
			"Bip01 L Toe1",
			"Bip01 L Toe11",
			"Bip01 L Toe1Nub",
			"Bip01 L Toe2",
			"Bip01 L Toe21",
			"Bip01 L Toe2Nub",
			"momotwist_L",
			"momoniku_L",
			"momoniku_L_nub",
			"momotwist2_L",
			"momotwist_L_nub",
			"Bip01 R Thigh",
			"_IK_thighR",
			"Bip01 R Calf",
			"_IK_calfR",
			"Bip01 R Foot",
			"_IK_footR",
			"Bip01 R Toe0",
			"Bip01 R Toe01",
			"Bip01 R Toe0Nub",
			"Bip01 R Toe1",
			"Bip01 R Toe11",
			"Bip01 R Toe1Nub",
			"Bip01 R Toe2",
			"Bip01 R Toe21",
			"Bip01 R Toe2Nub",
			"momotwist_R",
			"momoniku_R",
			"momoniku_R_nub",
			"momotwist2_R",
			"momotwist_R_nub",
			"Hip_L",
			"Hip_L_nub",
			"Hip_R",
			"Hip_R_nub",
			"Bip01 Spine",
			"_IK_hara",
			"Bip01 Spine0a",
			"Bip01 Spine1",
			"Bip01 Spine1a",
			"Bip01 L Clavicle",
			"Bip01 L UpperArm",
			"_IK_UpperArmL",
			"Bip01 L Forearm",
			"_IK_ForeArmL",
			"Bip01 L Hand",
			"_IK_handL",
			"Bip01 L Finger0",
			"Bip01 L Finger01",
			"Bip01 L Finger02",
			"Bip01 L Finger0Nub",
			"Bip01 L Finger1",
			"Bip01 L Finger11",
			"Bip01 L Finger12",
			"Bip01 L Finger1Nub",
			"Bip01 L Finger2",
			"Bip01 L Finger21",
			"Bip01 L Finger22",
			"Bip01 L Finger2Nub",
			"Bip01 L Finger3",
			"Bip01 L Finger31",
			"Bip01 L Finger32",
			"Bip01 L Finger3Nub",
			"Bip01 L Finger4",
			"Bip01 L Finger41",
			"Bip01 L Finger42",
			"Bip01 L Finger4Nub",
			"Foretwist1_L",
			"Foretwist_L",
			"Uppertwist1_L",
			"Uppertwist_L",
			"Kata_L",
			"Kata_L_nub",
			"Bip01 Neck",
			"Bip01 Head",
			"_IK_hohoL",
			"_IK_hohoR",
			"Bip01 HeadNub",
			"Bip01 R Clavicle",
			"Bip01 R UpperArm",
			"_IK_UpperArmR",
			"Bip01 R Forearm",
			"_IK_ForeArmR",
			"Bip01 R Hand",
			"_IK_handR",
			"Bip01 R Finger0",
			"Bip01 R Finger01",
			"Bip01 R Finger02",
			"Bip01 R Finger0Nub",
			"Bip01 R Finger1",
			"Bip01 R Finger11",
			"Bip01 R Finger12",
			"Bip01 R Finger1Nub",
			"Bip01 R Finger2",
			"Bip01 R Finger21",
			"Bip01 R Finger22",
			"Bip01 R Finger2Nub",
			"Bip01 R Finger3",
			"Bip01 R Finger31",
			"Bip01 R Finger32",
			"Bip01 R Finger3Nub",
			"Bip01 R Finger4",
			"Bip01 R Finger41",
			"Bip01 R Finger42",
			"Bip01 R Finger4Nub",
			"Foretwist1_R",
			"Foretwist_R",
			"Uppertwist1_R",
			"Uppertwist_R",
			"Kata_R",
			"Kata_R_nub",
			"Mune_L",
			"_IK_muneL",
			"Mune_L_sub",
			"Mune_L_nub",
			"Mune_R",
			"_IK_muneR",
			"Mune_R_sub",
			"Mune_R_nub",
			"body",
			"center",
			"Hara",
			"MuneL",
			"MuneS",
			"MuneTare",
			"RegFat",
			"RegMeet"
		};

		//private static calculateTangents(Vector2 p1, Vector2 p2)
		//{
		//    float tangLengthX = Mathf.Abs(p1.x - p2.x) / 3.0f;
		//    float tangLengthY = tangLengthX;
		//    c1 = p1;
		//    c2 = p2;
		//    c1.x += tangLengthX;
		//    c1.y += tangLengthY * tgOut;
		//    c2.x -= tangLengthX;
		//    c2.y -= tangLengthY * tgIn;

		//    float tangLength = Mathf.Abs(p1.x - p2.x) / 3.0f;
		//    tgOut = (c1.y - p1.y) / tangLength;
		//    tgIn = (p2.y - c2.y) / tangLength;



		//    float d = (p2.x - p1.x) / 3.0f;
		//    float a = 1;//h / w;
		//    Vector2 st = p1 + new Vector2(d, d * a * K1.outTangent);
		//    Vector2 et = p2 + new Vector2(-d, -d * a * K2.inTangent);
		//    Drawing.BezierLineGL(start, st, end, et, Color.red, 20);
		//}
		//private static float getInTangent(float t, float time0, float time1, float val0, float val1)
		//{

		//    float dt = time1 - time0;

		//    t = (t - time0) / dt;

		//    float t2 = t * t;
		//    float t3 = t2 * t;

		//    float a = 2 * t3 - 3 * t2 + 1;
		//    float b = t3 - 2 * t2 + t;
		//    float c = t3 - t2;
		//    float d = -2 * t3 + 3 * t2;

		//    float m0 = keyframe0.outTangent * dt;
		//    float m1 = keyframe1.inTangent * dt;

		//    float outT = m0 / dt;
		//    float inT = m1 / dt;



		//    //return a * val0 + b * m0 + c * m1 + d * val1;



		//    (2 * .0000029427665140246972)/ 0.0000000298023224 = ((1 - t) * (1 - c) * (1 + b)) * (-1) + ((1 - t) * (1 + c) * (1 - b));
		//}
	}
}