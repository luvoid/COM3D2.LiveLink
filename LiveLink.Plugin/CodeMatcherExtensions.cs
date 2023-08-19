using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace COM3D2.LiveLink.Plugin
{
	internal static class CodeMatcherExtensions
	{
		/// <exception cref="InvalidOperationException"></exception>
		public static void LogOrThrowMatch(this CodeMatcher transpileHead, MethodInfo method, string nameofMatch, ManualLogSource logger)
		{
			if (transpileHead.ReportFailure(method, logger.LogError))
				throw new InvalidOperationException($"Could not find {nameofMatch} while transpiling {method}");
			else
				logger.LogDebug($"{nameofMatch}  @  IL_{transpileHead.Pos:X04}: {transpileHead.Instruction}");
		}
	}
}
