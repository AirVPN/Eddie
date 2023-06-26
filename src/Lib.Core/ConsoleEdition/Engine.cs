// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using System;

namespace Eddie.Core.ConsoleEdition
{
	public class Engine : Core.Engine
	{
		bool EnableLogOnConsole = true;

		public Engine(string environmentCommandLine) : base(environmentCommandLine)
		{
			ConsoleMode = true;
		}

		public override bool OnInit()
		{
			if (StartCommandLine.Exists("version"))
			{
				EnableLogOnConsole = false;
				Console.WriteLine(Constants.Name + " - version " + GetVersionShow());
				return false;
			}
			else if (StartCommandLine.Exists("version.short"))
			{
				EnableLogOnConsole = false;
				Console.WriteLine(GetVersionShow());
				return false;
			}
			else if (StartCommandLine.Exists("help"))
			{
				EnableLogOnConsole = false;
				ProfileOptions o = new ProfileOptions();
				Console.WriteLine(o.GetMan(StartCommandLine.Get("help.format", "text")));

				return false;
			}

			Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

			return base.OnInit();
		}

		public override bool OnInitUi()
		{
			string login = ProfileOptions.Get("login").Trim();
			string password = ProfileOptions.Get("password").Trim();

			if ((login == "") || (password == ""))
			{
				Logs.Log(LogType.Fatal, LanguageManager.GetText(LanguageItems.ConsoleHelp));
				return false;
			}
			else
			{
				if (StartCommandLine.Exists("batch") == false)
				{
					Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConsoleKeyboardHelp));

					if (ProfileOptions.GetBool("connect") == false)
						Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConsoleKeyboardHelpNoConnect));
				}
			}

			return true;
		}

		public override void OnWork()
		{
			base.OnWork();

			if (ConsoleMode)
			{
				if (StartCommandLine.Exists("batch") == false)
				{
					try
					{
						// try/catch because throw an exception if stdin is redirected.
						// May in future we can use Console.IsInputRedirected, but require .Net 4.5								
						if (Console.KeyAvailable)
						{
							char ch = ReadConsoleKeyInsensitive();

							if (ch == 'x')
							{
								Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConsoleKeyCancel));
								if (Engine.IsConnected())
								{
									Disconnect();
								}
								else
								{
									Exit();
								}
							}
							else if (ch == 'n')
							{
								Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConsoleKeySwitch));
								if (Engine.IsConnected())
								{
									SwitchServer = true;
								}
								else
								{
									if ((Engine.CanConnect()) && (Engine.IsWaiting() == false))
										Connect();
								}
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Console error: " + ex.Message);
					}
				}
			}
		}

		public override void OnLog(LogEntry l)
		{
			if (EnableLogOnConsole == false)
				return;

			base.OnLog(l);
		}

		public override string OnAskProfilePassword(bool authFailed)
		{
			if (authFailed == false)
				Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.WindowsUnlockFirstAuth));
			else
				Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.WindowsUnlockFailedAuth));

			string password = Console.ReadLine();
			return password;
		}

		public override bool OnAskYesNo(string message)
		{
			Logs.Log(LogType.Info, message + " (y/n)");
			for (; ; )
			{
				string answer = Console.ReadLine().ToLowerInvariant();
				if (answer == "y")
					return true;
				else if (answer == "n")
					return false;
			}
		}

		public override Json OnAskExecExternalPermission(Json data)
		{
			Json Answer = new Json();
			Answer["allow"].Value = false;

			for (; ; )
			{
				Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionTop, data["path"].Value as string));
				Logs.Log(LogType.Verbose, "N: " + LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionNo));
				Logs.Log(LogType.Verbose, "Y: " + LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionYes));
				if ((data["sign-id"].Value as string).StartsWith("No: ") == false)
					Logs.Log(LogType.Verbose, "S: " + LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionRuleSign, data["sign-id"].Value as string));
				Logs.Log(LogType.Verbose, "H: " + LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionRuleHash, data["sha256"].Value as string));
				Logs.Log(LogType.Verbose, "P: " + LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionRulePath, data["path"].Value as string));
				Logs.Log(LogType.Verbose, "A: " + LanguageManager.GetText(LanguageItems.WindowsExecExternalPermissionRuleAll));

				char ch = ReadConsoleKeyInsensitive();

				if (ch == 'n')
				{
					Answer["allow"].Value = false;
					break;
				}
				else if (ch == 'y')
				{
					Answer["allow"].Value = true;
					break;
				}
				else if ((ch == 's') && ((data["sign-id"].Value as string).StartsWith("No: ") == false))
				{
					Answer.RemoveKey("allow");
					Answer["type"].Value = "sign";
					Answer["id"].Value = data["sign-id"].Value;
					break;
				}
				else if (ch == 'h')
				{
					Answer.RemoveKey("allow");
					Answer["type"].Value = "sha256";
					Answer["hash"].Value = data["sha256"].Value;
					break;
				}
				else if (ch == 'p')
				{
					Answer.RemoveKey("allow");
					Answer["type"].Value = "path";
					Answer["path"].Value = data["path"].Value;
					break;
				}
				else if (ch == 'a')
				{
					Answer.RemoveKey("allow");
					Answer["type"].Value = "all";
					break;
				}
			}

			return Answer;
		}

		private char ReadConsoleKeyInsensitive()
		{
			ConsoleKeyInfo c = Console.ReadKey();
			return Char.ToLowerInvariant(c.KeyChar);
		}

		private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			m_breakRequests++;

			if (m_breakRequests == 1)
			{
				e.Cancel = true;

				Logs.Log(LogType.Info, LanguageManager.GetText(LanguageItems.ConsoleKeyBreak));

				Exit();
			}
		}
	}

}
