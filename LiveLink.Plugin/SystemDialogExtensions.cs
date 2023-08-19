using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin
{
	using static SystemDialog;

	internal static class SystemDialogExtensions
	{

		public static void ShowEnqueue(this SystemDialog dialog, string message, TYPE type = TYPE.OK, 
			OnClick onOk = null, OnClick onCancel = null)
		{
			GameMain.Instance.StartCoroutine(ShowEnqueueCoroutine(dialog, message, type, onOk, onCancel));
		}

		private static IEnumerator ShowEnqueueCoroutine(SystemDialog dialog, string message, TYPE type, 
			OnClick onOk = null, OnClick onCancel = null)
		{
			while (!dialog.IsDecided)
			{
				yield return new WaitUntil(() => dialog.IsDecided);
			}
			dialog.Show(message, type, onOk, onCancel);
		}
	}
}
