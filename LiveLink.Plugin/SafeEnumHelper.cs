using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.Plugin
{
	internal static class SafeEnumHelper<TSafe, TUnsafe>
		where TSafe : Enum
		where TUnsafe : Enum
	{
		private static readonly SortedDictionary<TSafe, TUnsafe> safeMap = new();
		private static readonly SortedDictionary<TUnsafe, TSafe> unsafeMap = new();

		public static TUnsafe ToUnsafe(TSafe safeEnum)
		{
			if (!safeMap.TryGetValue(safeEnum, out TUnsafe unsafeEnum))
			{
				unsafeEnum = (TUnsafe)Enum.Parse(typeof(TUnsafe), safeEnum.ToString());
				safeMap.Add(safeEnum, unsafeEnum);
			}
			return unsafeEnum;
		}

		public static TSafe ToSafe(TUnsafe unsafeEnum, TSafe fallbackValue)
		{
			if (!unsafeMap.TryGetValue(unsafeEnum, out TSafe safeEnum))
			{
				try
				{
					safeEnum = (TSafe)Enum.Parse(typeof(TSafe), safeEnum.ToString());
				}
				catch (ArgumentException)
				{
					safeEnum = fallbackValue;
				}
				unsafeMap.Add(unsafeEnum, safeEnum);
			}
			return safeEnum;
		}
	}
}
