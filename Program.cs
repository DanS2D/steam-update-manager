// Copyright 2018 <Revolt64>
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Diagnostics;
using System.IO;
using Utils;

namespace SteamAutoUpdates
{
	class Program
	{
		static void Main(string[] args)
		{
			CommandLineArgs.Parse(args);
			
			bool dryRun = CommandLineArgs.DryRun;
			int disableUpdates = CommandLineArgs.DisableUpdates ? 1 : 0;
			string steamPath = (CommandLineArgs.SteamPath.Length > 0) ? CommandLineArgs.SteamPath : @"C:\Program Files (x86)\Steam\steamapps";
			string operatingMode = dryRun ? "Dry (no changes will be written to file)" : "Live (changes will be written to file)";
			const string gameInfoExtension = ".acf";
			const string gameNameKey = "name";
			const string gameAutoUpdateKey = "AutoUpdateBehavior";
			const string keepUpToDateTitle = "Always keep this game up to date";
			const string onlyUpdateWhenLaunchedTitle = "Only update this game when I launch it";
			int changesMade = 0;

			/* steam manifest values 
				* 0 == always keep game up to date
				* 1 == only update when I launch it
			*/

			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.WriteLine("Steam Update Manager: V0.1");
			Console.ForegroundColor = ConsoleColor.DarkBlue;
			Console.WriteLine($"  Operating Mode: {operatingMode}\n");
			Console.ResetColor();

			// kill the steam process
			StopSteam();

			foreach (string filePath in Directory.EnumerateFiles(steamPath))
			{
				string fileExtension = Path.GetExtension(filePath);

				if (fileExtension.EndsWith(gameInfoExtension))
				{
					string[] lines = File.ReadAllLines(filePath);
					string gameName = string.Empty;
					bool changedThisFile = false;

					foreach (string line in lines)
					{
						if (line.Contains(gameNameKey))
						{
							gameName = line.Substring(6, line.Length - 6);
							gameName = gameName.Remove(0, 4).Remove(gameName.Length - 5, 1);
						}
						else if (line.Contains(gameAutoUpdateKey))
						{
							int i = Array.IndexOf(lines, line);
							int oldUpdateValue = Convert.ToInt16(lines[i].Substring(lines[i].Length - 2, 1));
							lines[i] = $@"	""{gameAutoUpdateKey}""		""{disableUpdates}""";
							int newUpdateValue = Convert.ToInt16(lines[i].Substring(lines[i].Length - 2, 1));
							string oldUpdateTitle = (oldUpdateValue == 0) ? keepUpToDateTitle : onlyUpdateWhenLaunchedTitle;
							string newUpdateTitle = (newUpdateValue == 0) ? keepUpToDateTitle : onlyUpdateWhenLaunchedTitle;

							if (oldUpdateValue != newUpdateValue)
							{
								Console.ForegroundColor = ConsoleColor.DarkMagenta;
								Console.Write(gameName);
								Console.ResetColor();
								Console.ResetColor();
								Console.Write(": Update behavior changed from ");
								Console.ForegroundColor = ConsoleColor.DarkRed;
								Console.Write($"'{oldUpdateTitle}' ");
								Console.ResetColor();
								Console.Write("to ");
								Console.ForegroundColor = ConsoleColor.Blue;
								Console.Write($"'{newUpdateTitle}'.\n");
								Console.ResetColor();
								changesMade++;
								changedThisFile = true;
							}
						}
					}

					// write the changes to file
					if (!dryRun && changedThisFile)
					{
						File.WriteAllLines(filePath, lines);
					}
				}
			}

			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write("  Finished! ");
			Console.ResetColor();

			if (changesMade > 0)
			{
				string info = $"Wrote changes to {changesMade} files!";

				if (dryRun)
				{
					info = $"Running this action in live mode would result in changes to {changesMade} files.";
				}

				Console.WriteLine(info);
			}
			else
			{
				string info = "No changes were required";

				if (dryRun)
				{
					info = "Running this action in live mode would result in no changes, as none were required.";
				}

				Console.WriteLine(info);
			}

			StartSteam();
		}

		static void StartSteam()
		{
			if (CommandLineArgs.DryRun) return;

			ProcessStartInfo startInfo = new ProcessStartInfo{
				FileName = @"C:\Program Files (x86)\Steam\Steam.exe"
			};
			Process.Start(startInfo);
		}

		static void StopSteam()
		{
			if (CommandLineArgs.DryRun) return;

			foreach (var process in Process.GetProcessesByName("steam"))
			{
				process.Kill();
			}
		}
	}
}
