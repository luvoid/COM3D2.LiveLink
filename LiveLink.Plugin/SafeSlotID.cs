using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin
{
	internal enum SafeSlotID
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

	internal static class SafeSlotIDExtensions
	{
		public static TBody.SlotID ToSlotID(this SafeSlotID safeSlotID)
		{
			return SafeEnumHelper<SafeSlotID, TBody.SlotID>.ToUnsafe(safeSlotID);
		}
		public static SafeSlotID ToSafeSlotID(this TBody.SlotID slotID)
		{
			return SafeEnumHelper<SafeSlotID, TBody.SlotID>.ToSafe(slotID, SafeSlotID.end);
		}
	}
}
