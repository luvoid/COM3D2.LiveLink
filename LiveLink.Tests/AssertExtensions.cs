using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

internal static class AssertExtensions
{
	public static void ArraysAreEqual<T>(this Assert _, in T[] expected, in T[] actual, string message = "")
	{
		Assert.AreEqual(expected.Length, actual.Length, $"Lengths don't match. {message}");
		for (int i = 0; i < expected.Length; i++)
		{
			Assert.AreEqual(expected[i], actual[i], $"Index:<{i}>. {message}");
		}
	}

	public static void StreamsAreEqual(this Assert _, in Stream expected, in Stream actual, string message = "")
	{
		long offset = 0;
		while ((expected.Position < expected.Length) && (actual.Position < actual.Length))
		{
			offset = expected.Position;
			int sizeExpected = (int)Math.Min(expected.Length - expected.Position, 64);
			int sizeActual   = (int)Math.Min(actual  .Length - actual  .Position, 64);
			byte[] arrayExpected = new byte[sizeExpected];
			byte[] arrayActual   = new byte[sizeActual  ];

			expected.Read(arrayExpected, 0, arrayExpected.Length);
			actual  .Read(arrayActual  , 0, arrayActual  .Length);

			Assert.That.ArraysAreEqual(arrayExpected, arrayActual, $"Offset:<0x{offset:X6}>. {message}\n" +
				$"Expected:<{arrayExpected.ToHexString()}>.\n" +
				$"Actual  :<{arrayActual  .ToHexString()}>.");
		}

		Assert.AreEqual(expected.Length, actual.Length, $"Offset:<0x{offset:X6}>. Stream lengths don't match. {message}");
	}

	public static string ToHexString(this byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in bytes)
		{
			stringBuilder.Append($"{b:X2} ");
		}
		return stringBuilder.ToString(0, stringBuilder.Length - 1);
	}

	public static void ExitZero(this Assert _, Process process, int timeout, string stdOutFile = null, string stdErrFile = null)
	{
		ExitCode(_, process, 0, timeout, stdOutFile, stdErrFile);
	}

	public static void ExitZero(this Assert _, Process process, string stdOutFile = null, string stdErrFile = null)
	{
		ExitCode(_, process, 0, stdOutFile, stdErrFile);
	}

	public static void ExitCode(this Assert _, Process process, int code, int timeout, string stdOutFile = null, string stdErrFile = null)
	{
		process.WaitForExit(timeout);
		Assert.IsTrue(process.HasExited, $"Process {process.StartInfo.FileName} did not exit within the aloted timeout period");
		Assert.That.ExitCode(process, code, stdOutFile, stdErrFile);
	}

	public static void ExitCode(this Assert _, Process process, int code, string stdOutFile = null, string stdErrFile = null)
	{
		if (!process.HasExited)
		{
			process.WaitForExit();
		}

		string stdOut = "Could not retrieve standard output";
		string stdErr = "Could not retrieve standard error";
		if (process.StartInfo.RedirectStandardOutput)
		{
			stdOut = process.StandardOutput.ReadToEnd();
		}
		else if (stdOutFile != null)
		{
			stdOut = File.ReadAllText(stdOutFile);
		}
		if (process.StartInfo.RedirectStandardError)
		{
			stdErr = process.StandardError.ReadToEnd();
		}
		else if (stdErrFile != null)
		{
			stdErr = File.ReadAllText(stdErrFile);
		}

		try
		{
			Assert.IsTrue(process.ExitCode == code, $"Process {process.StartInfo.FileName} exited with code {process.ExitCode}");
		}
		catch (AssertFailedException ex)
		{
			throw new AssertFailedException($"{ex.Message}\n{stdErr}", ex);
		}
		finally
		{
			Console.WriteLine(stdOut);
		}
		Console.Error.WriteLine(stdErr);
	}


}
