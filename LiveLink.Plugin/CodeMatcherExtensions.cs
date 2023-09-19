using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace COM3D2.LiveLink.Plugin
{
	internal static class CodeMatcherExtensions
	{
		/// <exception cref="InvalidOperationException"></exception>
		public static void LogOrThrowMatch(this CodeMatcher transpileHead, MethodInfo method, string nameofMatch, ManualLogSource logger = null)
		{
			Action<string> logError = logger != null ? logger.LogError : Console.Error.WriteLine;
			Action<string> logDebug = logger != null ? logger.LogDebug : Console.Error.WriteLine;

			if (transpileHead.ReportFailure(method, logError))
			{
				throw new InvalidOperationException($"Could not find {nameofMatch} while transpiling {method}");
			}
			else
			{
				logDebug($"{nameofMatch}  @  OP_{transpileHead.Pos:X04}: {transpileHead.Instruction}");
			}
		}
	}
}
