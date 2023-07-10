using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin
{
	internal static class UIHelper
	{
		private static List<string> _animations;
		private static List<string> animations
		{
			get
			{
				if (_animations == null)
				{
					_animations = new List<string>();
					_animations.AddRange(GameUty.FileSystem.GetList("motion", AFileSystemBase.ListType.AllFile));
					for (int i = _animations.Count - 1; i >= 0; i--)
					{
						if (!_animations[i].EndsWith(".anm"))
						{
							_animations.RemoveAt(i);
						}
					}
					_animations.Sort();
				}

				return _animations;
			}
		}
		public static void UI_5_DrawMotionControls(Maid maidMan, ref string anmAnimationSearch, ref string anmAnimationFilter, ref Vector2 anmAnimationScroll, ref string anmAnimationSelectedName, ref string anmTimeText, ref string timeStepText, ref bool timeStepPressed, ref List<float> autoFrames, ref bool autoPoseNext, ref Structs.NewAnm newAnm, ref string infoMessage, int iconWidth)
		{
			Animation anim = maidMan.GetAnimation();
			AnimationState animState = null;

			// Animation selection
			{
				GUILayout.Label("Animation:");
				GUILayout.BeginHorizontal();
				{
					//Setup
					anmAnimationSearch = (anmAnimationSearch == null) ? "" : anmAnimationSearch;

					//Search text
					anmAnimationSearch = GUILayout.TextField(anmAnimationSearch);

					//Search Button
					if (GUILayout.Button("Search"))
					{
						anmAnimationFilter = anmAnimationSearch;
					}
				}
				GUILayout.EndHorizontal();

				// List of Animations
				anmAnimationScroll = GUILayout.BeginScrollView(anmAnimationScroll, false, true, GUILayout.Width((iconWidth * 5) + 20), GUILayout.Height(Screen.height * 1 / 8));
				{
					for (int i = 0; i < animations.Count; i++)
					{
						string nextAnmPath = animations[i];
						string[] splitAnmPath = nextAnmPath.Split('\\');
						if (anmAnimationFilter != null && !anmAnimationFilter.Equals("") && nextAnmPath.Contains(anmAnimationFilter))
						{
							if (GUILayout.Button(nextAnmPath))
							{
								if (anim != null)
								{
									anmAnimationSelectedName = splitAnmPath[splitAnmPath.Length - 1];
									maidMan.body0.CrossFade(splitAnmPath[splitAnmPath.Length - 1], GameUty.FileSystem, false, true);
								}
								else
								{
									anmAnimationSelectedName = null;
								}
							}
						}
					}
				}
				GUILayout.EndScrollView();
			}

			//GUI ENABLE
			{
				GUI.enabled = false;
				//If an animation was selected and it has been loaded into the Animation we can enable the following UI
				if (anmAnimationSelectedName != null && !anmAnimationSelectedName.Equals(""))
				{
					//First check if animation exists
					if (anim != null)
					{
						foreach (AnimationState anmState in anim)
						{
							if (anmState.name.Equals(anmAnimationSelectedName))
							{
								animState = anmState;
								GUI.enabled = true;
								break;
							}
						}
					}

				}
			}

			//Timeline
			GUILayout.Label("Jump to Time:");
			GUILayout.BeginHorizontal();
			{
				//Setup
				anmTimeText = (anmTimeText == null) ? "0.00" : anmTimeText;
				float maxTime = (animState == null) ? 0f : animState.length;

				//Text Edit
				anmTimeText = GUILayout.TextField(anmTimeText);
				GUILayout.Label(@" / " + maxTime + " seconds");

				//Apply button
				if (GUILayout.Button("Apply"))
				{
					if (System.Text.RegularExpressions.Regex.IsMatch(anmTimeText, @"[\+]?\d*\.?\d*") && float.Parse(anmTimeText) >= 0)
					{
						anmTimeText = Math.Min(float.Parse(anmTimeText), maxTime).ToString();
						animState.time = float.Parse(anmTimeText);
						animState.enabled = true;
						anim.Sample();
						animState.enabled = false;
					}
					else
					{
						infoMessage = "Invalid Frame Time float";
					}
				}
			}
			GUILayout.EndHorizontal();

			//Pause
			{
				if (animState == null)
				{
					GUILayout.Toggle(false, "Pause");
				}
				else
				{
					animState.enabled = !GUILayout.Toggle(!animState.enabled, "Pause");
				}
			}

			//Time-Step Frames
			GUILayout.Label("Increment-Snap From: " + ((animState == null) ? "" : (animState.time % animState.length).ToString()));
			GUILayout.BeginHorizontal();
			{
				//Setup
				timeStepText = (timeStepText == null) ? "0.00" : timeStepText;

				//Text Edit
				GUILayout.Label("Increment-Snap Every: ");
				timeStepText = GUILayout.TextField(timeStepText);
				GUILayout.Label(" Seconds");

				//Apply button
				if (GUILayout.Button("Auto-Step"))
				{
					if (System.Text.RegularExpressions.Regex.IsMatch(timeStepText, @"[\+]?\d*\.?\d*") && float.Parse(timeStepText) > 0)
					{
						timeStepPressed = true;
					}
					else
					{
						infoMessage = "Invalid Time-Step float";
					}
				}
			}
			GUILayout.EndHorizontal();

			//Auto-Frame
			if (maidMan.boMAN)
			{
				//Auto build frames from man's anm file
				if (GUILayout.Button("Auto-Snap Using Man .anm Frames"))
				{
					newAnm.frames.Clear();
					autoFrames = AnmMaker.readBoneFromBinary(anmAnimationSelectedName);
					autoPoseNext = true;
				}
			}
			GUI.enabled = true;

			//Old
			{
				//Animation to pick

				//AnimationState animState = null;
				//bool temp;

				//#region
				//if (maidMan != null)
				//{
				//    anim = maidMan.GetAnimation();

				//    //First check if animation is playing
				//    if (anim != null)
				//    {
				//        foreach (AnimationState anmState in anim)
				//        {
				//            //Take the first enabled animation as the one we want
				//            if (anmState.enabled)
				//            {
				//                animState = anmState;
				//                anmStateName = anmState.name;
				//                anmPause = false;
				//                break;
				//            }
				//        }
				//    }

				//    //If no animation is playing, check if we paused an animation
				//    if (animState == null && anmStateName != null)
				//    {
				//        if (anim != null)
				//        {
				//            foreach (AnimationState anmState in anim)
				//            {
				//                if (anmState.name.Equals(anmStateName))
				//                {
				//                    animState = anmState;
				//                    break;
				//                }
				//            }
				//        }
				//    }
				//}

				//anmTimeText = (anmTimeText == null)? "0.00" : anmTimeText;
				//float maxTime = (animState != null)? animState.length : 0f;
				//GUI.enabled = (anim != null && animState != null);

				//#endregion

				//Pause
				//{
				//    temp = GUI.enabled;
				//    if (!anmPause)
				//    {
				//        GUI.enabled = (anim != null && animState != null);
				//    }

				//    //Toggle
				//    anmPause = GUILayout.Toggle(anmPause, "PAUSE");
				//    {
				//        //If pause was just pressed
				//        if (anmPause)
				//        {
				//            //Check the animation cache
				//            foreach (AnimationState anmState in anim)
				//            {
				//                //Disable every animation as double check
				//                if (anmState.enabled)
				//                {
				//                    anmState.enabled = false;
				//                    anmPause = true;
				//                    anmStateName = anmState.name;
				//                }
				//            }
				//        }
				//        else
				//        {
				//            //If resume was just pressed, this is harder
				//            //you dont actually know which animation in the cache is playing if its already paused
				//            animState.enabled = true;
				//            anmPause = false;
				//            anmStateName = null;
				//        }
				//    }

				//    GUI.enabled = temp;
				//}

				////Time-Step Frames
				//{
				//    temp = GUI.enabled;
				//    GUI.enabled = (anim != null && animState != null);

				//    //Text Edit
				//    timeStepText = GUILayout.TextField(timeStepText);

				//    //Apply button
				//    if (GUILayout.Button("Time-Step"))
				//    {
				//        if (System.Text.RegularExpressions.Regex.IsMatch(timeStepText, @"[\+]?\d*\.?\d*"))
				//        {
				//            timeStepPressed = true;
				//        }
				//        else
				//        {
				//            infoMessage = "Invalid Time-Step float";
				//        }
				//    }

				//    GUI.enabled = temp;
				//}

				//GUI.enabled = true;
			}
		}
	}
}
