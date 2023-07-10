using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin
{
	internal class LiveLinkAnimator : InternalSingleton<LiveLinkAnimator>
	{
		[field: SerializeField] public static Maid TargetMaid { get; private set; } = null;

		void Update()
		{
			TargetFirstMaid();
			UpdateMaidPositionOffset();
		}

		void TargetFirstMaid()
		{
			if (TargetMaid != null && TargetMaid.isActiveAndEnabled) return;
			TargetMaid = GameObject.FindObjectOfType<Maid>();
		}

		void UpdateMaidPositionOffset()
		{
			if (TargetMaid == null) return;
			TargetMaid.gameObject.transform.localPosition = m_MaidPositionOffset;
		}

		private Vector3 m_MaidPositionOffset = Vector3.zero;
		public static void SetMaidPositionOffset(Vector3 offset)
		{
			Instance.m_MaidPositionOffset = offset;
		}

		internal static void SetAnimation(byte[] anmData)
		{
			TargetMaid.body0.CrossFade("LiveLinkAnimator", anmData, loop: true, fade: 0.1f);
		}
	}
}
