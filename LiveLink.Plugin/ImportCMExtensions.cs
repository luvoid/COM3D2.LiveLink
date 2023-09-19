using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;


namespace COM3D2.LiveLink.Plugin
{
	internal static class ImportCMExtensions
	{
		private static readonly MethodInfo smethod_LoadSkinMesh_R = AccessTools.Method(typeof(ImportCM), nameof(ImportCM.LoadSkinMesh_R));
		private static readonly bool isV3 = smethod_LoadSkinMesh_R.GetParameters().Length > 5;

		private delegate GameObject FuncV3(string filename, TMorph morph, string slotname, TBodySkin bodySkin, int layer, ref int modelVersion, Stream stream);
		private delegate GameObject FuncV2(string filename, TMorph morph, string slotname, TBodySkin bodySkin, int layer, Stream stream);

		public static void Patch(Harmony harmony)
		{
			HarmonyMethod harmonyMethodV2 = new(((FuncV2)LoadSkinMesh_R_v2).Method);
			HarmonyMethod harmonyMethodV3 = new(((FuncV3)LoadSkinMesh_R_v3).Method);

			var reversePatcher = harmony.CreateReversePatcher(
				smethod_LoadSkinMesh_R,
				isV3 ? harmonyMethodV3 : harmonyMethodV2
			);

			try
			{
				reversePatcher.Patch(HarmonyReversePatchType.Snapshot);
			}
			catch (NullReferenceException) // Harmony bug causes error if using Snapshot when the method has no patches yet.
			{
				reversePatcher.Patch(HarmonyReversePatchType.Original);
			}
		}

		public static GameObject LoadSkinMesh_R(Stream stream, TMorph morph, string slotname, TBodySkin bodySkin, int layer)
		{
			if (isV3)
			{
				int modelVersion = default;
				return LoadSkinMesh_R_v3("", morph, slotname, bodySkin, layer, ref modelVersion, stream);
			}
			else
			{
				return LoadSkinMesh_R_v2("", morph, slotname, bodySkin, layer, stream);
			}
		}

		private static GameObject LoadSkinMesh_R_v3(string filename, TMorph morph, string slotname, TBodySkin bodySkin, int layer, ref int modelVersion, Stream stream)
		{
			IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				if (instructions == null) return null;
				if (instructions.Count() == 0) throw new InvalidOperationException("instructions.Count() == 0");

				CodeMatcher transpileHead = new(instructions);

				// In 3.x a flag is set based on the filename, so check for that first.
				CodeMatch[] match_IsCRCFlag =
				{
					new(OpCodes.Ldc_I4_1),
					new((inst) => inst.opcode.ToString().StartsWith("stloc"))
				};
				transpileHead.MatchForward(useEnd: false, match_IsCRCFlag);
				transpileHead.LogOrThrowMatch(smethod_LoadSkinMesh_R, nameof(match_IsCRCFlag), LiveLinkPlugin.Logger);
				transpileHead.Advance(1);
				CodeInstruction crcFlagStoreInst = transpileHead.Instruction;

				LoadSkinMesh_R_v2_Transpile(transpileHead, new CodeInstruction(OpCodes.Ldarg_S, (byte)6));

				// bool IsCRCFlag = false
				transpileHead.Start();
				transpileHead.Insert(
					new CodeInstruction(OpCodes.Ldc_I4_0),
					crcFlagStoreInst
				);

				return transpileHead.InstructionEnumeration();
			}

			_ = Transpiler(null);
			modelVersion = default;
			return null;
		}

		private static GameObject LoadSkinMesh_R_v2(string filename, TMorph morph, string slotname, TBodySkin bodySkin, int layer, Stream stream)
		{
			IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				if (instructions == null) return null;
				if (instructions.Count() == 0) throw new InvalidOperationException("instructions.Count() == 0");

				CodeMatcher transpileHead = new(instructions);
				LoadSkinMesh_R_v2_Transpile(transpileHead, new CodeInstruction(OpCodes.Ldarg_S, (byte)5));
				return transpileHead.InstructionEnumeration();
			}

			_ = Transpiler(null);
			return null;
		}
	
		private static void LoadSkinMesh_R_v2_Transpile(CodeMatcher transpileHead, CodeInstruction loadStreamArg)
		{
			transpileHead.Start();

			var sfield_m_skinTempFile = AccessTools.DeclaredField(typeof(ImportCM), "m_skinTempFile");
			var ctor_MemoryStream = AccessTools.DeclaredConstructor(typeof(MemoryStream), new Type[] { typeof(byte[]) });
			var smethod_Encoding_get_UTF8 = AccessTools.DeclaredPropertyGetter(typeof(Encoding), nameof(Encoding.UTF8));
			var ctor_BinaryReader = AccessTools.DeclaredConstructor(typeof(BinaryReader), new Type[] { typeof(Stream), typeof(Encoding) });
			CodeMatch[] match_NewBinaryReader =
			{
					// BinaryReader r = new BinaryReader((Stream) new MemoryStream(ImportCM.m_skinTempFile), Encoding.UTF8);
					//new(OpCodes.Ldarg_0)
					new(OpCodes.Ldsfld , sfield_m_skinTempFile    ),
					new(OpCodes.Newobj , ctor_MemoryStream        ),
					new(OpCodes.Call   , smethod_Encoding_get_UTF8),
					new(OpCodes.Newobj , ctor_BinaryReader        ),
					new((inst) => inst.opcode.ToString().StartsWith("stloc"))
				};

			// Since there is no file to load,
			// remove the try-catch.
			transpileHead.MatchForward(useEnd: false, match_NewBinaryReader);
			transpileHead.LogOrThrowMatch(smethod_LoadSkinMesh_R, nameof(match_NewBinaryReader), LiveLinkPlugin.Logger);

			int length = transpileHead.Pos;
			transpileHead.Start();
			transpileHead.RemoveInstructions(length);

			/* --- Convert  ---
			 * BinaryReader r = new BinaryReader((Stream) new MemoryStream(ImportCM.m_skinTempFile), Encoding.UTF8);
			 * ---    To    ---
			 * BinaryReader r = new BinaryReader((Stream) stream, Encoding.UTF8);
			 */
			transpileHead.RemoveInstructions(2);
			transpileHead.Insert(loadStreamArg);
		}
	}
}
