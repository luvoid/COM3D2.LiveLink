using HarmonyLib;
using System;
using System.Reflection;
using ShapekeyMaster;
using static ShapekeyMaster.SMEventsAndArgs;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Utils;

namespace COM3D2.LiveLink.Plugin
{
	internal static class ShapekeyMasterUtils
	{
		private static readonly Type mainType = AccessTools.TypeByName($"{nameof(ShapekeyMaster)}.{nameof(Main)}");
		public static readonly bool Exists = mainType != null;
		
		/*
		private static class HarmonyPatchers
		{
			private static readonly Type type = AccessTools.TypeByName($"{nameof(ShapekeyMaster)}.{nameof(ShapekeyMaster.HarmonyPatchers)}");
			private static readonly EventInfo morphEventInfo = type.GetEvent("MorphEvent");
		}

		private static class SMEventsAndArgs
		{
			public class MorphEventArgs
			{
				private static readonly Type type = AccessTools.TypeByName($"{nameof(ShapekeyMaster)}.{nameof(ShapekeyMaster.SMEventsAndArgs)}.{nameof(ShapekeyMaster.SMEventsAndArgs.MorphEventArgs)}");
				private static readonly ConstructorInfo constructor = AccessTools.DeclaredConstructor(type, new Type[] { typeof(TMorph), typeof(bool) });
				public readonly object WrappedObject;
				public MorphEventArgs(TMorph changedMorph, bool wasCreated = true)
				{
					WrappedObject = constructor.Invoke(null, new object[] { changedMorph, wasCreated });
				}
			}
		}
		*/

		public static bool TryInvokeMorphEvent(TMorph morph, bool wasCreated = true)
		{
			if (!Exists) return false;
			InvokeMorphEvent(morph, wasCreated);
			return true;
		}

		private static FieldInfo morphEventField;
		private static void InvokeMorphEvent(TMorph morph, bool wasCreated = true)
		{
			morphEventField ??= AccessTools.DeclaredField(typeof(HarmonyPatchers), nameof(HarmonyPatchers.MorphEvent));
			MorphEventArgs args = new(morph, wasCreated);
			EventHandler morphEvent = morphEventField.GetValue(null) as EventHandler;
			morphEvent?.Invoke(null, args);
		}

		public static bool TryUpdateMorphDicImmediate(TMorph morph, bool wasCreated = true)
		{
			if (!Exists) return false;
			UpdateMorphDicImmediate(morph, wasCreated);
			return true;
		}

		private static void UpdateMorphDicImmediate(TMorph morph, bool wasCreated = true)
		{
			var morphShapekeyDictionary = UI.SKDatabase.morphShapekeyDictionary;
			if (wasCreated)
			{
				string maid = morph.bodyskin.body.maid.status.fullNameJpStyle;
				foreach (ShapeKeyEntry shapeKeyEntry 
					in UI.SKDatabase.globalShapekeyDictionary.Values.Concat(UI.SKDatabase.ShapekeysByMaid(maid).Values))
				{
					if (!morph.hash.ContainsKey(shapeKeyEntry.ShapeKey))
						continue;

					if (!morphShapekeyDictionary.TryGetValue(morph, out HashSet<ShapeKeyEntry> morphShapekeyEntries))
					{
						morphShapekeyDictionary[morph] = morphShapekeyEntries = new();
					}
					morphShapekeyEntries.Add(shapeKeyEntry);
				}
			}
			else if (morphShapekeyDictionary.ContainsKey(morph))
			{
				morphShapekeyDictionary.Remove(morph);
			}
		}
	}
}
