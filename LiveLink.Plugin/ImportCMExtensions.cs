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
	[HarmonyPatch(typeof(ImportCM))]
	internal static class ImportCMExtensions
	{
		[HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
		[HarmonyPatch(nameof(ImportCM.LoadSkinMesh_R))]
		public static GameObject LoadSkinMesh_R(Stream stream, TMorph morph, string slotname, TBodySkin bodySkin, int layer)
		{
			IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				if (instructions == null) return null;
				if (instructions.Count() == 0) throw new InvalidOperationException("instructions.Count() == 0");

				var transpileHead = new CodeMatcher(instructions);

				try
				{
					var smethod_LoadSkinMesh_R = AccessTools.DeclaredMethod(typeof(ImportCM), nameof(ImportCM.LoadSkinMesh_R));

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
						new(OpCodes.Stloc_2)
					};

					// Since there is no file to load,
					// remove everything up to the creation of the binary loader.
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
					transpileHead.Insert(new CodeInstruction(OpCodes.Ldarg_0));

					return transpileHead.InstructionEnumeration();
				}
				catch (CannotUnloadAppDomainException e) 
				{
					throw e;
				}
				finally 
				{
					//LiveLinkPlugin.Logger.LogDebug("");
					//LiveLinkPlugin.Logger.LogDebug("");
					//foreach (var instr in transpileHead.InstructionEnumeration())
					//{
					//	LiveLinkPlugin.Logger.LogDebug(instr);
					//}
				}
			}

			_ = Transpiler(null);
			return null;
		}
	}
}
