using CM3D2.UGUI.Panels;
using COM3D2.LiveLink.Plugin;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UniverseLib.UI;

namespace COM3D2.LiveLink.Plugin
{
	internal enum TargetSlotID
	{
		body,
		head,
		eye,
		hairF,
		hairR,
		hairS,
		hairT,
		wear,
		skirt,
		onepiece,
		mizugi,
		panz,
		bra,
		stkg,
		shoes,
		headset,
		glove,
		accHead,
		hairAho,
		accHana,
		accHa,
		accKami_1_,
		accMiMiR,
		accKamiSubR,
		accNipR,
		HandItemR,
		accKubi,
		accKubiwa,
		accHeso,
		accUde,
		accAshi,
		accSenaka,
		accShippo,
		accAnl,
		accVag,
		kubiwa,
		megane,
		accXXX,
		chinko,
		chikubi,
		accHat,
		kousoku_upper,
		kousoku_lower,
		seieki_naka,
		seieki_hara,
		seieki_face,
		seieki_mune,
		seieki_hip,
		seieki_ude,
		seieki_ashi,
		accNipL,
		accMiMiL,
		accKamiSubL,
		accKami_2_,
		accKami_3_,
		HandItemL,
		underhair,
		moza,
		end
	}

	internal class LiveLinkDirector : InternalSingleton<LiveLinkDirector>
	{
		[SerializeField] private DirectorPanel panel;

		[SerializeField] internal Maid TargetMaid = null;
		[SerializeField] internal TargetSlotID TargetSlot;
		[SerializeField] internal int TargetLayer = 0;
		[SerializeField] internal TBody ActiveBody = null;
		[SerializeField] internal TBodySkin ActiveBodySkin = null;

		internal MPN TargetMPN => slotMPNMap[TargetSlot];

		private static readonly Dictionary<TargetSlotID, MPN> slotMPNMap = new Dictionary<TargetSlotID, MPN>()
		{
			{ TargetSlotID.body         , MPN.body         },
			{ TargetSlotID.head         , MPN.head         },
			{ TargetSlotID.eye          , MPN.eye          },
			{ TargetSlotID.hairF        , MPN.hairf        },
			{ TargetSlotID.hairR        , MPN.hairr        },
			{ TargetSlotID.hairS        , MPN.hairs        },
			{ TargetSlotID.hairT        , MPN.hairt        },
			{ TargetSlotID.wear         , MPN.wear         },
			{ TargetSlotID.skirt        , MPN.skirt        },
			{ TargetSlotID.onepiece     , MPN.onepiece     },
			{ TargetSlotID.mizugi       , MPN.mizugi       },
			{ TargetSlotID.panz         , MPN.panz         },
			{ TargetSlotID.bra          , MPN.bra          },
			{ TargetSlotID.stkg         , MPN.stkg         },
			{ TargetSlotID.shoes        , MPN.shoes        },
			{ TargetSlotID.headset      , MPN.headset      },
			{ TargetSlotID.glove        , MPN.glove        },
			{ TargetSlotID.accHead      , MPN.acchead      },
			{ TargetSlotID.hairAho      , MPN.hairaho      },
			{ TargetSlotID.accHana      , MPN.acchana      },
			{ TargetSlotID.accHa        , MPN.accha        },
			{ TargetSlotID.accKami_1_   , MPN.acckami      },
			{ TargetSlotID.accMiMiR     , MPN.accmimi      },
			{ TargetSlotID.accKamiSubR  , MPN.acckamisub   },
			{ TargetSlotID.accNipR      , MPN.accnip       },
			{ TargetSlotID.HandItemR    , MPN.handitem     },
			{ TargetSlotID.accKubi      , MPN.acckubi      },
			{ TargetSlotID.accKubiwa    , MPN.acckubiwa    },
			{ TargetSlotID.accHeso      , MPN.accheso      },
			{ TargetSlotID.accUde       , MPN.accude       },
			{ TargetSlotID.accAshi      , MPN.accashi      },
			{ TargetSlotID.accSenaka    , MPN.accsenaka    },
			{ TargetSlotID.accShippo    , MPN.accshippo    },
			{ TargetSlotID.accAnl       , MPN.accanl       },
			{ TargetSlotID.accVag       , MPN.accvag       },
			{ TargetSlotID.kubiwa       , MPN.null_mpn     },
			{ TargetSlotID.megane       , MPN.megane       },
			{ TargetSlotID.accXXX       , MPN.accxxx       },
			{ TargetSlotID.chinko       , MPN.null_mpn     },
			{ TargetSlotID.chikubi      , MPN.chikubi      },
			{ TargetSlotID.accHat       , MPN.acchat       },
			{ TargetSlotID.kousoku_upper, MPN.kousoku_upper},
			{ TargetSlotID.kousoku_lower, MPN.kousoku_lower},
			{ TargetSlotID.seieki_naka  , MPN.seieki_naka  },
			{ TargetSlotID.seieki_hara  , MPN.seieki_hara  },
			{ TargetSlotID.seieki_face  , MPN.seieki_face  },
			{ TargetSlotID.seieki_mune  , MPN.seieki_mune  },
			{ TargetSlotID.seieki_hip   , MPN.seieki_hip   },
			{ TargetSlotID.seieki_ude   , MPN.seieki_ude   },
			{ TargetSlotID.seieki_ashi  , MPN.seieki_ashi  },
			{ TargetSlotID.accNipL      , MPN.accnip       },
			{ TargetSlotID.accMiMiL     , MPN.accmimi      },
			{ TargetSlotID.accKamiSubL  , MPN.acckamisub   },
			{ TargetSlotID.accKami_2_   , MPN.acckami      },
			{ TargetSlotID.accKami_3_   , MPN.acckami      },
			{ TargetSlotID.HandItemL    , MPN.handitem     },
			{ TargetSlotID.underhair    , MPN.underhair    },
			{ TargetSlotID.moza         , MPN.moza         },
			{ TargetSlotID.end          , MPN.null_mpn     },
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
			TargetMaid.body0.CrossFade("LiveLinkAnimator", anmData, loop: true, fade: 0.1f);
		}


		internal void LoadModel(Stream stream)
		{
			TBodySkin bodySkin = TargetMaid.body0.GetSlot(TargetSlot.ToString());

			//ImportCM.LoadSkinMesh_R(filename, bodySkin.morph, Instance.TargetSlot.ToString(), bodySkin, 0);

			TBody_AddItem(TargetMaid.body0, TargetMPN, TargetSlot.ToString(), stream);
		}

		private static int itemCount = 0;
		private static void TBody_AddItem(TBody body, MPN mpn, string slotname, Stream filestream)//, string AttachSlot, string AttachName, bool f_bTemp, int version)
		{
			int num = (int)TBody.hashSlotName[slotname];
			string bonename = body.m_strSlotName[num * TBody.strSlotNameItemCnt + 1];
			//if (AttachSlot == "ボーンにアタッチ" && !string.IsNullOrEmpty(AttachName))
			//{
			//	bonename = AttachName;
			//}

			TBodySkin bodySkin = body.goSlot[num];
			TBodySkin_Load(bodySkin, mpn, body.m_trBones2, body.m_trBones, body.m_dicTrans, bonename, filestream, slotname, null, 10, false, 100);
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
				body.quaUpperArmL = CMT.SearchObjName(bodySkin.obj_tr, "Bip01 L UpperArm", boSMPass: false).localRotation;
				body.quaUppertwist_R = CMT.SearchObjName(bodySkin.obj_tr, "Uppertwist_R", boSMPass: false).localRotation;
				body.quaUpperArmR = CMT.SearchObjName(bodySkin.obj_tr, "Bip01 R UpperArm", boSMPass: false).localRotation;
			}

			body.bonemorph.Init();
			body.bonemorph.InitBoneMorphEdit(bodySkin.obj_tr, mpn, (TBody.SlotID)num, bodySkin);
			body.bonemorph.AddRoot(body.m_trBones);
			body.bonemorph.Blend();
			//if (body.boMAN)
			//{
			//	body.SetManHide(body.m_bManMeshHide);
			//}
		}

		private static void TBodySkin_Load(TBodySkin bodySkin, MPN mpn, Transform srcbody, Transform body1, Dictionary<string, Transform> trans, string bonename, Stream stream, string slotname, string AttachSlot, int layer, bool f_bTemp, int version)
		{
			bodySkin.DeleteObj();
			bodySkin.m_partsVersion = version;
			if (mpn == MPN.accashi || mpn == MPN.shoes)
			{
				bodySkin.m_bHitFloorY = false;
			}

			bodySkin.m_ParentMPN = mpn;
			if (bodySkin.m_ParentMPN != 0)
			{
				bodySkin.m_mp = bodySkin.body.maid.GetProp(bodySkin.m_ParentMPN);
			}

			bodySkin.m_bTemp = f_bTemp;
			bodySkin.boVisible = true;

			Vector3 position = srcbody.position;
			Quaternion rotation = srcbody.rotation;
			Vector3 localScale = srcbody.localScale;
			Vector3 position2 = body1.position;
			Quaternion rotation2 = body1.rotation;
			Vector3 localScale2 = body1.localScale;

			srcbody.position = Vector3.zero;
			srcbody.rotation = Quaternion.identity;
			srcbody.localScale = new Vector3(1f / srcbody.lossyScale.x, 1f / srcbody.lossyScale.y, 1f / srcbody.lossyScale.z);
			body1.position = Vector3.zero;
			body1.rotation = Quaternion.identity;
			body1.localScale = new Vector3(1f / body1.lossyScale.x, 1f / body1.lossyScale.y, 1f / body1.lossyScale.z);


			bodySkin.morph = new TMorph(bodySkin);
			GameObject gameObject = ImportCMExtensions.LoadSkinMesh_R(stream, bodySkin.morph, slotname, bodySkin, layer);
			if (bodySkin.m_bMan)
			{
				Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
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
				Transform[] componentsInChildren2 = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
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

			bodySkin.morph.InitGameObject(gameObject);
			if (Product.isEnglish && !Product.isPublic)
			{
				switch (mpn)
				{
					case MPN.body:
						bodySkin.kupaCtrl = new KupaCtrl(bodySkin.body, bodySkin.morph);
						break;
					case MPN.moza:
						bodySkin.body.GetSlot(0).kupaCtrl.AddMozaMorph(bodySkin.morph);
						break;
				}
			}

			gameObject.transform.parent = CMT.SearchObjName(srcbody, bonename);
			Vector3 localPosition = gameObject.transform.localPosition;
			Vector3 localScale3 = gameObject.transform.localScale;
			Quaternion localRotation = gameObject.transform.localRotation;
			gameObject.transform.parent = CMT.SearchObjName(body1, bonename);
			//if (!string.IsNullOrEmpty(AttachSlot))
			//{
			//	bodySkin.AttachVisible = true;
			//}

			if (AttachSlot == "ボーンにアタッチ" && (bonename == "_IK_handR" || bonename == "_IK_handL"))
			{
				localPosition = Vector3.zero;
				localRotation = Quaternion.identity;
				localScale3 = Vector3.one;
			}

			gameObject.transform.localPosition = localPosition;
			gameObject.transform.localRotation = localRotation;
			gameObject.transform.localScale = localScale3;
			bodySkin.obj = gameObject;
			bodySkin.obj_tr = bodySkin.obj.transform;
			bodySkin.listTrs = new List<Transform>(200);
			bodySkin.listTrsScr = new List<Transform>(4);
			CMT.BindTrans(bodySkin.listTrs, bodySkin.listTrsScr, trans, gameObject.transform);

			srcbody.position = position;
			srcbody.rotation = rotation;
			srcbody.localScale = localScale;
			body1.position = position2;
			body1.rotation = rotation2;
			body1.localScale = localScale2;

			if (bodySkin.body.m_bNewPhyscs)
			{
				if (!bodySkin.bonehair2.InitGameObject(gameObject, mpn))
				{
					bool bNoSkirt = bodySkin.bonehair3.InitGameObject(gameObject, mpn);
					bodySkin.bonehair.SearchGameObj(gameObject, bNoSkirt);
				}
			}
			else
			{
				bodySkin.bonehair.SearchGameObj(gameObject);
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

			IEnumerator enumerator2 = bodySkin.morph.hash.Keys.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					string key = (string)enumerator2.Current;
					int f_nIdx = (int)bodySkin.morph.hash[key];
					if (bodySkin.body.m_MorphBlendValues.TryGetValue(key, out var value))
					{
						bodySkin.morph.SetBlendValues(f_nIdx, value);
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator2 as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}

			bodySkin.morph.FixBlendValues();
			bodySkin.m_vDefPosLocal = (bodySkin.m_vPosLocal = bodySkin.obj_tr.localPosition);
			bodySkin.m_qDefRotLocal = (bodySkin.m_qRotLocal = bodySkin.obj_tr.localRotation);
			bodySkin.m_vDefScaleLocal = (bodySkin.m_vScaleRate = bodySkin.obj_tr.localScale);
			if (!f_bTemp
				&& (   bodySkin.SlotId == TBody.SlotID.accHat
					|| bodySkin.SlotId == TBody.SlotID.headset
					|| bodySkin.SlotId == TBody.SlotID.hairT
					|| bodySkin.SlotId == TBody.SlotID.accSenaka
					|| bodySkin.SlotId == TBody.SlotID.accKubi
					|| bodySkin.SlotId == TBody.SlotID.accKubiwa
					|| bodySkin.SlotId == TBody.SlotID.accShippo
					|| bodySkin.SlotId == TBody.SlotID.accKubiwa))
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
				&& (bodySkin.SlotId == TBody.SlotID.head || bodySkin.SlotId == TBody.SlotID.megane || bodySkin.SlotId == TBody.SlotID.accHead))
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