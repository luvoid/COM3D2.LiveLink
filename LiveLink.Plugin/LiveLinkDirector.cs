using CM3D2.UGUI.Panels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UniverseLib.UI;

namespace COM3D2.LiveLink.Plugin
{

	internal class LiveLinkDirector : InternalSingleton<LiveLinkDirector>
	{
		[SerializeField] private DirectorPanel panel;

		[SerializeField] internal Maid TargetMaid = null;
		[SerializeField] internal SafeSlotID TargetSlot;
		[SerializeField] internal int TargetLayer = 0;
		[SerializeField] internal TBody ActiveBody = null;
		[SerializeField] internal TBodySkin ActiveBodySkin = null;

		internal SafeMPN TargetMPN => slotMPNMap[TargetSlot];

		private static readonly Dictionary<SafeSlotID, SafeMPN> slotMPNMap = new()
		{
			{ SafeSlotID.body         , SafeMPN.body         },
			{ SafeSlotID.head         , SafeMPN.head         },
			{ SafeSlotID.eye          , SafeMPN.eye          },
			{ SafeSlotID.hairF        , SafeMPN.hairf        },
			{ SafeSlotID.hairR        , SafeMPN.hairr        },
			{ SafeSlotID.hairS        , SafeMPN.hairs        },
			{ SafeSlotID.hairT        , SafeMPN.hairt        },
			{ SafeSlotID.wear         , SafeMPN.wear         },
			{ SafeSlotID.skirt        , SafeMPN.skirt        },
			{ SafeSlotID.onepiece     , SafeMPN.onepiece     },
			{ SafeSlotID.mizugi       , SafeMPN.mizugi       },
			{ SafeSlotID.panz         , SafeMPN.panz         },
			{ SafeSlotID.bra          , SafeMPN.bra          },
			{ SafeSlotID.stkg         , SafeMPN.stkg         },
			{ SafeSlotID.shoes        , SafeMPN.shoes        },
			{ SafeSlotID.headset      , SafeMPN.headset      },
			{ SafeSlotID.glove        , SafeMPN.glove        },
			{ SafeSlotID.accHead      , SafeMPN.acchead      },
			{ SafeSlotID.hairAho      , SafeMPN.hairaho      },
			{ SafeSlotID.accHana      , SafeMPN.acchana      },
			{ SafeSlotID.accHa        , SafeMPN.accha        },
			{ SafeSlotID.accKami_1_   , SafeMPN.acckami      },
			{ SafeSlotID.accMiMiR     , SafeMPN.accmimi      },
			{ SafeSlotID.accKamiSubR  , SafeMPN.acckamisub   },
			{ SafeSlotID.accNipR      , SafeMPN.accnip       },
			{ SafeSlotID.HandItemR    , SafeMPN.handitem     },
			{ SafeSlotID.accKubi      , SafeMPN.acckubi      },
			{ SafeSlotID.accKubiwa    , SafeMPN.acckubiwa    },
			{ SafeSlotID.accHeso      , SafeMPN.accheso      },
			{ SafeSlotID.accUde       , SafeMPN.accude       },
			{ SafeSlotID.accAshi      , SafeMPN.accashi      },
			{ SafeSlotID.accSenaka    , SafeMPN.accsenaka    },
			{ SafeSlotID.accShippo    , SafeMPN.accshippo    },
			{ SafeSlotID.accAnl       , SafeMPN.accanl       },
			{ SafeSlotID.accVag       , SafeMPN.accvag       },
			{ SafeSlotID.kubiwa       , SafeMPN.null_mpn     },
			{ SafeSlotID.megane       , SafeMPN.megane       },
			{ SafeSlotID.accXXX       , SafeMPN.accxxx       },
			{ SafeSlotID.chinko       , SafeMPN.null_mpn     },
			{ SafeSlotID.chikubi      , SafeMPN.chikubi      },
			{ SafeSlotID.accHat       , SafeMPN.acchat       },
			{ SafeSlotID.kousoku_upper, SafeMPN.kousoku_upper},
			{ SafeSlotID.kousoku_lower, SafeMPN.kousoku_lower},
			{ SafeSlotID.seieki_naka  , SafeMPN.seieki_naka  },
			{ SafeSlotID.seieki_hara  , SafeMPN.seieki_hara  },
			{ SafeSlotID.seieki_face  , SafeMPN.seieki_face  },
			{ SafeSlotID.seieki_mune  , SafeMPN.seieki_mune  },
			{ SafeSlotID.seieki_hip   , SafeMPN.seieki_hip   },
			{ SafeSlotID.seieki_ude   , SafeMPN.seieki_ude   },
			{ SafeSlotID.seieki_ashi  , SafeMPN.seieki_ashi  },
			{ SafeSlotID.accNipL      , SafeMPN.accnip       },
			{ SafeSlotID.accMiMiL     , SafeMPN.accmimi      },
			{ SafeSlotID.accKamiSubL  , SafeMPN.acckamisub   },
			{ SafeSlotID.accKami_2_   , SafeMPN.acckami      },
			{ SafeSlotID.accKami_3_   , SafeMPN.acckami      },
			{ SafeSlotID.HandItemL    , SafeMPN.handitem     },
			{ SafeSlotID.underhair    , SafeMPN.underhair    },
			{ SafeSlotID.moza         , SafeMPN.moza         },
			{ SafeSlotID.end          , SafeMPN.null_mpn     },
		};

		internal void InitializePanel(UIBase owner)
		{
			panel = new DirectorPanel(owner, this);
		}

		void Update()
		{
			TargetFirstMaid();
		}

		void TargetFirstMaid()
		{
			if (TargetMaid != null && TargetMaid.isActiveAndEnabled) return;
			TargetMaid = GameObject.FindObjectOfType<Maid>();
		}

		internal void SetAnimation(byte[] anmData)
		{
			TargetMaid.body0.GetAnimation().Stop();
			TargetMaid.body0.CrossFade("LiveLinkAnimator", anmData, additive: false, loop: true, fade: 0.1f);
		}


		internal void LoadModel(Stream stream)
		{
			TBodySkin bodySkin = TargetMaid.body0.GetSlot(TargetSlot.ToString());

			//ImportCM.LoadSkinMesh_R(filename, bodySkin.morph, Instance.TargetSlot.ToString(), bodySkin, 0);

			TBody_AddItem(TargetMaid.body0, TargetMPN, TargetSlot.ToString(), stream);
		}

		private static int itemCount = 0;
		private static void TBody_AddItem(TBody body, SafeMPN safempn, string slotname, Stream filestream)//, string AttachSlot, string AttachName, bool f_bTemp, int version)
		{
			int slotNum = (int)TBody.hashSlotName[slotname];
			string bonename = body.m_strSlotName[slotNum * TBody.strSlotNameItemCnt + 1];
			//if (AttachSlot == "ボーンにアタッチ" && !string.IsNullOrEmpty(AttachName))
			//{
			//	bonename = AttachName;
			//}

			TBodySkin bodySkin = body.SafeGoSlot()[slotNum];
			TBodySkin_Load(bodySkin, safempn, body.m_trBones2, body.m_trBones, body.m_dicTrans, bonename, filestream, slotname, null, 10, false, 100);
			//tBodySkin.SyojiType = 0;
			bodySkin.m_strModelFileName = $"LiveLinkItem{itemCount++}";
			bodySkin.RID = bodySkin.m_strModelFileName.GetHashCode();
			//tBodySkin.AttachName = null;
			//tBodySkin.AttachSlotIdx = 0;
			//if (AttachSlot == "ボーンにアタッチ")
			//{
			//	tBodySkin.trsBoneAttach = CMT.SearchObjName(body.m_trBones, AttachName);
			//}
			//else if (AttachSlot != string.Empty)
			//{
			//	if (!TBody.hashSlotName.ContainsKey(AttachSlot))
			//	{
			//		return;
			//	}
			//
			//	int attachSlotIdx = (int)TBody.hashSlotName[AttachSlot];
			//	tBodySkin.AttachName = AttachName;
			//	tBodySkin.AttachSlotIdx = attachSlotIdx;
			//}

			if (slotname == "head")
			{
				body.Face = bodySkin;
				if (!body.boMAN)
				{
					body.trsEyeL = CMT.SearchObjName(body.Face.obj_tr, "Eye_L", boSMPass: false);
					body.trsEyeR = CMT.SearchObjName(body.Face.obj_tr, "Eye_R", boSMPass: false);
					body.quaDefEyeL = body.trsEyeL.localRotation;
					body.quaDefEyeR = body.trsEyeR.localRotation;
					//body.EyeEulerAngle = Vector3.zero;
				}
			}

			if (slotname == "body" && !body.boMAN)
			{
				body.quaUppertwist_L = CMT.SearchObjName(bodySkin.obj_tr, "Uppertwist_L", boSMPass: false).localRotation;
				body.quaUpperArmL    = CMT.SearchObjName(bodySkin.obj_tr, "Bip01 L UpperArm", boSMPass: false).localRotation;
				body.quaUppertwist_R = CMT.SearchObjName(bodySkin.obj_tr, "Uppertwist_R", boSMPass: false).localRotation;
				body.quaUpperArmR    = CMT.SearchObjName(bodySkin.obj_tr, "Bip01 R UpperArm", boSMPass: false).localRotation;
			}

			body.bonemorph.Init();
			body.bonemorph.InitBoneMorphEdit(bodySkin.obj_tr, safempn.ToMPN(), (TBody.SlotID)slotNum, bodySkin);
			body.bonemorph.AddRoot(body.m_trBones);
			body.bonemorph.Blend();
			//if (body.boMAN)
			//{
			//	body.SetManHide(body.m_bManMeshHide);
			//}
		}

		private static void TBodySkin_Load(TBodySkin bodySkin, SafeMPN safempn, Transform srcbody, Transform body1, Dictionary<string, Transform> trans, string bonename, Stream stream, string slotname, string AttachSlot, int layer, bool f_bTemp, int version)
		{
			bodySkin.SafeDeleteObj();
			bodySkin.m_partsVersion = version;
			if (safempn == SafeMPN.accashi || safempn == SafeMPN.shoes)
			{
				bodySkin.m_bHitFloorY = false;
			}

			bodySkin.m_ParentMPN = safempn.ToMPN();
			if (bodySkin.m_ParentMPN != 0)
			{
				bodySkin.m_mp = bodySkin.body.maid.GetProp(bodySkin.m_ParentMPN);
			}

			bodySkin.m_bTemp = f_bTemp;
			bodySkin.boVisible = true;

			Vector3    srcbodyPosition   = srcbody.position;
			Quaternion srcebodyRotation  = srcbody.rotation;
			Vector3    srcbodyLocalScale = srcbody.localScale;
			Vector3    body1Position     = body1.position;
			Quaternion body1Rotation     = body1.rotation;
			Vector3    body1LocalScale   = body1.localScale;

			srcbody.position = Vector3.zero;
			srcbody.rotation = Quaternion.identity;
			srcbody.localScale = new Vector3(1f / srcbody.lossyScale.x, 1f / srcbody.lossyScale.y, 1f / srcbody.lossyScale.z);

			body1.position = Vector3.zero;
			body1.rotation = Quaternion.identity;
			body1.localScale = new Vector3(1f / body1.lossyScale.x, 1f / body1.lossyScale.y, 1f / body1.lossyScale.z);


			bodySkin.morph = new TMorph(bodySkin);
			GameObject meshObject = ImportCMExtensions.LoadSkinMesh_R(stream, bodySkin.morph, slotname, bodySkin, layer);
			ShapekeyMasterUtils.TryUpdateMorphDicImmediate(bodySkin.morph);

			if (bodySkin.m_bMan)
			{
				Transform[] componentsInChildren = meshObject.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform in componentsInChildren)
				{
					Renderer component = transform.GetComponent<Renderer>();
					if (component == null || component.materials == null)
					{
						continue;
					}

					Material[] materials = component.materials;
					foreach (Material material in materials)
					{
						if (material.shader.name == "CM3D2/Man")
						{
							bodySkin.m_listManAlphaMat.Add(material);
						}

						if (material.shader.name == "CM3D2/Mosaic")
						{
							material.SetFloat("_FloatValue1", 15f);
						}
					}
				}
			}

			if (bodySkin.m_bMan && Product.isEnglish && !Product.isPublic)
			{
				Transform[] componentsInChildren2 = meshObject.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform2 in componentsInChildren2)
				{
					Renderer component2 = transform2.GetComponent<Renderer>();
					if (component2 == null || component2.materials == null)
					{
						continue;
					}

					Material[] materials2 = component2.materials;
					foreach (Material material2 in materials2)
					{
						if (material2.shader.name == "CM3D2/Mosaic")
						{
							material2.shader = Shader.Find("CM3D2/Mosaic_en");
						}
					}
				}
			}

			bodySkin.morph.InitGameObject(meshObject);
			if (Product.isEnglish && !Product.isPublic)
			{
				switch (safempn)
				{
					case SafeMPN.body:
						bodySkin.kupaCtrl = new KupaCtrl(bodySkin.body, bodySkin.morph);
						break;
					case SafeMPN.moza:
						bodySkin.body.GetSlot(0).kupaCtrl.AddMozaMorph(bodySkin.morph);
						break;
				}
			}

			meshObject.transform.parent = CMT.SearchObjName(srcbody, bonename);
			Vector3    meshLocalPosition = meshObject.transform.localPosition;
			Vector3    meshLocalScale    = meshObject.transform.localScale;
			Quaternion meshLocalRotation = meshObject.transform.localRotation;
			meshObject.transform.parent = CMT.SearchObjName(body1, bonename);
			//if (!string.IsNullOrEmpty(AttachSlot))
			//{
			//	bodySkin.AttachVisible = true;
			//}

			if (AttachSlot == "ボーンにアタッチ" && (bonename == "_IK_handR" || bonename == "_IK_handL"))
			{
				meshLocalPosition = Vector3.zero;
				meshLocalRotation = Quaternion.identity;
				meshLocalScale    = Vector3.one;
			}

			meshObject.transform.localPosition = meshLocalPosition;
			meshObject.transform.localRotation = meshLocalRotation;
			meshObject.transform.localScale    = meshLocalScale;
			bodySkin.obj = meshObject;
			bodySkin.obj_tr = bodySkin.obj.transform;
			bodySkin.listTrs = new List<Transform>(200);
			bodySkin.listTrsScr = new List<Transform>(4);
			CMT.BindTrans(bodySkin.listTrs, bodySkin.listTrsScr, trans, meshObject.transform);

			srcbody.position   = srcbodyPosition;
			srcbody.rotation   = srcebodyRotation;
			srcbody.localScale = srcbodyLocalScale;
			body1.position     = body1Position;
			body1.rotation     = body1Rotation;
			body1.localScale   = body1LocalScale;

			if (bodySkin.body.m_bNewPhyscs)
			{
				if (!bodySkin.bonehair2.InitGameObject(meshObject, safempn.ToMPN()))
				{
					bool bNoSkirt = bodySkin.bonehair3.InitGameObject(meshObject, safempn.ToMPN());
					bodySkin.bonehair.SearchGameObj(meshObject, bNoSkirt);
				}
			}
			else
			{
				bodySkin.bonehair.SearchGameObj(meshObject);
			}

			bodySkin.ItemScaleReset();
			bodySkin.OnChangeScreenSizeOrAA();
			if (bodySkin.m_bMan)
			{
				bodySkin.ManColorUpdate();
			}

			foreach (KeyValuePair<string, float> item in bodySkin.m_BonehairBodyhitScaleBackup)
			{
				bodySkin.bonehair.bodyhit.ScaleMune(item.Key, item.Value);
			}

			foreach (string key in bodySkin.morph.hash.Keys)
			{
				int f_nIdx = (int)bodySkin.morph.hash[key];
				if (bodySkin.body.m_MorphBlendValues.TryGetValue(key, out var value))
				{
					bodySkin.morph.SetBlendValues(f_nIdx, value);
				}
			}

			bodySkin.morph.FixBlendValues();

			bodySkin.m_vDefPosLocal = (bodySkin.m_vPosLocal = bodySkin.obj_tr.localPosition);
			bodySkin.m_qDefRotLocal = (bodySkin.m_qRotLocal = bodySkin.obj_tr.localRotation);
			bodySkin.m_vDefScaleLocal = (bodySkin.m_vScaleRate = bodySkin.obj_tr.localScale);
			SafeSlotID safeSlotID = bodySkin.SlotId.ToSafeSlotID();
			if (!f_bTemp
				&& (   safeSlotID == SafeSlotID.accHat
					|| safeSlotID == SafeSlotID.headset
					|| safeSlotID == SafeSlotID.hairT
					|| safeSlotID == SafeSlotID.accSenaka
					|| safeSlotID == SafeSlotID.accKubi
					|| safeSlotID == SafeSlotID.accKubiwa
					|| safeSlotID == SafeSlotID.accShippo
					|| safeSlotID == SafeSlotID.accKubiwa))
			{
				SkinnedMeshRenderer componentInChildren = bodySkin.obj_tr.GetComponentInChildren<SkinnedMeshRenderer>(includeInactive: true);
				if (componentInChildren != null)
				{
					Bounds bounds = componentInChildren.bounds;
					GameObject gameObject2 = new GameObject("center");
					gameObject2.transform.SetParent(bodySkin.obj_tr, worldPositionStays: false);
					gameObject2.transform.position = bounds.center;
					GameObject gameObject3 = new GameObject("center2");
					gameObject3.transform.SetParent(gameObject2.transform, worldPositionStays: false);
					gameObject3.transform.position = bounds.center;
					bodySkin.m_vCenterPosLocal = gameObject2.transform.localPosition;
					bodySkin.center_tr = gameObject2.transform;
					bodySkin.center_tr2 = gameObject3.transform;
					BoneAttachPos tBodySkinPos = bodySkin.body.maid.GetTBodySkinPos(bodySkin.m_ParentMPN, bodySkin.SlotId);
					if (tBodySkinPos != null)
					{
						bodySkin.m_vPosLocal = tBodySkinPos.pss.position;
						bodySkin.m_qRotLocal = tBodySkinPos.pss.rotation;
						bodySkin.m_vScaleRate = tBodySkinPos.pss.scale;
						bodySkin.m_bEnablePartPosEdit = tBodySkinPos.bEnable;
						if (bodySkin.m_bEnablePartPosEdit)
						{
							bodySkin.obj_tr.localPosition = tBodySkinPos.pss.position;
							bodySkin.obj_tr.localRotation = tBodySkinPos.pss.rotation;
							bodySkin.obj_tr.localScale = tBodySkinPos.pss.scale;
						}
					}
					else
					{
						bodySkin.body.maid.ClearTBodySkinPos(bodySkin.m_ParentMPN, bodySkin.SlotId);
					}
				}
			}
			bodySkin.m_HairLengthCtrl.NotExistThenClearHairLength();
			if (OvrIK.IsModeVRIK && OvrIK.Instance != null && OvrIK.Instance.NowMaid == bodySkin.body.maid 
				&& (safeSlotID == SafeSlotID.head || safeSlotID == SafeSlotID.megane || safeSlotID == SafeSlotID.accHead))
			{
				bodySkin.LayerCheck(bodySkin.obj);
			}
		}

		/*
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
		*/

	}

	internal class DirectorPanel : CM3D2Panel
	{
		public readonly LiveLinkDirector Director;

		public DirectorPanel(UIBase owner, LiveLinkDirector director) : base(owner)
		{
			this.Director = director;
		}

		public override string Name { get { return "LiveLink: Director"; } }

		public override Vector2 DefaultAnchorMin => new Vector2(1, 0.5f);

		public override Vector2 DefaultAnchorMax => DefaultAnchorMin;

		public override Vector2 DefaultPosition => new Vector2(-PreferredSize.x - 10, PreferredSize.y / 2);
		public override Vector2 PreferredSize => base.PreferredSize + new Vector2(80, 0);

		protected override void OnClosePanelClicked()
		{
			LiveLinkPlugin.Instance.PluginUIBase.Enabled = false;
		}

		protected override void ConstructPanelContent()
		{
			using (Create.LayoutContext(flexibleWidth: 1))
			{
				Create.StringProperty(ContentRoot, nameof(Director.TargetMaid),
					() => Director.TargetMaid?.name ?? "");

				Create.EnumProperty(ContentRoot, nameof(Director.TargetSlot),
					() => Director.TargetSlot,
					(x) => Director.TargetSlot = x);

				Create.EnumProperty(ContentRoot, nameof(Director.TargetMPN),
					() => Director.TargetMPN);

				//Create.Label(ContentRoot, "TestFileLabel", "Test File");
				//var loadFileGroup = Create.HorizontalGroup(ContentRoot, "LoadFileGroup", childAlignment: TextAnchor.MiddleLeft);
				//var filenameInputField = Create.InputField(loadFileGroup, "FilenameInputField", "Filename");
				//var loadButton = Create.Button(loadFileGroup, "LoadButton", "Load");
				//UIFactory.SetLayoutElement(loadButton.GameObject, flexibleWidth: 0);
				//loadButton.OnClick += () => Director.LoadModel(filenameInputField.Text);
			}
		}

		private void OnDropdownValueChanged(int value)
		{
			Debug.Log("Dropdown " + value);
		}
	}
}