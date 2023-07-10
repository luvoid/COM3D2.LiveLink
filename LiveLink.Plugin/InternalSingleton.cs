using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin
{
	internal abstract class InternalSingleton<T> : MonoSingleton<T>
		where T : InternalSingleton<T>
	{
		protected static T Instance;
		public sealed override void OnInitialize()
		{
			Instance = this as T;
		}
		public sealed override void OnFinalize()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}
	}
}
