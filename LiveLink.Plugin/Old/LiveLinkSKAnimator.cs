using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin
{
	internal class LiveLinkSKAnimator : MonoSingleton<LiveLinkSKAnimator>
	{
		public static string lastAnmLoadMaidGUID { get; set; }

		public static Dictionary<string, Dictionary<int, string>> channelToSK = new Dictionary<string, Dictionary<int, string>>()
		{
			{
				"default", new Dictionary<int, string>() { {21, "munel" } }
			}
		};

		public void UpdateShapeKey(AnmShapeKeyEvent skEvt)
		{
			if (skEvt == null)
			{
				UnityEngine.Debug.Log("Null skEvt");
			}

			if (GameMain.Instance == null)
			{
				UnityEngine.Debug.Log("Null inst");
			}

			if (GameMain.Instance.CharacterMgr == null)
			{
				UnityEngine.Debug.Log("Null charmgr");
			}
			//Check every maid
			int maidCount = GameMain.Instance.CharacterMgr.GetMaidCount();
			for (int i = 0; i < maidCount; i++)
			{
				//Ensure active
				Maid maid = GameMain.Instance.CharacterMgr.GetMaid(i);
				if (maid != null && maid.isActiveAndEnabled)
				{
					if (maid.status.guid.Equals(skEvt.maidGuid))
					{
						UnityEngine.Debug.Log("Maid Found");

						//Convert channel number to shapekey name
						foreach (Dictionary<int, string> convertDict in channelToSK.Values)
						{
							if (skEvt.skName != null)
							{
								//for now use 0 which is always body -- need to update in future
								TMorph morph = maid.body0.goSlot[0].morph;
								if (morph != null)
								{
									foreach (string key in morph.hash.Keys)
									{
										if (key.Equals(skEvt.skName))
										{
											UnityEngine.Debug.Log("SK Found");
											int f_nIdx = (int)morph.hash[(object)key];
											morph.SetBlendValues(f_nIdx, skEvt.value);
											break;
										}
									}
									morph.FixBlendValues();
								}
								break;
							}
							else
							{
								if (convertDict.ContainsKey(skEvt.channel))
								{
									string skName = convertDict[skEvt.channel];

									UnityEngine.Debug.Log("Conversion Found");

									//for now use 0 which is always body -- need to update in future
									TMorph morph = maid.body0.goSlot[0].morph;
									if (morph != null)
									{
										foreach (string key in morph.hash.Keys)
										{
											if (key.Equals(skName))
											{
												UnityEngine.Debug.Log("SK Found");
												int f_nIdx = (int)morph.hash[(object)key];
												morph.SetBlendValues(f_nIdx, skEvt.value);
												break;
											}
										}
										morph.FixBlendValues();
									}
									break;
								}
							}
						}

						break;
					}
					////Get the body's animation
					//Animation anim = maid.GetAnimation();
					//if(anim != null)
					//{
					//    //Get animation states
					//    bool directMatch = false;
					//    foreach (AnimationState anmState in anim)
					//    {
					//        if(skEvt.evt == null)
					//        {
					//            UnityEngine.Debug.Log("null evt");
					//        }
					//        if(skEvt.evt.animationState == null)
					//        {
					//            UnityEngine.Debug.Log("null evt st");
					//        }
					//        //If it matches the event's animation state -- may have to tweak to be name if reference doesnt work
					//        if (anmState == skEvt.evt.animationState || anmState.name.Equals(skEvt.evt.animationState.name))
					//        {
					//            if(anmState == skEvt.evt.animationState)
					//            {
					//                UnityEngine.Debug.Log("Animation State Matched");
					//            }
					//            else
					//            {
					//                UnityEngine.Debug.Log("Animation State Name Matched....TODO need another way to narrow");
					//            }

					//            //Convert channel number to shapekey name
					//            foreach(Dictionary<int, string> convertDict in channelToSK.Values)
					//            {
					//                if(convertDict.ContainsKey(skEvt.channel))
					//                {
					//                    string skName = convertDict[skEvt.channel];

					//                    //for now use 0 which is always body -- need to update in future
					//                    TMorph morph = maid.body0.goSlot[0].morph;
					//                    if (morph != null)
					//                    {
					//                        foreach (string key in morph.hash.Keys)
					//                        {
					//                            if (key.Equals(skName))
					//                            {
					//                                int f_nIdx = (int)morph.hash[(object)key];
					//                                morph.SetBlendValues(f_nIdx, skEvt.value);
					//                                break;
					//                            }
					//                        }
					//                        morph.FixBlendValues();
					//                    }
					//                    break;
					//                }
					//            }

					//            directMatch = true;
					//            break;
					//        }
					//    }

					//    //If we got the actual state reference no need to check every maid
					//    if(directMatch)
					//    {
					//        break;
					//    }
					//}
					//else
					//{
					//    UnityEngine.Debug.Log("null anim");
					//}
				}
			}
		}

		public class AnmShapeKeyEvent : ScriptableObject
		{
			public int channel { get; set; }
			public float value { get; set; }
			public AnimationEvent evt { get; set; }
			public string maidGuid { get; set; }
			public string skName { get; set; }

			public AnmShapeKeyEvent()
			{
				evt = null;
				maidGuid = null;
				skName = null;
			}
		}
	}

}
