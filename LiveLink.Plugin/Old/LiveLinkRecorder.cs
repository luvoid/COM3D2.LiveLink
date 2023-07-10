using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static COM3D2.LiveLink.Plugin.AnmMaker;

namespace COM3D2.LiveLink.Plugin
{
	internal class LiveLinkRecorder : MonoSingleton<LiveLinkRecorder>
	{
		public static Maid RecordingMaid;

		public static bool autoPoseNext = false;
		public static bool autoSnapNext = false;
		public static bool captureBones = false;
		public static bool captureSks   = false;

		private void Update()
		{
			// Automatic frame capturing
			AutoCaptureFrame();

			RecordCaptureFrame();
		}

		private void AutoCaptureFrame()
		{
			/*
			// Order is important, must check snap before pose to ensure pose is triggered on separate frame from snap on first Update
			if (autoSnapNext)
			{
				autoSnapNext = false;

				// Record the Maid's data
				NewAnmFrame newFrame = new NewAnmFrame();
				newFrame.timestamp = DateTime.Now.ToString("yy-MM-dd HH:mm:ss:f");
				newFrame.frameTime = autoFrames[0];
				newFrame.RecordBones(new CacheBoneDataArray.BoneData(recordingMaid.body0.GetBone("Bip01"), null));

				//Add to the list
				newAnm.frames.Add(newFrame);

				//Remove the autoframe
				autoFrames.RemoveAt(0);

				//Check if another frame waits
				if (autoFrames.Count > 0)
				{
					autoPoseNext = true;
				}
			}

			if (autoPoseNext)
			{
				autoPoseNext = false;

				//Determine who the auto frames are from
				Maid autoFrameMaidOrMan = (baseMaidMan != null) ? baseMaidMan : recordingMaid;
				//String anmStateName = (baseMaidMan != null) ? anmStateNameMan : anmStateNameMaid;
				String anmStateName = anmAnimationSelectedName;

				//Find the animation in the cache
				Animation anim = autoFrameMaidOrMan.GetAnimation();
				if (anim != null)
				{
					foreach (AnimationState anmState in anim)
					{
						//Match on state name
						if (anmState.name.Equals(anmStateName))
						{
							//Set the time according to the frame data
							anmState.time = autoFrames[0];

							//Sample
							anmState.enabled = true;
							anim.Sample();
							anmState.enabled = false;

							//Tell next Update to take a snapshot
							autoSnapNext = true;
							break;
						}
					}
				}
			}

			if (timeStepPressed)
			{
				timeStepPressed = false;

				//Prep data
				newAnm.frames.Clear();
				autoFrames = new List<float>();

				//Determine who the time step frames are from
				Maid autoFrameMaidOrMan = (baseMaidMan != null) ? baseMaidMan : recordingMaid;
				//String anmStateName = (baseMaidMan != null) ? anmStateNameMan : anmStateNameMaid;
				String anmStateName = anmAnimationSelectedName;

				//Find the animation in the cache
				Animation anim = autoFrameMaidOrMan.GetAnimation();
				if (anim != null)
				{
					foreach (AnimationState anmState in anim)
					{
						//Match on state name
						if (anmState.name.Equals(anmStateName))
						{
							//Build a list of frame times to record
							int frame = 0;
							bool keepGoing = true;
							while (keepGoing)
							{
								float nextFrameTime = Math.Min(anmState.length, anmState.time + (frame * float.Parse(timeStepText)));
								autoFrames.Add(nextFrameTime);
								frame = frame + 1;
								keepGoing = (nextFrameTime < anmState.length);
							}
							autoPoseNext = true;
							break;
						}
					}
				}
			}
			*/
		}
	
		private void RecordCaptureFrame()
		{
			/*
			if (captureBones || captureSks)
			{
				captureBones = false;
				captureSks = false;
			
				//Record the Maid's data
				NewAnmFrame newFrame = new NewAnmFrame();
				newFrame.timestamp = DateTime.Now.ToString("yy-MM-dd HH:mm:ss:f");
				newFrame.frameTime = newAnm.frames.Count == 0 ? 0.0f : (newAnm.frames[newAnm.frames.Count - 1].frameTime + 0.1f);
			
				if (captureBones)
				{
					newFrame.RecordBones(new CacheBoneDataArray.BoneData(recordingMaid.body0.GetBone("Bip01"), null));
				}
				if (captureSks)
				{
					newFrame.RecordSks(recordingMaid, recordMaidBaseSks);
				}
			
				//Add to the list
				newAnm.frames.Add(newFrame);
			}
			*/
		}
	}
}
