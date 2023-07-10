using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace COM3D2.LiveLink
{
	public static class StreamExtensions
	{
		public static string ReadAllAvailable(this StreamReader streamReader)
		{
			StringBuilder stringBuilder = new StringBuilder();
			char[] buffer = new char[1024];
			int peek;
			while ((peek = streamReader.Peek()) != -1)
			{
				int charCount = streamReader.Read(buffer, 0, 1024);
				stringBuilder.Append(buffer, 0, charCount);
			}
			return stringBuilder.ToString();
		}
	}
}
