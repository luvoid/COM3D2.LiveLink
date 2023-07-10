using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CM3D2.Serialization;

namespace COM3D2.LiveLink.CLI.Commands
{
	internal class SendCommand : Command
	{
		public override string Name => "send";

		private LiveLinkCore m_Core;

		private MemoryStream m_TempStream;
		private CM3D2Serializer m_Serializer;


		private static Parse.ArgDef s_StringArgDef = new Parse.ArgDef("--string", "-s");
		private static Parse.ArgDef s_HexArgDef    = new Parse.ArgDef("--hex"   , "-x");
		private static Parse.ArgDef s_FileArgDef   = new Parse.ArgDef("--file"  , "-f");
		private static Parse.ArgDef[] s_ArgDefs =
		{
			s_StringArgDef,
			s_HexArgDef,
			s_FileArgDef,
		};

		public SendCommand(LiveLinkCore core)
		{
			m_Core = core;
			m_Serializer = new CM3D2Serializer();
		}

		public override int Run(in string[] args)
		{
			DebugConsole.Write("[ ");
			foreach (var arg in args)
			{
				DebugConsole.Write(arg);
				DebugConsole.Write(" ");
			}
			DebugConsole.WriteLine("]");

			List<Parse.ArgMatch> argMatches = Parse.Args(args, s_ArgDefs);
			if (argMatches.Count <= 0)
			{
				ErrorConsole.WriteLine("Must specify data format");
				return 1;
			}

			m_TempStream = new MemoryStream();
			foreach (var argMatch in argMatches)
			{
				try
				{
					if      (argMatch.Definition == s_StringArgDef) OnStringArg(argMatch);
					else if (argMatch.Definition == s_HexArgDef   ) OnHexArg   (argMatch);
					else if (argMatch.Definition == s_FileArgDef  ) OnFileArg  (argMatch);
				}
				catch (Exception e)
				{
					ErrorConsole.WriteLine(e.Message);
					return 1;
				}
			}

			if (m_TempStream.Length > m_TempStream.Position)
			{
				m_TempStream.SetLength(m_TempStream.Position);
			}

			try
			{
				m_Core.SendBytes(m_TempStream.GetBuffer(), (int)m_TempStream.Length);
			}
			catch (Exception e)
			{
				ErrorConsole.WriteLine(e.Message);
				return 1;
			}

			return 0;
		}

		void OnStringArg(Parse.ArgMatch argMatch)
		{
			string str = string.Join(" ", argMatch.Values.ToArray());
			m_Serializer.Serialize(m_TempStream, str);
		}

		void OnHexArg(Parse.ArgMatch argMatch)
		{
			var vals = string.Join("", argMatch.Values.ToArray());
			byte[] bytes = ConvertFromHex(vals);
			m_TempStream.Write(bytes, 0, bytes.Length);
		}

		void OnFileArg(Parse.ArgMatch argMatch)
		{
			using (var fileStream = new FileStream(argMatch.Values[0], FileMode.Open, FileAccess.Read))
			{
				long offset = m_TempStream.Position;
				long newOffset = offset + fileStream.Length;
				if (m_TempStream.Length < newOffset)
				{
					m_TempStream.SetLength(newOffset);
				}
				fileStream.Read(m_TempStream.GetBuffer(), (int)offset, (int)fileStream.Length);
				m_TempStream.Position = newOffset;
			}
		}


		byte[] ConvertFromHex(string s)
		{
			List<byte> data = new List<byte>();
			s = s.Trim();
			for (int i = 0; i < s.Length; i += 2) 
			{
				string sub = s.Substring(i, Math.Min(i + 1, s.Length));
				data.Add(byte.Parse(sub, System.Globalization.NumberStyles.HexNumber));
			}
			return data.ToArray();
		}
	}
}
