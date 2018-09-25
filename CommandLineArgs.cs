// Copyright 2018 <Revolt64>
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
	public static class CommandLineArgs
	{
		private static List<string> _args;
		public static bool DryRun { get; set; }
		public static bool DisableUpdates { get; set; }
		public static string SteamPath { get; set; }

		private static class Arguments
		{
			public const string DryRun = "--dry";
			public const string SteamPath = "--path";
			public const string DisableUpdates = "--disable";
		}

		public static void Parse(string[] cmdArgs)
		{
			_args = cmdArgs.ToList();
			DryRun = ArgExists(Arguments.DryRun);
			DisableUpdates = ArgExists(Arguments.DisableUpdates);
			SteamPath = ArgExists(Arguments.SteamPath) ? ParseArgValue(Arguments.SteamPath, ParseArg(Arguments.SteamPath)) : string.Empty;
		}

		private static bool ArgExists(string name) => (ParseArg(name) >= 0);

		private static int ParseArg(string name) => _args.IndexOf(name);

		private static string ParseArgValue(string name, int index)
		{
			int argIndex = (index + 1);

			if (argIndex < 0 || (argIndex >= _args.Count))
			{
				Console.WriteLine($"unable to process argument '{name}'. Please ensure you have passed a value after the '{name}' argument.");
				return string.Empty;
			}

			return _args[argIndex];
		}
	}
}
