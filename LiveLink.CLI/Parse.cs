using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.LiveLink.CLI
{
	internal static class Parse
	{
		public static string[] StringToArray(in string arguments)
		{
			return arguments.Split(' ');
		}

		public static bool NamedFlag(in IEnumerable<string> args, params string[] names)
		{
			foreach (string arg in args)
			{
				foreach (string name in names)
				{
					if (arg == name) return true;
				}
			}
			return false;
		}

		public static bool NamedString(in IEnumerable<string> args, out string value, params string[] names)
		{
			string foundName = null;
			foreach (string arg in args)
			{
				if (foundName != null)
				{
					value = arg;
					return true;
				}

				foreach (string name in names)
				{
					if (arg == name)
					{
						foundName = name;
						continue;
					}
				}
			}

			if (foundName != null)
			{
				Console.WriteLine($"Expected a value after argument '{foundName}'");
			}

			value = null;
			return false;
		}

		public static bool NamedInt(in IEnumerable<string> args, out int value, params string[] names)
		{
			value = 0;
			if (NamedString(args, out string stringValue, names))
			{
				if (int.TryParse(stringValue, out value))
				{
					return true;
				}
				else
				{
					Console.WriteLine($"Expected an integer after argument '{names[0]}'");
				}
			}
			return false;
		}




		public static List<ArgMatch> Args(in IEnumerable<string> arguments, in IEnumerable<ArgDef> argumentDefinitions)
		{
			List<ArgMatch> matches = new List<ArgMatch>();
			ArgMatch lastMatch = null;

			foreach (string arg in arguments)
			{
				foreach (ArgDef argDef in argumentDefinitions)
				{
					if (argDef.Match(arg, out ArgMatch match))
					{
						matches.Add(match);
						lastMatch = match;
						goto CONTINUE_ARG_LOOP;
					}
				}

				// else, if arg has no match

				lastMatch?.Values.Add(arg);

			CONTINUE_ARG_LOOP:
				continue;
			}

			return matches;
		}

		public class ArgDef
		{
			public readonly string[] Names;

			public ArgDef(params string[] names)
			{
				Names = names;
			}

			public bool Match(string arg, out ArgMatch match)
			{
				foreach (string name in Names)
				{
					if (arg != name) continue;

					match = new ArgMatch(this, name);
					return true;
				}

				match = null;
				return false;
			}
		}

		public class ArgMatch
		{
			public readonly ArgDef Definition;
			public readonly string NameUsed;
			public readonly List<string> Values = new List<string>();

			public ArgMatch(ArgDef definition, string nameUsed)
			{
				Definition = definition;
				NameUsed = nameUsed;
			}
		}
	}

	
}
