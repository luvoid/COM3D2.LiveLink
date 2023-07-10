using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin
{
	internal static class Hooks
	{
		private static bool initialized;
		private static HarmonyLib.Harmony instance;

		public static void Initialize()
		{
			//Copied from examples
			if (Hooks.initialized)
				return;

			Hooks.instance = Harmony.CreateAndPatchAll(typeof(Hooks), "org.guest4168.anmmakerplugin.hooks.base");
			Hooks.initialized = true;

			UnityEngine.Debug.Log("Anm Maker: Hooks Initialize");
		}

		[HarmonyPatch(typeof(TBody), nameof(TBody.LoadBody_R))]
		[HarmonyPostfix]
		public static void TBody_LoadBody_R(TBody __instance, string f_strModelFileName, Maid f_maid)
		{
			if (__instance.m_Bones != null)
			{
				__instance.m_Bones.AddComponent<LiveLinkSKAnimator>();
			}
		}

		[HarmonyPatch(typeof(TBody), "CacheLoadAnime", new Type[] { typeof(AFileSystemBase), typeof(string), typeof(bool), typeof(bool) })]
		[HarmonyPrefix]
		public static bool TBody_CacheLoadAnime_Pre(TBody __instance, AFileSystemBase fileSystem, string filename, bool load_mune_l, bool load_mune_r)
		{
			LiveLinkSKAnimator.lastAnmLoadMaidGUID = __instance.maid.status.guid;
			UnityEngine.Debug.Log("LoadAnime_Pre 1 Maid GUID:" + LiveLinkSKAnimator.lastAnmLoadMaidGUID);
			return true;

			//string animeTag = this.GetAnimeTag(filename);
			//Animation animation = this.m_Animation;
			//AnimationClip clip1 = animation.GetClip(animeTag);
			//byte num1 = 0;
			//byte num2 = 0;
			//bool flag = this.m_AnimCache.TryGetValue(animeTag, out num1);
			//byte num3 = (byte)((int)(byte)((int)num2 | (!load_mune_l ? 0 : 1)) | (!load_mune_r ? 0 : 2));
			//if (!((UnityEngine.Object)clip1 == (UnityEngine.Object)null) && flag && ((int)num1 == (int)num3 && !this.m_bForceReloadAnime))
			//    return;
			//AnimationClip clip2 = ImportCM.LoadAniClipNative(fileSystem, filename, ((int)num3 & 1) != 0, ((int)num3 & 2) != 0, false);
			//if ((UnityEngine.Object)clip2 == (UnityEngine.Object)null)
			//    return;
			//animation.AddClip(clip2, animeTag);
			//if (animeTag.Contains("_l_"))
			//{
			//    for (int index = 2; index <= 8; ++index)
			//    {
			//        if (animeTag.Contains("_l_" + index.ToString() + "_"))
			//        {
			//            animation[animeTag].layer = index;
			//            break;
			//        }
			//    }
			//}
			//this.m_AnimCache[animeTag] = num3;
		}

		[HarmonyPatch(typeof(TBody), "CacheLoadAnime", new Type[] { typeof(AFileSystemBase), typeof(string), typeof(bool), typeof(bool) })]
		[HarmonyPostfix]
		public static void TBody_CacheLoadAnime_Post(TBody __instance, AFileSystemBase fileSystem, string filename, bool load_mune_l, bool load_mune_r)
		{
			LiveLinkSKAnimator.lastAnmLoadMaidGUID = null;
			UnityEngine.Debug.Log("LoadAnime_Post 1 Maid GUID:" + LiveLinkSKAnimator.lastAnmLoadMaidGUID);
		}

		[HarmonyPatch(typeof(TBody), "LoadAnime", new Type[] { typeof(string), typeof(AFileSystemBase), typeof(string), typeof(bool), typeof(bool) })]
		[HarmonyPrefix]
		public static bool TBody_LoadAnime_Pre(TBody __instance, ref AnimationState __result, string tag, AFileSystemBase fileSystem, string filename, bool additive, bool loop)
		{
			LiveLinkSKAnimator.lastAnmLoadMaidGUID = __instance.maid.status.guid;
			UnityEngine.Debug.Log("LoadAnime_Pre 2 Maid GUID:" + LiveLinkSKAnimator.lastAnmLoadMaidGUID);
			return true;

			//if ((UnityEngine.Object)__instance.m_Bones == (UnityEngine.Object)null)
			//    UnityEngine.Debug.LogError((object)("未だキャラがロードさていません。" + __instance.gameObject.name));
			//Animation animation = __instance.m_Animation;
			//AnimationClip clip1 = animation.GetClip(tag);
			//byte num1 = 0;
			//byte num2 = 0;
			//bool flag = __instance.m_AnimCache.TryGetValue(tag, out num1);
			//byte num3 = (byte)((int)(byte)((int)num2 | (!((UnityEngine.Object)__instance.jbMuneL != (UnityEngine.Object)null) || (double)__instance.jbMuneL.BlendValueON != 0.0 ? 0 : 1)) | (!((UnityEngine.Object)__instance.jbMuneR != (UnityEngine.Object)null) || (double)__instance.jbMuneR.BlendValueON != 0.0 ? 0 : 2));
			//if ((UnityEngine.Object)clip1 == (UnityEngine.Object)null || !flag || ((int)num1 != (int)num3 || __instance.m_bForceReloadAnime))
			//{
			//    AnimationClip clip2 = ImportCM.LoadAniClipNative(fileSystem, filename, ((int)num3 & 1) != 0, ((int)num3 & 2) != 0, false);
			//    if ((UnityEngine.Object)clip2 == (UnityEngine.Object)null)
			//        return (AnimationState)null;
			//    animation.AddClip(clip2, tag);
			//    if (tag.Contains("_l_"))
			//    {
			//        for (int index = 2; index <= 8; ++index)
			//        {
			//            if (tag.Contains("_l_" + index.ToString() + "_"))
			//            {
			//                animation[tag].layer = index;
			//                break;
			//            }
			//        }
			//    }
			//    __instance.m_AnimCache[tag] = num3;
			//}
			//__instance.LastAnimeFN = filename;
			//AnimationState animationState = animation[tag];
			//animationState.blendMode = !additive ? AnimationBlendMode.Blend : AnimationBlendMode.Additive;
			//animationState.wrapMode = !loop ? WrapMode.Once : WrapMode.Loop;
			//animationState.speed = 1f;
			//animationState.time = 0.0f;
			//animationState.weight = 0.0f;
			//animationState.enabled = false;
			//return animationState;
		}

		[HarmonyPatch(typeof(TBody), "LoadAnime", new Type[] { typeof(string), typeof(AFileSystemBase), typeof(string), typeof(bool), typeof(bool) })]
		[HarmonyPostfix]
		public static void TBody_LoadAnime_Post(TBody __instance, ref AnimationState __result, string tag, AFileSystemBase fileSystem, string filename, bool additive, bool loop)
		{
			LiveLinkSKAnimator.lastAnmLoadMaidGUID = null;
			UnityEngine.Debug.Log("LoadAnime_Post 2 Maid GUID:" + LiveLinkSKAnimator.lastAnmLoadMaidGUID);
		}

		[HarmonyPatch(typeof(TBody), "LoadAnime", new Type[] { typeof(string), typeof(byte[]), typeof(bool), typeof(bool) })]
		[HarmonyPrefix]
		public static bool TBody_LoadAnime_Pre(TBody __instance, ref AnimationState __result, string tag, byte[] byte_data, bool additive, bool loop)
		{
			LiveLinkSKAnimator.lastAnmLoadMaidGUID = __instance.maid.status.guid;
			UnityEngine.Debug.Log("LoadAnime_Pre 3 Maid GUID:" + LiveLinkSKAnimator.lastAnmLoadMaidGUID);
			return true;

			//if ((UnityEngine.Object)__instance.m_Bones == (UnityEngine.Object)null)
			//    UnityEngine.Debug.LogError((object)("未だキャラがロードさていません。" + __instance.gameObject.name));
			//Animation animation = __instance.m_Animation;
			//animation.GetClip(tag);
			//AnimationClip clip = ImportCM.LoadAniClipNative(byte_data, true, true, false);
			//animation.AddClip(clip, tag);
			//if (tag.Contains("_l_"))
			//{
			//    for (int index = 2; index <= 8; ++index)
			//    {
			//        if (tag.Contains("_l_" + index.ToString() + "_"))
			//        {
			//            animation[tag].layer = index;
			//            break;
			//        }
			//    }
			//}
			//__instance.LastAnimeFN = tag;
			//AnimationState animationState = animation[tag];
			//animationState.blendMode = !additive ? AnimationBlendMode.Blend : AnimationBlendMode.Additive;
			//animationState.wrapMode = !loop ? WrapMode.Once : WrapMode.Loop;
			//animationState.speed = 1f;
			//animationState.time = 0.0f;
			//animationState.weight = 0.0f;
			//animationState.enabled = false;
			//__result = animationState;

			//return false;
		}

		[HarmonyPatch(typeof(TBody), "LoadAnime", new Type[] { typeof(string), typeof(byte[]), typeof(bool), typeof(bool) })]
		[HarmonyPostfix]
		public static void TBody_LoadAnime_Post(TBody __instance, ref AnimationState __result, string tag, byte[] byte_data, bool additive, bool loop)
		{
			LiveLinkSKAnimator.lastAnmLoadMaidGUID = null;
			UnityEngine.Debug.Log("LoadAnime_Post 3 Maid GUID:" + LiveLinkSKAnimator.lastAnmLoadMaidGUID);
		}

		[HarmonyPatch(typeof(Maid), "AnimationObject", new Type[] { typeof(string), typeof(string), typeof(bool), typeof(bool) })]
		[HarmonyPrefix]
		public static bool Maid_AnimationObject_Pre(Maid __instance, string f_strName, string f_strAnimName, bool f_bNowPlay, bool f_bLoop)
		{
			LiveLinkSKAnimator.lastAnmLoadMaidGUID = __instance.status.guid;
			UnityEngine.Debug.Log("Maid_AnimationObject 1 Maid GUID:" + LiveLinkSKAnimator.lastAnmLoadMaidGUID);
			return true;
		}

		[HarmonyPatch(typeof(Maid), "AnimationObject", new Type[] { typeof(string), typeof(string), typeof(bool), typeof(bool) })]
		[HarmonyPostfix]
		public static void Maid_AnimationObject_Post(Maid __instance, string f_strName, string f_strAnimName, bool f_bNowPlay, bool f_bLoop)
		{
			LiveLinkSKAnimator.lastAnmLoadMaidGUID = null;
			UnityEngine.Debug.Log("Maid_AnimationObject 1 Maid GUID:" + LiveLinkSKAnimator.lastAnmLoadMaidGUID);
		}


		[HarmonyPatch(typeof(AnmParse), "CreateAnmClip")]
		[HarmonyPrefix]
		public static bool AnmParse_CreateAnmClip(AnmParse __instance, ref AnimationClip __result)
		{
			if (!DLLAnmParse.IsValid(ref __instance.data_))
			{
				__result = (AnimationClip)null;
				return false;
			}
			AnimationClip animationClip = new AnimationClip();
			if ((UnityEngine.Object)animationClip != (UnityEngine.Object)null)
			{
				animationClip.legacy = true;
			}
			System.Type type = typeof(Transform);
			IntPtr animationCurveDataList = DLLAnmParse.GetAnimationCurveDataList(ref __instance.data_);
			int length1 = DLLAnmParse.AnimationCurveDataList_Size(animationCurveDataList);
			AnimationCurve[] animationCurveArray = new AnimationCurve[length1];
			DLLAnmParse.AnmKeyFrame dest = new DLLAnmParse.AnmKeyFrame();
			for (int pos1 = 0; pos1 < length1; ++pos1)
			{
				IntPtr curve_data = DLLAnmParse.AnimationCurveDataList_At(animationCurveDataList, pos1);
				string bonePath = Marshal.PtrToStringAnsi(DLLAnmParse.AnimationCurveData_GetPath(curve_data));
				int num = DLLAnmParse.AnimationCurveData_ComID(curve_data);
				IntPtr key_framelist = DLLAnmParse.AnimationCurveData_KeyFrameList(curve_data);
				int length2 = DLLAnmParse.KeyFrameList_Size(key_framelist);
				Keyframe[] keyframeArray = new Keyframe[length2];
				for (int pos2 = 0; pos2 < length2; ++pos2)
				{
					DLLAnmParse.KeyFrameList_At(key_framelist, pos2, ref dest);
					keyframeArray[pos2].time = dest.time;
					keyframeArray[pos2].value = dest.value;
					keyframeArray[pos2].inTangent = dest.in_tangent;
					keyframeArray[pos2].outTangent = dest.out_tangent;
				}
				// Custom channels for shapekeys
				if ((num - 100) > 20 || bonePath.StartsWith("SK_"))
				{
					for (int i = 0; i < length2; i++)
					{
						UnityEngine.Debug.Log("AnmParse Secret Channel:" + num);

						//Get the next keyframe
						Keyframe kf = keyframeArray[i];

						//Create Unity AnimationEvent
						AnimationEvent evt = new AnimationEvent();
						evt.time = kf.time;
						evt.functionName = "UpdateShapeKey";

						//Create ShapeKeyEvent
						LiveLinkSKAnimator.AnmShapeKeyEvent skEvt = ScriptableObject.CreateInstance<LiveLinkSKAnimator.AnmShapeKeyEvent>();
						skEvt.skName = bonePath.Substring(3);
						skEvt.channel = (num - 100);
						skEvt.value = kf.value;
						//skEvt.evt = evt;
						skEvt.maidGuid = LiveLinkSKAnimator.lastAnmLoadMaidGUID;

						UnityEngine.Debug.Log("AnmParse Maid GUID:" + LiveLinkSKAnimator.lastAnmLoadMaidGUID);

						//if (evt.animatorStateInfo != null)
						//{
						//    UnityEngine.Debug.Log("AnimationStateInfo:" + evt.animatorStateInfo.fullPathHash);
						//    UnityEngine.Debug.Log("AnimationStateInfo:" + evt.animatorStateInfo.shortNameHash);
						//    UnityEngine.Debug.Log("AnimationStateInfo:" + evt.animatorStateInfo.tagHash);
						//}

						//if (evt.animatorClipInfo != null && evt.animatorClipInfo.clip != null)
						//{
						//    UnityEngine.Debug.Log("AnimationState:" + evt.animatorClipInfo.clip.name);
						//}

						//Set the parameter that gets sent to the function
						evt.objectReferenceParameter = skEvt;

						animationClip.AddEvent(evt);
					}
				}
				else
				{
					animationCurveArray[pos1] = new AnimationCurve(keyframeArray);
					animationClip.SetCurve(bonePath, AnmParse.proptypes[num - 100], AnmParse.properties[num - 100], animationCurveArray[pos1]);
				}
			}
			DLLAnmParse.Clear(ref __instance.data_);
			__result = animationClip;
			return false;
		}


		//[HarmonyPatch(typeof(TBody), "LoadAniClip")]
		//[HarmonyPrefix]
		//public static bool AnmParse_LoadAniClip(AnmParse __instance, AnimationClip __result, byte[] file_byte, bool load_l_mune_anime, bool load_r_mune_anime, bool no_chara = false)
		//{
		//    if (ImportCM.m_aniTempFile == null)
		//        ImportCM.m_aniTempFile = new byte[Math.Max(500000, file_byte.Length)];
		//    else if (ImportCM.m_aniTempFile.Length < file_byte.Length)
		//        ImportCM.m_aniTempFile = new byte[file_byte.Length];
		//    Buffer.BlockCopy((Array)file_byte, 0, (Array)ImportCM.m_aniTempFile, 0, file_byte.Length);

		//    BinaryReader binaryReader = new BinaryReader((Stream)new MemoryStream(ImportCM.m_aniTempFile), System.Text.Encoding.UTF8);
		//    string formatStr = binaryReader.ReadString();
		//    if (formatStr != "CM3D2_ANIM")
		//    {
		//        NDebug.Assert("LoadAniClip 例外 : ヘッダーファイルが不正です。" + formatStr, false);
		//    }
		//    int int1001 = binaryReader.ReadInt32();

		//    AnimationClip animationClip = new AnimationClip();
		//    if ((UnityEngine.Object)animationClip != (UnityEngine.Object)null)
		//    {
		//        animationClip.legacy = true;
		//    }

		//    //Read byte
		//    byte keepGoing = binaryReader.ReadByte();

		//    while (keepGoing == (byte)1)
		//    {
		//        //Write bone path
		//        string bonePath = binaryReader.ReadString();
		//        UnityEngine.Debug.Log("Path " + bonePath);

		//        while (true)
		//        {
		//            //Write the channel
		//            byte channel = binaryReader.ReadByte();
		//            if (channel == (byte)1)
		//            {
		//                break;
		//            }
		//            UnityEngine.Debug.Log("Channel " + channel);

		//            //Write the number of frames for this bone
		//            int frames = binaryReader.ReadInt32();
		//            UnityEngine.Debug.Log("Frames " + frames);

		//            AnimationCurve curve = new AnimationCurve();
		//            Keyframe[] keyframeArray = new Keyframe[frames];

		//            //Write the data for coordinates
		//            for (int index2 = 0; index2 < frames; ++index2)
		//            {
		//                //Write the frame time
		//                float time = binaryReader.ReadSingle(); //UnityEngine.Debug.Log("Time " + time);

		//                //Write the channel's value for this frame
		//                float f0 = binaryReader.ReadSingle(); //UnityEngine.Debug.Log("f0:" + f0);
		//                float f1 = binaryReader.ReadSingle(); //UnityEngine.Debug.Log("f1:" + f1);
		//                float f2 = binaryReader.ReadSingle(); //UnityEngine.Debug.Log("f2:" + f2);

		//                keyframeArray[index2].time = time;
		//                keyframeArray[index2].value = f0;
		//                keyframeArray[index2].inTangent = f1;
		//                keyframeArray[index2].outTangent = f2;
		//            }
		//            curve.keys = keyframeArray;

		//            if (channel > 20)
		//            {
		//                animationClip.SetCurve(bonePath, AnmParse.proptypes[(int)channel - 100], AnmParse.properties[(int)channel - 100], curve);
		//            }
		//            else
		//            {
		//                animationClip.SetCurve(bonePath, AnmParse.proptypes[(int)channel - 100], AnmParse.properties[(int)channel - 100], curve);
		//            }

		//            //Check if there is another channel
		//            byte nextChannel = binaryReader.ReadByte();
		//            binaryReader.BaseStream.Position = binaryReader.BaseStream.Position - 1;
		//            if (nextChannel == (byte)1)
		//            {
		//                break;
		//            }
		//        }

		//        keepGoing = binaryReader.ReadByte();
		//    }

		//    binaryReader.Close();

		//    int num1 = 0;
		//    while (true)
		//    {
		//        byte num2;
		//        AnimationCurve curve = new AnimationCurve();
		//        bool flag = false;
		//        do
		//        {
		//            string str2 = "";
		//            do
		//            {
		//                num2 = binaryReader.ReadByte();
		//                if (num2 != (byte)0)
		//                {
		//                    if (num2 == (byte)1)
		//                    {
		//                        relativePath = binaryReader.ReadString();
		//                        ++num1;
		//                    }
		//                    else if (num2 >= (byte)100)
		//                    {

		//                        flag = true;
		//                        str2 = relativePath + "*";
		//                    }
		//                    else
		//                        goto label_25;
		//                }
		//                else
		//                    goto label_26;
		//            }
		//            while (no_chara);
		//            if (!no_chara && 
		//                num2 >= (byte)104 && 
		//                (num2 <= (byte)106 && !str2.Contains("Bip01*")) && 
		//                (!str2.Contains("_IK_") && !str2.Contains("ManBip*") && 
		//                (!str2.Contains("Hip_L") && !str2.Contains("Hip_R"))))
		//                flag = false;
		//            if (!load_l_mune_anime && str2.Contains("Mune_L"))
		//                flag = false;
		//            if (!load_r_mune_anime && str2.Contains("Mune_R"))
		//                flag = false;
		//        }
		//        while (!flag);
		//        animationClip.SetCurve(relativePath, AnmParse.proptypes[(int)num2 - 100], AnmParse.properties[(int)num2 - 100], curve);
		//        continue;
		//    label_25:
		//        Debug.LogError((object)("com " + (object)num2));
		//    }
		//label_26:
		//    binaryReader.Close();

		//    __result = animationClip;

		//    return false;
		//}

		//maid.body0.CrossFade("maid_stand01.json", GameUty.FileSystem, false, true)

		//[HarmonyPatch(typeof(TBody), nameof(TBody.LoadAnime), new Type[] { typeof(string), typeof(AFileSystemBase), typeof(string), typeof(bool), typeof(bool) })]
		//[HarmonyPrefix]
		//public static bool TBody_LoadAnime(string tag, AFileSystemBase fileSystem, string filename, bool additive, bool loop, TBody __instance, UnityEngine.AnimationState __result)
		//{
		//    //default functionality
		//    if (!filename.EndsWith(".json"))
		//    {
		//        return true;
		//    }


		//    if ((UnityEngine.Object)__instance.m_Bones == (UnityEngine.Object)null)
		//        UnityEngine.Debug.LogError((object)("未だキャラがロードさていません。" + __instance.gameObject.name));

		//    //Check if this clip is already in the body's animation via its tag
		//    UnityEngine.Animation animation = __instance.m_Animation;
		//    UnityEngine.AnimationClip clip1 = animation.GetClip(tag);

		//    var crv = AnimationCurve.EaseInOut(0.6666666865348816f, 0.15053583681583405f, 0.7333333492279053f, 0.15053585171699524f )
		//    //byte num1 = 0;
		//    byte num2 = 0;
		//    //bool flag = __instance.m_AnimCache.TryGetValue(tag, out num1);
		//    byte num3 = (byte)((int)(byte)((int)num2 | (!((UnityEngine.Object)__instance.jbMuneL != (UnityEngine.Object)null) || (double)__instance.jbMuneL.BlendValueON != 0.0 ? 0 : 1)) | (!((UnityEngine.Object)__instance.jbMuneR != (UnityEngine.Object)null) || (double)__instance.jbMuneR.BlendValueON != 0.0 ? 0 : 2));

		//    //gonna treat it like forcereloadanimation for now
		//    if (true)//((UnityEngine.Object)clip1 == (UnityEngine.Object)null || !flag || ((int)num1 != (int)num3 || __instance.m_bForceReloadAnime))
		//    {
		//        AnmJson
		//        UnityEngine.AnimationClip clip2 = new UnityEngine.AnimationClip(); //ImportCM.LoadAniClipNative(fileSystem, filename, ((int)num3 & 1) != 0, ((int)num3 & 2) != 0, false);
		//        clip2.legacy = true;


		//        // create a curve to move the GameObject and assign to the clip
		//        UnityEngine.Keyframe[] keys;
		//        keys = new UnityEngine.Keyframe[3];
		//        keys[0] = new UnityEngine.Keyframe(0.0f, 0.0f);
		//        keys[1] = new UnityEngine.Keyframe(1.0f, 1.5f);
		//        keys[2] = new UnityEngine.Keyframe(2.0f, 0.0f);
		//        AnimationCurve curve = new UnityEngine.AnimationCurve(keys);
		//        curve.AddKey(0.0f, __instance.transform.position[0]);

		//        clip2.SetCurve("", typeof(Transform), "localPosition.x", curve);

		//        // update the clip to a change the red color
		//        curve = AnimationCurve.Linear(0.0f, 1.0f, 2.0f, 0.0f);
		//        clip2.SetCurve("", typeof(Material), "_Color.r", curve);


		//        clip2.events;
		//        clip2.frameRate;
		//        clip2.hideFlags;

		//        clip2.localBounds;
		//        clip2.name = ;
		//        clip2.wrapMode = UnityEngine.WrapMode.Default;

		//        clip2.SetCurve
		//        if ((UnityEngine.Object)clip2 == (UnityEngine.Object)null)
		//        {
		//            __result = (UnityEngine.AnimationState)null;
		//            return false;
		//        }
		//        animation.AddClip(clip2, tag);
		//        if (tag.Contains("_l_"))
		//        {
		//            for (int index = 2; index <= 8; ++index)
		//            {
		//                if (tag.Contains("_l_" + index.ToString() + "_"))
		//                {
		//                    animation[tag].layer = index;
		//                    break;
		//                }
		//            }
		//        }
		//        __instance.m_AnimCache[tag] = num3;
		//    }
		//    __instance.LastAnimeFN = filename;
		//    UnityEngine.AnimationState animationState = animation[tag];
		//    animationState.blendMode = !additive ? UnityEngine.AnimationBlendMode.Blend : UnityEngine.AnimationBlendMode.Additive;
		//    animationState.wrapMode = !loop ? UnityEngine.WrapMode.Once : UnityEngine.WrapMode.Loop;
		//    animationState.speed = 1f;
		//    animationState.time = 0.0f;
		//    animationState.weight = 0.0f;
		//    animationState.enabled = false;
		//    return animationState;
		//}

		//[HarmonyPatch(typeof(TBody), "CreateAnmClip")]
		//[HarmonyPrefix]
		//public static bool AnmParse_CreateAnmClip(AnmParse __instance, AnimationClip __result)
		//{
		//    if (!DLLAnmParse.IsValid(ref __instance.data_))
		//    {
		//        __result = (AnimationClip)null;
		//        return false;
		//    }

		//    AnimationClip animationClip = new AnimationClip();
		//    if ((UnityEngine.Object)animationClip != (UnityEngine.Object)null)
		//        animationClip.legacy = true;

		//    System.Type type = typeof(Transform);
		//    IntPtr animationCurveDataList = DLLAnmParse.GetAnimationCurveDataList(ref __instance.data_);
		//    int length1 = DLLAnmParse.AnimationCurveDataList_Size(animationCurveDataList);
		//    AnimationCurve[] animationCurveArray = new AnimationCurve[length1];
		//    DLLAnmParse.AnmKeyFrame dest = new DLLAnmParse.AnmKeyFrame();
		//    for (int pos1 = 0; pos1 < length1; ++pos1)
		//    {
		//        IntPtr curve_data = DLLAnmParse.AnimationCurveDataList_At(animationCurveDataList, pos1);
		//        int num = DLLAnmParse.AnimationCurveData_ComID(curve_data);
		//        IntPtr key_framelist = DLLAnmParse.AnimationCurveData_KeyFrameList(curve_data);
		//        int length2 = DLLAnmParse.KeyFrameList_Size(key_framelist);
		//        Keyframe[] keyframeArray = new Keyframe[length2];
		//        for (int pos2 = 0; pos2 < length2; ++pos2)
		//        {
		//            DLLAnmParse.KeyFrameList_At(key_framelist, pos2, ref dest);
		//            keyframeArray[pos2].time = dest.time;
		//            keyframeArray[pos2].value = dest.value;
		//            keyframeArray[pos2].inTangent = dest.in_tangent;
		//            keyframeArray[pos2].outTangent = dest.out_tangent;
		//        }
		//        animationCurveArray[pos1] = new AnimationCurve(keyframeArray);

		//        UnityEngine.Debug.Log("Channel: " + num + " ");
		//        animationClip.SetCurve(Marshal.PtrToStringAnsi(DLLAnmParse.AnimationCurveData_GetPath(curve_data)), AnmParse.proptypes[num - 100], AnmParse.properties[num - 100], animationCurveArray[pos1]);
		//    }
		//    DLLAnmParse.Clear(ref __instance.data_);
		//    return animationClip;
		//}
	}
}