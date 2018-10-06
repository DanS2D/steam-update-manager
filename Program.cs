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
			bool clearQueuedUpdates = CommandLineArgs.ClearQueuedUpdates;
			CommandLineArgs.QueuedUpdateClearMode clearQueuedUpdateMode = CommandLineArgs.ClearQueuedUpdateMode;
			int disableUpdatesValue = CommandLineArgs.DisableUpdates ? 1 : 0;
			string steamPath = (CommandLineArgs.SteamPath.Length > 0) ? CommandLineArgs.SteamPath : @"C:\Program Files (x86)\Steam\steamapps";
			string operatingMode = dryRun ? "Dry (no changes will be written to file)" : "Live (changes will be written to file)";
			const string gameInfoExtension = ".acf";
			const string gameNameKey = "name";
			const string gameAutoUpdateKey = "AutoUpdateBehavior";
			const string gameStateFlagsKey = "StateFlags";
			const string gameUpdateResultKey = "UpdateResult";
			const string gameBytesToDownloadKey = "BytesToDownload";
			const string gameBytesDownloadedKey = "BytesDownloaded";
			const string keepUpToDateTitle = "Always keep this game up to date";
			const string onlyUpdateWhenLaunchedTitle = "Only update this game when I launch it";
			int changesMade = 0;
			const int gameStateUpdated = 4;
			const int gameUpdateDone = 0;

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
					bool changedThisFile = false;
					string gameName = Array.Find(lines, n => n.Contains(gameNameKey));
					string gameAutoupdate = Array.Find(lines, n => n.Contains(gameAutoUpdateKey));
					string gameUpdateResult = Array.Find(lines, n => n.Contains(gameUpdateResultKey));
					string gameBytesToDownload = Array.Find(lines, n => n.Contains(gameBytesToDownloadKey));
					string gameBytesDownloaded = Array.Find(lines, n => n.Contains(gameBytesDownloadedKey));
					string gameStateFlags = Array.Find(lines, n => n.Contains(gameStateFlagsKey));

					if (gameName.Length > 0)
					{
						gameName = gameName.Substring(6, gameName.Length - 6);
						gameName = gameName.Remove(0, 4).Remove(gameName.Length - 5, 1);

						Console.ForegroundColor = ConsoleColor.DarkRed;
						Console.WriteLine(gameName);
						Console.ResetColor();

						if (gameAutoUpdateKey.Length > 0)
						{
							long autoUpdateValue = GetNumericValue(gameAutoupdate);

							if (autoUpdateValue != disableUpdatesValue)
							{
								string oldUpdateTitle = (autoUpdateValue == 0) ? keepUpToDateTitle : onlyUpdateWhenLaunchedTitle;
								string newUpdateTitle = (disableUpdatesValue == 0) ? keepUpToDateTitle : onlyUpdateWhenLaunchedTitle;
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
								
								lines[Array.IndexOf(lines, gameAutoupdate)] = $@"	""{gameAutoUpdateKey}""		""{disableUpdatesValue}""";
								changedThisFile = true;
							}
						}

						if (gameUpdateResult.Length > 0)
						{
							long updateResult = GetNumericValue(gameUpdateResult);

							if (clearQueuedUpdates)
							{
								if (updateResult != gameUpdateDone)
								{
									int index = Array.IndexOf(lines, gameUpdateResult);
								
									// "UpdateResult"	"0" == updated
									switch (clearQueuedUpdateMode)
									{
										case CommandLineArgs.QueuedUpdateClearMode.All:
											lines[index] = $@"	""{gameUpdateResultKey}""		""{gameUpdateDone}""";
											break;

										case CommandLineArgs.QueuedUpdateClearMode.Partial:
											lines[index] = $@"	""{gameUpdateResultKey}""		""{gameUpdateDone}""";
											break;

										case CommandLineArgs.QueuedUpdateClearMode.ExcludePartial:
											lines[index] = $@"	""{gameUpdateResultKey}""		""{gameUpdateDone}""";
											break;
									}

									Console.WriteLine($"Update value changed from '{updateResult}' to '{gameUpdateDone}' (updated)");

									changedThisFile = true;
								}

								if (gameBytesToDownload.Length > 0 && gameBytesDownloaded.Length > 0)
								{
									long bytesToDownload = GetNumericValue(gameBytesToDownload);
									long bytesDownloaded = GetNumericValue(gameBytesDownloaded);

									Console.Write("bytes to download: ");
									Console.ForegroundColor = ConsoleColor.DarkBlue;
									Console.Write(bytesToDownload);
									Console.ResetColor();
									Console.Write("/");
									Console.ForegroundColor = ConsoleColor.DarkRed;
									Console.Write($"{bytesDownloaded}\n");
									Console.ResetColor();
								}

								if (gameStateFlags.Length > 0)
								{
									long currentState = GetNumericValue(gameStateFlags);
									int index = Array.IndexOf(lines, gameStateFlags);

									if (currentState == gameStateUpdated) continue;

									// "StateFlags"	"4" == no update required
									switch (clearQueuedUpdateMode)
									{
										case CommandLineArgs.QueuedUpdateClearMode.All:
											lines[index] = $@"	""{gameStateFlagsKey}""		""{gameStateUpdated}""";
											break;

										case CommandLineArgs.QueuedUpdateClearMode.Partial:
											lines[index] = $@"	""{gameStateFlagsKey}""		""{gameStateUpdated}""";
											break;

										case CommandLineArgs.QueuedUpdateClearMode.ExcludePartial:
											lines[index] = $@"	""{gameStateFlagsKey}""		""{gameStateUpdated}""";
											break;
									}

									Console.WriteLine($"StateFlags value changed from '{currentState}' to '{gameStateUpdated}' (updated)");

									changedThisFile = true;
								}
							}
						}

						if (changedThisFile) changesMade++;

						// write the changes to this file
						if (!dryRun && changedThisFile)
						{
							File.WriteAllLines(filePath, lines);
						}
					}
				}
			}

			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write("  Finished! ");
			Console.ResetColor();

			if (changesMade > 0)
			{
				string info = $"Wrote changes to {changesMade} file(s)!";

				if (dryRun)
				{
					info = $"Running this action in live mode would result in changes to {changesMade} file(s).";
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

		static long GetNumericValue(string input)
		{
			int startIdx = input.IndexOfAny("0123456789".ToCharArray());
			int endIdx = input.Length - startIdx - 1;
			string value = input.Substring(startIdx, endIdx);

			return Convert.ToInt64(value);
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
