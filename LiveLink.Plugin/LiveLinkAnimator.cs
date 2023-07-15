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
		}

		void TargetFirstMaid()
		{
			if (TargetMaid != null && TargetMaid.isActiveAndEnabled) return;
			TargetMaid = GameObject.FindObjectOfType<Maid>();
		}

		internal static void SetAnimation(byte[] anmData)
		{
			TargetMaid.body0.GetAnimation().Stop();
			TargetMaid.body0.CrossFade("LiveLinkAnimator", anmData, loop: true, fade: 0.1f);
		}
	}
}
