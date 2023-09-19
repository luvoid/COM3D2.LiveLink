using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin
{
	internal ref struct SafeGoSlot
	{
		public static readonly bool SlotTypeExists = typeof(TBody).GetNestedType(nameof(TBody.Slot)) != null;

		private static readonly FieldInfo slotField = AccessTools.DeclaredField(typeof(TBody), nameof(TBody.goSlot));


		private object slotObject;
		private List<TBodySkin> SlotAsList => SlotTypeExists ? null : slotObject as List<TBodySkin>;
		public SafeGoSlot(TBody body)
		{
			slotObject = slotField.GetValue(body);
		}

		public TBodySkin this[int i]
		{
			get => SlotTypeExists ? UnsafeGet(i) : SlotAsList[i];

			set
			{
				if (SlotTypeExists)
				{
					UnsafeSet(i, value);
				}
				else
				{
					SlotAsList[i] = value;
				}
			}
		}
		private TBodySkin UnsafeGet(int i) => (slotObject as TBody.Slot)[i];
		private TBodySkin UnsafeSet(int i, TBodySkin value) => (slotObject as TBody.Slot)[i] = value;


		[System.Obsolete($"This method does not exist in 2.x. Check that {nameof(SafeGoSlot)}.{nameof(SlotTypeExists)} == true before using it.")]
		public TBodySkin this[int i, int j]
		{
			get => (slotObject as TBody.Slot)[i, j];
			set => (slotObject as TBody.Slot)[i, j] = value;
		}
		public int Count => SlotTypeExists ? UnsafeCount() : SlotAsList.Count;
		private int UnsafeCount() => (slotObject as TBody.Slot).Count;

		public void Add(TBodySkin bodySkin)
		{
			if (SlotTypeExists)
			{
				UnsafeAdd(bodySkin);
			}
			else
			{
				SlotAsList.Add(bodySkin);
			}
		}
		private void UnsafeAdd(TBodySkin bodySkin) => (slotObject as TBody.Slot).Add(bodySkin);

		[System.Obsolete($"This method does not exist in 2.x. Check that {nameof(SafeGoSlot)}.{nameof(SlotTypeExists)} == true before using it.")]
		public List<TBodySkin> GetListParents() => (slotObject as TBody.Slot).GetListParents();

		[System.Obsolete($"This method does not exist in 2.x. Check that {nameof(SafeGoSlot)}.{nameof(SlotTypeExists)} == true before using it.")]
		public int CountChildren(int slotNo) => (slotObject as TBody.Slot).CountChildren(slotNo);

		[System.Obsolete($"This method does not exist in 2.x. Check that {nameof(SafeGoSlot)}.{nameof(SlotTypeExists)} == true before using it.")]
		public bool HasChildren(int slotNo) => (slotObject as TBody.Slot).HasChildren(slotNo);

		[System.Obsolete($"This method does not exist in 2.x. Check that {nameof(SafeGoSlot)}.{nameof(SlotTypeExists)} == true before using it.")]
		public void AddChild(int i, TBodySkin bodySkin) => (slotObject as TBody.Slot).AddChild(i, bodySkin);

		[System.Obsolete($"This method does not exist in 2.x. Check that {nameof(SafeGoSlot)}.{nameof(SlotTypeExists)} == true before using it.")]
		public void DeleteObjAll() => (slotObject as TBody.Slot).DeleteObjAll();


	}

	internal static class SafeGoSlotExtensions
	{
		public static SafeGoSlot SafeGoSlot(this TBody body)
		{
			return new SafeGoSlot(body);
		}
	}
}
