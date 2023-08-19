using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin
{
	[HarmonyPatch]
	internal class LiveLinkModelViewer : InternalSingleton<LiveLinkModelViewer>
	{
		[field: SerializeField] public static TBody ActiveBody { get; private set; } = null;
		[field: SerializeField] public static TBodySkin ActiveBodySkin { get; private set; } = null;

		internal static void SetModel(Stream modelData)
		{
			ResetActiveBody();

			ImportCMExtensions.LoadSkinMesh_R(modelData, ActiveBodySkin.morph, "", ActiveBodySkin, 0);
		}
		
		private static void ResetActiveBody()
		{
			if (ActiveBody == null)
			{
				CreateBody(out TBody body, out TBodySkin bodySkin);
				ActiveBody = body;
				ActiveBodySkin = bodySkin;
			}
			else
			{
				ClearBody(ActiveBody, ActiveBodySkin);
			}

			InitBody(ActiveBody, ActiveBodySkin);
		}

		private static void CreateBody(out TBody body, out TBodySkin bodySkin)
		{
			GameObject gameObject = new GameObject("LiveLinkModel");
			Object.DontDestroyOnLoad(gameObject);
			body = gameObject.AddComponent<TBody>();
			body.enabled = false;

			bodySkin = DefaultConstructTBodySkin();
			bodySkin.boVisible = true;
			bodySkin.body = body;
			bodySkin.Category = "";
			bodySkin.SlotId = TBody.SlotID.body;
			//bodySkin.m_bMan = false;
			AccessTools.Field(typeof(TBodySkin), "m_bMan").SetValue(bodySkin, false);
			//bodySkin.m_trMaid = body.transform;
			AccessTools.Field(typeof(TBodySkin), "m_trMaid").SetValue(bodySkin, body.transform);
			//bodySkin.m_trMaidOffs = body.transform;
			AccessTools.Field(typeof(TBodySkin), "m_trMaidOffs").SetValue(bodySkin, body.transform);
			bodySkin.bonehair = new TBoneHair_(bodySkin);
			bodySkin.TextureCache = new InfinityColorTextureCache(null);
			bodySkin.bonehair2 = new BoneHair2(bodySkin);
			bodySkin.bonehair3 = new BoneHair3(bodySkin);
			bodySkin.m_HairLengthCtrl = new TBodySkin.HairLengthCtrl(bodySkin);
		}

		private static void InitBody(TBody body, TBodySkin bodySkin)
		{
			//body.Init(null);
			bodySkin.morph = new TMorph(bodySkin);
		}

		private static void ClearBody(TBody body, TBodySkin bodySkin)
		{
			if (bodySkin != null)
			{
				bodySkin.m_ParentMPN = MPN.null_mpn;
				bodySkin.m_mp = null;
				//bodySkin.m_animItem = null;
				AccessTools.DeclaredField(typeof(TBodySkin), "m_animItem").SetValue(bodySkin, null);
				body.MulTexRemove(bodySkin.Category);
				bodySkin.TextureCache.RemoveTexture();
				//bodySkin.m_listManAlphaMat.Clear();
				(AccessTools.DeclaredField(typeof(TBodySkin), "m_listManAlphaMat").GetValue(bodySkin) as IList).Clear();
				//bodySkin.m_HairLengthCtrl.NotExistThenClearHairLength();
				//bodySkin.m_HairLengthCtrl.m_dicHairLenght.Clear();
				(AccessTools.DeclaredField(typeof(TBodySkin.HairLengthCtrl), "m_dicHairLenght").GetValue(bodySkin.m_HairLengthCtrl) as IDictionary).Clear();
				//bodySkin.listTrs = null;
				AccessTools.DeclaredField(typeof(TBodySkin), "listTrs").SetValue(bodySkin, null);
				//bodySkin.listTrsScr = null;
				AccessTools.DeclaredField(typeof(TBodySkin), "listTrsScr").SetValue(bodySkin, null);
				bodySkin.trsBoneAttach = null;
				foreach (Object item in bodySkin.listDEL)
				{
					Object.DestroyImmediate(item);
				}

				bodySkin.listDEL = new List<Object>();
				bodySkin.boMizugi = false;
				if (bodySkin.morph != null)
				{
					bodySkin.morph.DeleteObj();
					bodySkin.morph = null;
				}

				bodySkin.boVisible = false;
				bodySkin.listMaskSlot = new List<int>();
				//bodySkin.m_dicBackupShader.Clear();
				(AccessTools.DeclaredField(typeof(TBodySkin), "m_dicBackupShader").GetValue(bodySkin) as IDictionary).Clear();
				if (bodySkin.bonehair != null)
				{
					bodySkin.bonehair.Init();
				}

				if (bodySkin.bonehair2 != null)
				{
					bodySkin.bonehair2.Uninit();
				}

				if (bodySkin.bonehair3 != null)
				{
					bodySkin.bonehair3.Uninit();
				}

				if (bodySkin.obj != null)
				{
					bodySkin.obj_tr.parent = null;
					Object.DestroyImmediate(bodySkin.obj);
					bodySkin.obj = null;
					bodySkin.obj_tr = null;
				}

				bodySkin.m_dicDelNodeParts.Clear();
				//bodySkin.m_dicParam.Clear();
				(AccessTools.DeclaredField(typeof(TBodySkin), "m_dicParam").GetValue(bodySkin) as IDictionary).Clear();
				//bodySkin.m_dicParam2.Clear();
				(AccessTools.DeclaredField(typeof(TBodySkin), "m_dicParam2").GetValue(bodySkin) as IDictionary).Clear();
				bodySkin.m_OriVert.Clear();
				bodySkin.m_ParentMPN = MPN.null_mpn;
				bodySkin.m_mp = null;
				bodySkin.m_bHitFloorY = true;
				//bodySkin.m_partsVersion = 0;
				AccessTools.DeclaredField(typeof(TBodySkin), "m_partsVersion").SetValue(bodySkin, 0);
				//bodySkin.kupaCtrl = null;
				AccessTools.DeclaredPropertySetter(typeof(TBodySkin), "kupaCtrl").Invoke(bodySkin, new object[] { null });
				body.ToeLockCtrl?.Remove(bodySkin.SlotId);
			}
		}

		private static TBodySkin DefaultConstructTBodySkin()
		{
			TBodySkin bodySkin = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TBodySkin)) as TBodySkin;
			bodySkin.listMaskSlot = new List<int>();
			bodySkin.listDEL = new List<Object>();
			bodySkin.dicDel = new Dictionary<string, Object>();
			//bodySkin.m_listRtMaterial = new List<Material>();
			AccessTools.Field(typeof(TBodySkin), "m_listRtMaterial").SetValue(bodySkin, new List<Material>());

			bodySkin.m_dicDelNodeBody = new Dictionary<string, bool>();
			bodySkin.m_dicDelNodeParts = new Dictionary<string, Dictionary<string, bool>>();
			bodySkin.dicParamSet = new Dictionary<string, KeyValuePair<float, string>[]>();
			//bodySkin.m_dicParam = new Dictionary<string, string>();
			AccessTools.Field(typeof(TBodySkin), "m_dicParam").SetValue(bodySkin, new Dictionary<string, string>());
			//bodySkin.m_dicParam2 = new Dictionary<string, string>();
			AccessTools.Field(typeof(TBodySkin), "m_dicParam2").SetValue(bodySkin, new Dictionary<string, string>());
			bodySkin.m_strModelFileName = string.Empty;
			//bodySkin.m_listManAlphaMat = new List<Material>();
			AccessTools.Field(typeof(TBodySkin), "m_listManAlphaMat").SetValue(bodySkin, new List<Material>());
			bodySkin.m_BonehairBodyhitScaleBackup = new Dictionary<string, float>();
			bodySkin.m_dicTempAttachPoint = new Dictionary<string, TMorph.TempAttachPos>();
			bodySkin.m_bHitFloorY = true;
			//bodySkin.m_dicBackupShader = new Dictionary<int, Shader>();
			AccessTools.Field(typeof(TBodySkin), "m_dicBackupShader").SetValue(bodySkin, new Dictionary<int, Shader>());
			bodySkin.m_OriVert = new TBodySkin.OriVert();
			//bodySkin.m_listRnederTemp = new List<Renderer>(4);
			AccessTools.Field(typeof(TBodySkin), "m_listRnederTemp").SetValue(bodySkin, new List<Renderer>(4));
			return bodySkin;
		}
	}
}
