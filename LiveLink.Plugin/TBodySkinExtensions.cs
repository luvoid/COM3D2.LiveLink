using HarmonyLib;
using System;
using System.Reflection;

namespace COM3D2.LiveLink.Plugin
{
	internal static class TBodySkinExtensions
	{
		private static readonly MethodInfo deleteObjMethod = AccessTools.DeclaredMethod(typeof(TBodySkin), nameof(TBodySkin.DeleteObj));
		private static readonly ParameterInfo[] deleteObjParameters = deleteObjMethod.GetParameters();
		public static void SafeDeleteObj(this TBodySkin bodySkin)
		{
			object[] args = deleteObjParameters.Length > 0 ? new object[] { false } : null;
			deleteObjMethod.Invoke(bodySkin, args);
		}
	}
}
