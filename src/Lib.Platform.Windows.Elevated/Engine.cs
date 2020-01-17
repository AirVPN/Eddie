// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lib.Platform.Windows.Elevated
{
	public class Engine
	{
		public Thread m_ipcThread = null;
		private IpcBase m_ipc = new IpcSocket();
		private Random m_randomGenerator = new Random();		

		public static Dictionary<string, string> ParseCommandLine(string[] args)
		{
			Dictionary<string, string> cmdline = new Dictionary<string, string>();

			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i];

				string k = "";
				string v = "";

				if (i == 0)
				{
					k = "path";
					v = arg;
				}
				else
				{
					int posEq = arg.IndexOf('=');
					if (posEq != -1)
					{
						k = arg.Substring(0, posEq);
						v = arg.Substring(posEq + 1);
					}
					else
					{
						k = arg;
						v = "";
					}
				}
				cmdline[k] = v;
			}

			return cmdline;
		}

		public void Start(Dictionary<string, string> cmdline)
		{
			if (m_ipcThread != null)
				return;

			IPC.CommandEvent += IPC_CommandEvent;
			IPC.m_cmdline = cmdline;
			IPC.m_serviceMode = ( (cmdline.ContainsKey("mode")) && (cmdline["mode"] == "service") );

			IPC.LogDebug("Start");

			m_ipcThread = new Thread(new ThreadStart(IPC.MainLoop));
			m_ipcThread.Start();
		}

		public void Stop(bool force)
		{
			if (m_ipcThread == null)
				return;
							
			if (force)
			{
				m_ipc.LogDebug("Stop requested");
				m_ipc.m_serviceMode = false;
				m_ipc.Close("Quit");
			}
			m_ipcThread.Join();
			IPC.CommandEvent -= IPC_CommandEvent;
			m_ipc.LogDebug("Stop completed");
		}
		
		public IpcBase IPC
		{
			get
			{
				return m_ipc;
			}
		}

		private void IPC_CommandEvent(Dictionary<string, string> parameters)
		{
			DoCommand(parameters["_id"], parameters["command"], parameters);
		}

		private void DoCommand(string id, string command, Dictionary<string, string> parameters)
		{
			ThreadPool.QueueUserWorkItem((o) =>
			{
				try
				{
					if (IPC.m_sessionKey == "")
					{
						if (command == "session-key")
						{
							string clientVersion = "";
							if (parameters.ContainsKey("version"))
								clientVersion = parameters["version"];

							if(clientVersion != Constants.Version)
								throw new Exception("Unexpected version, elevated: " + Constants.Version + ", client: " + clientVersion);

							IPC.m_sessionKey = parameters["key"];
						}
						else
						{
							throw new Exception("Not init.");
						}
					}
					else if ((parameters.ContainsKey("_token") == false) || (IPC.m_sessionKey != parameters["_token"]))
					{
						throw new Exception("Not auth.");
					}
					else if (command == "exit")
					{
					}
					/*
					else if (command == "stress")
					{
						IPC.ReplyCommand(id, "0");
						Thread.Sleep(m_randomGenerator.Next() % 1000);
						IPC.ReplyCommand(id, "1");
					}
					*/
					else if (command == "kill")
					{
						if (int.TryParse(parameters["pid"], out int pid) == false)
							throw new Exception("Invalid pid.");

						// signal parameter not used in Windows

						Process process = Process.GetProcessById(pid);
						if(process != null)
						{
							process.Kill();
						}
					}
					else if (command == "process_openvpn")
					{
						string openvpnExecutablePath = parameters["path"];
						string configPath = parameters["config"];
						if (File.Exists(configPath) == false)
						{
							throw new Exception("Invalid config path");
						}

						string checkResult = CheckValidOpenVpnConfig(configPath);
						if(checkResult != "")
						{
							IPC.ReplyException(id, "Not supported OpenVPN config: " + checkResult);
						}
						else
						{
							Process process = new Process();
							process.StartInfo.FileName = openvpnExecutablePath;
							//process.StartInfo.Arguments = "--config \"" + configPath + "\"";
							process.StartInfo.Arguments = "\"" + configPath + "\"";
							process.StartInfo.WorkingDirectory = "";
							process.StartInfo.CreateNoWindow = true;
							process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
							process.StartInfo.UseShellExecute = false;
							process.StartInfo.RedirectStandardInput = true;
							process.StartInfo.RedirectStandardError = true;
							process.StartInfo.RedirectStandardOutput = true;
							process.StartInfo.StandardErrorEncoding = Encoding.UTF8; // 2.11.10
							process.StartInfo.StandardOutputEncoding = Encoding.UTF8; // 2.11.10

							process.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
							{
								if (e.Data != null)
									if (e.Data.Trim() != "")
										IPC.ReplyCommand(id, "stderr:" + e.Data + "\n");
							};
							process.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
							{
								if (e.Data != null)
									if (e.Data.Trim() != "")
										IPC.ReplyCommand(id, "stdout:" + e.Data + "\n");
							};

							process.Start();

							process.BeginOutputReadLine();
							process.BeginErrorReadLine();

							IPC.ReplyCommand(id, "procid:" + process.Id.ToString());

							process.WaitForExit();

							IPC.ReplyCommand(id, "return:" + process.ExitCode.ToString());
						}
					}
					/*
					else if (command == "shell") // Avoid
					{
						string stdout = "";
						string stderr = "";
						string path = parameters["path"];
						List<string> args = new List<string>();
						for (int a = 1; ; a++)
						{
							string kA = "arg" + a.ToString();
							if (parameters.ContainsKey(kA))
								args.Add(parameters[kA]);
							else
								break;
						}
						string autoWriteStdin = "";
						if (parameters.ContainsKey("write-stdin"))
							autoWriteStdin = parameters["write-stdin"];

						int exitCode = Utils.Shell(path, args.ToArray(), autoWriteStdin, out stdout, out stderr);

						if (stdout != "")
							ReplyCommand(id, "stdout:" + stdout);
						if (stderr != "")
							ReplyCommand(id, "stderr:" + stderr);
						ReplyCommand(id, "return:" + exitCode);
					}
					*/
					else if (command == "compatibility-remove-task")
					{
						// Remove old <2.17.3 task
						Utils.Shell("schtasks", "/delete /tn AirVPN /f");
						Utils.Shell("schtasks", "/delete /tn Eddie /f");						
					}
					else if (command == "compatibility-profiles")
					{						
						string dataPath = parameters["path-data"];

						if (Directory.Exists(dataPath) == false)
						{
							string appPath = parameters["path-app"];

							List<string> filesToMove = new List<string>();
							filesToMove.Add("AirVPN.xml");
							filesToMove.Add("default.xml");
							filesToMove.Add("Recovery.xml");
							filesToMove.Add("winfirewall_rules_original.wfw");
							filesToMove.Add("winfirewall_rules_backup.wfw");
							filesToMove.Add("winfirewallrules.wfw");
							filesToMove.Add("winfirewall_rules_original.airvpn");

							if (Directory.Exists(dataPath) == false)
								Directory.CreateDirectory(dataPath);

							if (appPath != dataPath)
							{
								// Old Eddie <2.17.3 save data in C:\Program Files\... . Move now.							
								foreach (string filename in filesToMove)
								{
									string fileOldPath = appPath + "\\" + filename;
									string fileNewPath = dataPath + "\\" + filename;
									if (File.Exists(fileOldPath))
									{
										File.Move(fileOldPath, fileNewPath);
									}
								}
							}

							string oldDataPath = dataPath.Substring(0, dataPath.LastIndexOf("\\")) + "\\AirVPN";
							if (Directory.Exists(oldDataPath))
							{
								// Old Eddie <2.17.3 save data in AirVPN folder... . Move now.								
								foreach (string filename in filesToMove)
								{
									string fileOldPath = oldDataPath + "\\" + filename;
									string fileNewPath = dataPath + "\\" + filename;
									if (File.Exists(fileOldPath))
									{
										File.Move(fileOldPath, fileNewPath);
									}
								}

								Directory.Delete(oldDataPath, true);
							}
						}	
					}
					else if (command == "wfp")
					{						
						string action = parameters["action"];
						if (action == "init")
						{
							string name = parameters["name"];
							NativeMethods.WfpInit(name);
							IPC.ReplyCommand(id, "1");
						}
						else if (action == "start")
						{
							string xml = parameters["xml"];

							if (NativeMethods.WfpStart(xml))
								IPC.ReplyCommand(id, "1");
							else
								IPC.ReplyCommand(id, "0");
						}
						else if (action == "stop")
						{
							NativeMethods.WfpStop();
						}
						else if (action == "rule-add")
						{							
							string xml = parameters["xml"];
							UInt64 ruleId = NativeMethods.WfpRuleAdd(xml);
							IPC.ReplyCommand(id, ruleId.ToString());
						}
						else if (action == "rule-remove")
						{
							UInt64 ruleId = 0;
							if (UInt64.TryParse(parameters["id"], out ruleId) == false)
								throw new Exception("Invalid id.");
							bool result = NativeMethods.WfpRuleRemove(ruleId);
							IPC.ReplyCommand(id, result ? "1" : "0");
						}
						else if (action == "pending-remove")
						{
							bool found = false;
							string wfpName = parameters["name"];
							string path = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".xml";

							using (System.Diagnostics.Process p = new System.Diagnostics.Process())
							{
								p.StartInfo.UseShellExecute = false;
								p.StartInfo.CreateNoWindow = true;
								p.StartInfo.RedirectStandardOutput = true;
								p.StartInfo.FileName = Utils.LocateExecutable("netsh.exe");
								p.StartInfo.Arguments = "WFP Show Filters file=\"" + path + "\"";
								p.StartInfo.WorkingDirectory = Path.GetTempPath();
								p.Start();
								p.StandardOutput.ReadToEnd();
								p.WaitForExit();
							}

							if (File.Exists(path))
							{
								System.Xml.XmlDocument xmlDoc = new XmlDocument();
								xmlDoc.Load(path);
								foreach (XmlElement xmlFilter in xmlDoc.DocumentElement.GetElementsByTagName("filters"))
								{
									foreach (XmlElement xmlItem in xmlFilter.GetElementsByTagName("item"))
									{
										foreach (XmlElement xmlName in xmlItem.SelectNodes("displayData/name"))
										{
											string name = xmlName.InnerText;
											if (name == wfpName)
											{
												foreach (XmlNode xmlFilterId in xmlItem.GetElementsByTagName("filterId"))
												{
													ulong idR;
													if (ulong.TryParse(xmlFilterId.InnerText, out idR))
													{
														NativeMethods.WfpRuleRemoveDirect(idR);
														found = true;
													}
												}
											}
										}
									}
								}

								File.Delete(path);
							}

							IPC.ReplyCommand(id, found ? "1" : "0");
						}
						else if (action == "last-error")
						{
							string msg = NativeMethods.WfpGetLastError();
							IPC.ReplyCommand(id, msg);
						}
						else
						{
							IPC.ReplyException(id, "Unknown action.");
						}
					}
					else if (command == "dns-flush")
					{
						bool full = (parameters["full"] == "1");

						if (full)
						{
							Utils.Shell(Utils.LocateExecutable("net.exe"), "stop dnscache");
							Utils.Shell(Utils.LocateExecutable("net.exe"), "start dnscache");
						}

						Utils.Shell(Utils.LocateExecutable("ipconfig.exe"), "/registerdns");
					}
					else if (command == "set-interface-metric")
					{
						string idx = parameters["idx"];
						string value = parameters["value"];
						string layer = ((parameters["layer"] == "ipv4") ? "ipv4" : "ipv6");
						NativeMethods.SetInterfaceMetric(Convert.ToInt32(idx), layer, Convert.ToInt32(value));
					}
					else if (command == "route")
					{
						bool accept = false;
						string result = "";
						if (parameters["layer"] == "ipv4")
						{
							string args = "";
							if (parameters["action"] == "add")
								args += "add ";
							else if (parameters["action"] == "remove")
								args += "delete ";
							else
								throw new Exception("Unknown action");

							args += Utils.EnsureStringIpAddress(parameters["address"]) + " mask " + Utils.EnsureStringIpAddress(parameters["mask"]) + " " + Utils.EnsureStringIpAddress(parameters["gateway"]);
							args += " if " + Utils.EnsureStringNumericInt(parameters["iface"]);
							/*
							Metric param are ignored or misinterpreted. http://serverfault.com/questions/238695/how-can-i-set-the-metric-of-a-manually-added-route-on-windows
							if(parameters.ContainsKey("metric"))
								cmd += " metric " + Utils.EnsureStringNumericInt(parameters["metric"]);
							*/
							result = Utils.Shell(Utils.LocateExecutable("route.exe"), args).Trim().Trim(new char[] { '!', '.' });
							if (result.ToLowerInvariant() == "ok")
								accept = true;
						}
						else if (parameters["layer"] == "ipv6")
						{
							string args = "interface ipv6";
							if (parameters["action"] == "add")
								args += " add";
							else if (parameters["action"] == "remove")
								args += " del";
							else
								throw new Exception("Unknown action");
							args += " route";
							args += " prefix=\"" + Utils.EnsureStringCidr(parameters["cidr"]) + "\"";
							args += " interface=\"" + Utils.EnsureStringNumericInt(parameters["iface"]) + "\"";
							args += " nexthop=\"" + Utils.EnsureStringIpAddress(parameters["gateway"]) + "\"";
							if (parameters.ContainsKey("metric"))
								args += " metric=" + Utils.EnsureStringNumericInt(parameters["metric"]);
							result = Utils.Shell(Utils.LocateExecutable("netsh.exe"), args).Trim().Trim(new char[] { '!', '.' });
							if (result.ToLowerInvariant() == "ok")
								accept = true;
							if (result.ToLowerInvariant().Contains("the system cannot find the file specified"))
								accept = true;
							if (result.ToLowerInvariant().Contains("element not found."))
								accept = true;
						}
						if (accept)
							IPC.ReplyCommand(id, "1");
						else
							throw new Exception(result);
					}
					else if (command == "netsh") // TOFIX - Too much open
					{
						string args = parameters["args"];
						string result = Utils.Shell(Utils.LocateExecutable("netsh.exe"), args.Trim());
					}
					else if (command == "windows-firewall")
					{
						string args = parameters["args"];
						string result = Utils.Shell(Utils.LocateExecutable("netsh.exe"), "advfirewall " + args.Trim());
					}
					else if (command == "windows-workaround-25139")
					{
						string cidr = parameters["cidr"];
						string iface = parameters["iface"];
						string result = Utils.Shell(Utils.LocateExecutable("netsh.exe"), "interface ipv6 del route \"" + Utils.EnsureStringCidr(cidr) + "\" interface=\"" + Utils.EnsureStringNumericInt(iface) + "\"");
					}
					else if (command == "windows-workaround-interface-up")
					{
						string name = parameters["name"];

						string result = Utils.Shell(Utils.LocateExecutable("netsh.exe"), "interface set interface \"" + Utils.EscapeStringForInsideQuote(name) + "\" ENABLED");
					}
					else
					{
						IPC.ReplyException(id, "Unknown command.");
					}
				}
				catch (Exception ex)
				{
					IPC.ReplyException(id, ex.Message);
				}

				IPC.EndCommand(id);

				if (command == "exit")
				{
					// Note: don't wait for pending thread.
					// IPC.ClientDisconnectSignal.Set();
					IPC.Close("Disconnect");
				}
			});
		}

		private string CheckValidOpenVpnConfig(string path)
		{
			if (File.Exists(path) == false)
				return "file not found";

			string body = File.ReadAllText(path);
			string[] lines = body.Split('\n');
			foreach(string line in lines)
			{
				string lineNormalized = line.ToLowerInvariant().Trim();

				if (lineNormalized.StartsWith("#"))
					continue;
									
				bool lineAllowed = true;

				// The below list directive can be incompleted with newest OpenVPN version, 
				// but any new dangerous directive is expected to be blocked by script-security
				// Note: parser can be better (any <key> lines are treated as directive, but never match for the space)
				// But this validation is only for additional security, Eddie already parse and prune with a better parse the config
				if (lineNormalized.StartsWith("script-security ")) lineAllowed = false;
				if (lineNormalized.StartsWith("plugin ")) lineAllowed = false;
				if (lineNormalized.StartsWith("up ")) lineAllowed = false;
				if (lineNormalized.StartsWith("down ")) lineAllowed = false;
				if (lineNormalized.StartsWith("client-connect ")) lineAllowed = false;
				if (lineNormalized.StartsWith("client-disconnect ")) lineAllowed = false;
				if (lineNormalized.StartsWith("learn-address ")) lineAllowed = false;
				if (lineNormalized.StartsWith("auth-user-pass-verify ")) lineAllowed = false;
				if (lineNormalized.StartsWith("tls-verify ")) lineAllowed = false;
				if (lineNormalized.StartsWith("ipchange ")) lineAllowed = false;
				if (lineNormalized.StartsWith("iproute ")) lineAllowed = false;
				if (lineNormalized.StartsWith("route-up ")) lineAllowed = false;
				if (lineNormalized.StartsWith("route-pre-down ")) lineAllowed = false;

				if (lineAllowed == false)
					return "directive '" + lineNormalized + "' not allowed";
			}

			return "";
		}
	}
}
