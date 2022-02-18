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
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace Eddie.Core.ConnectionTypes
{
	public class OpenVPN : IConnectionType
	{
		protected TemporaryFile m_fileAuthProxy;
		protected TemporaryFile m_fileAuthPassword;
		protected TemporaryFile m_fileConfig;

		protected ConfigBuilder.OpenVPN m_configStartup;
		protected ConfigBuilder.OpenVPN m_configWithPush;
		protected Elevated.Command m_elevatedCommand;

		protected List<string> m_pendingPushDetected = new List<string>();
		protected IpAddresses m_dns = new IpAddresses();

		protected Int64 m_transportLastSignalTime = 0;
		protected string m_transportLastSignalType = "none";
		protected Int64 m_vpnLastSignalTime = 0;
		protected string m_vpnLastSignalType = "none";
		protected Process m_processTransport = null;
		protected TemporaryFile m_fileSshKey;
		protected TemporaryFile m_fileSslCrt;
		protected TemporaryFile m_fileSslConfig;
		protected string m_networkLockSoftwareExceptionPath = "";

		public string Transport = "";
		public int TransportSshLocalPort = 0;
		public int TransportSshPortDestination = 0;
		public int TransportSslLocalPort = 0;

		private bool m_deleteAfterStart = false;

		private StringWriterLine m_stdout = new StringWriterLine();
		private StringWriterLine m_stderr = new StringWriterLine();

		public override string GetTypeName()
		{
			return "OpenVPN";
		}
		public override void Build()
		{
			base.Build();

			Options options = Engine.Instance.Options;

			m_configStartup = GenerateConfigBuilder();

			m_configStartup.AppendDirective("setenv", "IV_GUI_VER " + Constants.Name + Constants.VersionDesc, "Client level");

			if (options.GetBool("openvpn.skip_defaults") == false)
			{
				m_configStartup.AppendDirectives(Engine.Instance.Options.Get("openvpn.directives"), "Client level");
				string directivesPath = Engine.Instance.Options.Get("openvpn.directives.path");
				if (directivesPath.Trim() != "")
				{
					try
					{
						if (Platform.Instance.FileExists(directivesPath))
						{
							string text = Platform.Instance.FileContentsReadText(directivesPath);
							m_configStartup.AppendDirectives(text, "Client level");
						}
						else
						{
							Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("FileNotFound", directivesPath));
						}
					}
					catch (Exception ex)
					{
						Engine.Instance.Logs.Log(LogType.Warning, LanguageManager.GetText("FileErrorRead", directivesPath, ex.Message));
					}
				}
				Info.Provider.OnBuildOvpnDefaults(m_configStartup);

				m_configStartup.AppendDirectives(Info.OvpnDirectives, "Server level");
			}

			if (Info.Path != "")
			{
				if (Platform.Instance.FileExists(Info.Path))
				{
					string text = Platform.Instance.FileContentsReadText(Info.Path);
					m_configStartup.AppendDirectives(text, "Config file");

					string dirPath = Platform.Instance.FileGetDirectoryPath(Info.Path);
					m_configStartup.NormalizeRelativePath(dirPath);
				}
			}

			if (options.Get("openvpn.dev_node") != "")
				m_configStartup.AppendDirective("dev-node", options.Get("openvpn.dev_node"), "");

			// Find bind IP
			string bindIP = "";
			if (options.Get("network.entry.iface") != "")
			{
				if (IsPreviewMode() == false) // Only to avoid NetworkInterface.GetAllNetworkInterfaces
				{
					string v = options.Get("network.entry.iface");

					NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
					foreach (NetworkInterface adapter in interfaces)
					{
						IpAddresses ips = new IpAddresses();
						try
						{
							foreach (UnicastIPAddressInformation ip in adapter.GetIPProperties().UnicastAddresses)
							{
								IpAddress ip2 = new IpAddress(ip.Address.ToString());
								if (ip2.Valid)
									ips.Add(ip2);
							}
						}
						catch
						{
						}

						if (options.Get("network.entry.iplayer") == "ipv4-only")
							ips = ips.OnlyIPv4;
						else if (options.Get("network.entry.iplayer") == "ipv6-only")
							ips = ips.OnlyIPv6;

						if ((IpAddress.IsIP(v)) && (ips.Contains(new IpAddress(v))))
						{
							bindIP = v;
							break;
						}
						else if (adapter.Id == v)
						{
							if ((options.Get("network.entry.iplayer") == "ipv4-ipv6") && (ips.CountIPv4 > 0))
								ips = ips.OnlyIPv4;
							else if ((options.Get("network.entry.iplayer") == "ipv6-ipv4") && (ips.CountIPv6 > 0))
								ips = ips.OnlyIPv6;
							if (ips.Count > 0)
							{
								bindIP = ips.First.ToString();
								break;
							}
						}
					}
				}
			}

			if (bindIP != "")
			{
				m_configStartup.AppendDirective("local", bindIP, "");
				m_configStartup.RemoveDirective("nobind");
			}
			else
			{
				m_configStartup.RemoveDirective("local");
				m_configStartup.AppendDirective("nobind", "", "");
			}

			int rcvbuf = options.GetInt("openvpn.rcvbuf");
			if (rcvbuf == -2) rcvbuf = Platform.Instance.GetOpenVpnRecommendedRcvBufDirective();
			if (rcvbuf == -2) rcvbuf = -1;
			if (rcvbuf != -1)
				m_configStartup.AppendDirective("rcvbuf", rcvbuf.ToString(), "");

			int sndbuf = options.GetInt("openvpn.sndbuf");
			if (sndbuf == -2) sndbuf = Platform.Instance.GetOpenVpnRecommendedSndBufDirective();
			if (sndbuf == -2) sndbuf = -1;
			if (sndbuf != -1)
				m_configStartup.AppendDirective("sndbuf", sndbuf.ToString(), "");

			string proxyDirectiveName = "";
			string proxyDirectiveArgs = "";

			string proxyMode = options.GetLower("proxy.mode");
			string proxyWhen = options.GetLower("proxy.when");
			if ((proxyWhen == "none") || (proxyWhen == "web"))
				proxyMode = "none";
			if (proxyMode == "tor")
			{
				proxyDirectiveName = "socks-proxy";
			}
			else if (proxyMode == "http")
			{
				proxyDirectiveName = "http-proxy";

			}
			else if (proxyMode == "socks")
			{
				proxyDirectiveName = "socks-proxy";
			}

			if (proxyDirectiveName != "")
			{
				proxyDirectiveArgs += options.Get("proxy.host") + " " + options.Get("proxy.port");

				if ((options.GetLower("proxy.mode") != "none") && (options.GetLower("proxy.mode") != "tor"))
				{
					if (options.Get("proxy.auth") != "None")
					{
						string fileNameAuthOvpn = "";
						if (IsPreviewMode())
						{
							fileNameAuthOvpn = "dummy.ppw";
						}
						else
						{
							m_fileAuthProxy = new TemporaryFile("ppw");
							fileNameAuthOvpn = m_fileAuthProxy.Path;
							string fileNameData = options.Get("proxy.login") + "\n" + options.Get("proxy.password") + "\n";
							Platform.Instance.FileContentsWriteText(m_fileAuthProxy.Path, fileNameData, Encoding.Default);
							Platform.Instance.FileEnsurePermission(m_fileAuthProxy.Path, "600");
						}
						proxyDirectiveArgs += " " + m_configStartup.EncodePath(fileNameAuthOvpn) + " " + options.Get("proxy.auth").ToLowerInvariant(); // 2.6 Auth Fix
					}
				}

				m_configStartup.AppendDirective(proxyDirectiveName, proxyDirectiveArgs, "");
			}

			SetupLayers();

			if (Engine.Instance.GetOpenVpnTool().VersionAboveOrEqual("2.4"))
			{
				m_configStartup.RemoveDirective("redirect-gateway"); // Remove if exists
				m_configStartup.AppendDirective("pull-filter", "ignore \"redirect-gateway\"", "Routes managed by Eddie");

				if (ConfigIPv6 == false)
				{
					m_configStartup.AppendDirective("pull-filter", "ignore \"dhcp-option DNS6\"", "Client side");
					m_configStartup.AppendDirective("pull-filter", "ignore \"tun-ipv6\"", "Client side");
					m_configStartup.AppendDirective("pull-filter", "ignore \"ifconfig-ipv6\"", "Client side");
				}

				if (Platform.Instance.GetUseOpenVpnRoutes())
				{
					SkipRouteAll = true; // Route All managed by OpenVPN

					if ((RouteAllIPv4 == false) && (RouteAllIPv6 == false))
					{
						// no redirect-gateway
					}
					else if ((RouteAllIPv4 == true) && (RouteAllIPv6 == false))
					{
						m_configStartup.AppendDirective("redirect-gateway", "def1 bypass-dhcp", "");

					}
					else if ((RouteAllIPv4 == false) && (RouteAllIPv6 == true))
					{
						m_configStartup.AppendDirective("redirect-gateway", "ipv6 !ipv4 def1 bypass-dhcp", "");
					}
					else
					{
						m_configStartup.AppendDirective("redirect-gateway", "ipv6 def1 bypass-dhcp", "");
					}
				}

				// If custom DNS provided, add to config
				IpAddresses dnsCustom = new IpAddresses(Engine.Instance.Options.Get("dns.servers"));
				foreach (IpAddress dnsIP in dnsCustom.IPs)
				{
					if ((dnsIP.IsV4) && (ConfigIPv4))
						m_configStartup.AppendDirectives("dhcp-option DNS " + dnsIP.ToString(), "");
					else if ((dnsIP.IsV6) && (ConfigIPv6))
						m_configStartup.AppendDirectives("dhcp-option DNS6 " + dnsIP.ToString(), "");
				}

				// If config contain dhcp-options about DNS, ignore what received via push
				if (m_configStartup.ExtractDns().Count > 0)
				{
					m_configStartup.AppendDirectives("pull-filter ignore \"dhcp-option DNS\"", "Forced client side");
					m_configStartup.AppendDirectives("pull-filter ignore \"dhcp-option DNS6\"", "Forced client side");
				}

				if (Engine.Instance.Options.GetBool("dns.delegate") == false)
				{
					if (dnsCustom.Count == 0) // Otherwise already added
					{
						m_configStartup.AppendDirectives("pull-filter ignore \"dhcp-option DNS\"", "DNS managed by Eddie");
						m_configStartup.AppendDirectives("pull-filter ignore \"dhcp-option DNS6\"", "DNS managed by Eddie");
					}

					m_dns.Add(m_configStartup.ExtractDns());

					m_configStartup.RemoveDirectiveValueStart("dhcp-option", "DNS ");
					m_configStartup.RemoveDirectiveValueStart("dhcp-option", "DNS6 ");
				}
			}
			else
			{
				// OpenVPN <2.4, IPv6 not supported, IPv4 required.
				ConfigIPv4 = true;
				ConfigIPv6 = false;

				if (RouteAllIPv4)
				{
					if (Platform.Instance.GetUseOpenVpnRoutes())
					{
						m_configStartup.AppendDirective("redirect-gateway", "def1 bypass-dhcp", "");
					}
				}
				else
				{
					m_configStartup.AppendDirective("route-nopull", "", "For Routes Out");

					// 2.9, this is used by Linux resolv-conf DNS method. Need because route-nopull also filter pushed dhcp-option.
					// Incorrect with other provider, but the right-approach (pull-filter based) require OpenVPN >2.4.
					m_configStartup.AppendDirective("dhcp-option", "DNS " + Constants.DnsVpn, "");
				}
			}

			Info.Provider.OnBuildConnection(this);

			m_configStartup.AppendDirectives(Engine.Instance.Options.Get("openvpn.custom"), "Custom level");

			foreach (ConnectionRoute route in Routes)
			{
				// We never find a better method to manage IPv6 route via OpenVPN, at least <2.4.4.                
				// Eddie manage this kind of routes by itself. Anyway, this is need by WireGuard.
				//m_configStartup.AppendDirective("route", route.Address.ToOpenVPN() + " " + route.Gateway, route.Notes.Safe());
			}

			m_configStartup.Adaptation();
		}

		public override void OnInit()
		{
			base.OnInit();

			m_configWithPush = m_configStartup.Clone();
		}

		public override void OnStart()
		{
			if (Transport == "SSH")
			{
				TransportStartProcessSSH();
			}
			else if (Transport == "SSL")
			{
				TransportStartProcessSSL();
			}
			else
			{
				VpnStartProcess();
			}
		}

		public override void OnCleanAfterStart()
		{
			base.OnCleanAfterStart();

			if (m_fileConfig != null)
			{
				m_fileConfig.Close();
				m_fileConfig = null;
			}

			if (m_fileAuthProxy != null)
			{
				m_fileAuthProxy.Close();
				m_fileAuthProxy = null;
			}
		}

		public override bool OnWaitingConnection() // Return true if waiting need to end
		{
			bool result = false;

			if ((m_elevatedCommand != null) && (m_elevatedCommand.IsComplete)) // 2.2
			{
				Session.SetReset("ERROR");
				result = true;
			}
			if ((m_processTransport != null) && (m_processTransport.ReallyExited)) // 2.2
			{
				Session.SetReset("ERROR");
				result = true;
			}

			return result;
		}

		public override bool OnWaitingDisconnection() // Return true if waiting need to end
		{
			// Main VPN process                                    
			if ((m_elevatedCommand != null) && (m_elevatedCommand.IsComplete == false))
			{
				string tool = "openvpn";
				if (this is Hummingbird)
					tool = "hummingbird";

				if (Utils.UnixTimeStamp() - m_vpnLastSignalTime >= 20)
				{
					if (m_vpnLastSignalType == "none")
					{
						m_vpnLastSignalTime = Utils.UnixTimeStamp();
						m_vpnLastSignalType = "soft";
						Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithSoft"));

						Engine.Instance.Elevated.DoCommandSync(tool, "action", "stop", "id", Id, "signal", "sigint");
					}
					else
					{
						m_vpnLastSignalTime = Utils.UnixTimeStamp();
						m_vpnLastSignalType = "hard";
						Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithHard"));

						Engine.Instance.Elevated.DoCommandSync(tool, "action", "stop", "id", Id, "signal", "sigterm");
					}
				}
			}

			// Transport (SSH/SSL) process
			if ((m_processTransport != null) && (m_elevatedCommand != null) && (m_processTransport.ReallyExited == false) && (m_elevatedCommand.IsComplete == true))
			{
				if (Utils.UnixTimeStamp() - m_transportLastSignalTime >= 10)
				{
					if (m_transportLastSignalType == "none")
					{
						m_transportLastSignalTime = Utils.UnixTimeStamp();
						m_transportLastSignalType = "soft";

						if (Platform.Instance.ProcessKillSoft(m_processTransport))
							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithSoft"));
						else
						{
							m_transportLastSignalType = "hard";
							m_processTransport.Kill();
							Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithHard"));
						}
					}
					else
					{
						m_transportLastSignalTime = Utils.UnixTimeStamp();
						m_transportLastSignalType = "hard";
						Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("KillWithHard"));
						m_processTransport.Kill();
					}
				}
			}

			bool result = true;

			if ((m_processTransport != null) && (m_processTransport.ReallyExited == false))
				result = false;

			if ((m_elevatedCommand != null) && (m_elevatedCommand.IsComplete == false))
				result = false;

			return result;
		}

		public override void OnClose()
		{
			base.OnClose();

			if (m_elevatedCommand != null)
			{
				//m_elevatedCommand.Abort();
				m_elevatedCommand = null;
			}

			if (m_processTransport != null)
			{
				m_processTransport.Dispose();
				m_processTransport = null;
			}

			if (m_networkLockSoftwareExceptionPath != "")
			{
				if (Engine.Instance.NetworkLockManager != null)
					Engine.Instance.NetworkLockManager.DeallowProgram(m_networkLockSoftwareExceptionPath);
			}

			// Closing temporary files
			if (m_fileSshKey != null)
			{
				m_fileSshKey.Close();
				m_fileSshKey = null;
			}
			if (m_fileSslCrt != null)
			{
				m_fileSslCrt.Close();
				m_fileSslCrt = null;
			}
			if (m_fileSslConfig != null)
			{
				m_fileSslConfig.Close();
				m_fileSslConfig = null;
			}

			// Here because reneg keys require it, and we can't know when OpenVPN need it.
			if (m_fileAuthPassword != null)
			{
				m_fileAuthPassword.Close();
				m_fileAuthPassword = null;
			}
		}

		public override void OnLogEvent(string source, string message)
		{
			string messageLower = message.ToLowerInvariant(); // Try to match lower/insensitive case when possible.
			RegexOptions regexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline;

			if (source == "Tunnel")
			{
				bool log = true;
				LogType logType = LogType.Verbose;

				if (m_fileConfig != null)
				{
					// First feedback from OpenVPN process. We can remove temporary files.
					OnCleanAfterStart();
				}

				// Ignore
				if (message.IndexOfInv("MANAGEMENT: CMD 'status'") != -1)
					return;

				// Level
				if (messageLower.StartsWithInv("warning:"))
					logType = LogType.Warning;
				if (messageLower.StartsWithInv("warn:"))
					logType = LogType.Warning;
				if (messageLower.StartsWithInv("error:"))
					logType = LogType.Error;
				if (messageLower.StartsWithInv("fatal:"))
					logType = LogType.Error;
				if (messageLower.StartsWithInv("options error:"))
					logType = LogType.Warning;

				// Exception
				if (Platform.Instance.GetCode() != "MacOS")
				{
					// Unresolved issue, but don't want to warn users.
					// Under OpenVPN provider, file hash.tmp.ppw for auth, any permission flags cause denied or this warning. // TOFIX
					if ((messageLower.StartsWithInv("warning:")) && (messageLower.Contains("is group or others accessible")))
						logType = LogType.Verbose;
				}

				if (Platform.Instance.GetCode() == "MacOS")
				{
					if (message.Contains("ioctl"))
						logType = LogType.Verbose;
					// OpenVPN 2.5.1 when restore from sleep mode, throw this, but connect anyway
					if (message.Contains("ifconfig: ioctl (SIOCDIFADDR): Can't assign requested address"))
						logType = LogType.Verbose;
					// OpenVPN 2.5.1 when restore from sleep mode, throw this, but connect anyway
					if (message.Contains("route: writing to routing socket: File exists"))
						logType = LogType.Verbose;
				}

				if (Engine.Instance.GetOpenVpnTool().VersionAboveOrEqual("2.4") == false)
				{
					// Don't warning users for correct behiavour (outside tunnel)
					if (
						(message.IndexOfInv("Options error: option 'redirect-gateway' cannot be used in this context ([PUSH-OPTIONS])") != -1) ||
						(message.IndexOfInv("Options error: option 'dhcp-option' cannot be used in this context ([PUSH-OPTIONS])") != -1)
						)
						log = false;
				}

				// Useless
				if (message.Contains("Use --help for more information."))
					log = false;

				// Ignore, caused by Windows method GenerateConsoleCtrlEvent to soft-kill
				if (message.Contains("win_trigger_event: WriteConsoleInput: The handle is invalid."))
					log = false;

				// Eddie delete any ovpn/crt/key/pwd files after the reading by OpenVPN.
				// If a connection drop, it's not OpenVPN that need to retry the connection, must performed by Eddie.
				// But unfortunately there isn't any method in OpenVPN to avoid the behiavour of SIGHUP (process restarting).
				// Neither the directive 'single-session'.
				// Anyway, when happen, OpenVPN can't find the .ovpn config, so effectively stop and Eddie will perform the retry.
				if ((message.StartsWithInv("Options error:")) && (message.Contains("Error opening configuration file")))
				{
					log = false;
				}

				// Under Windows, kind of errors when i programmatically send CTRL+C. Ignore it.
				if (message.Contains("win_trigger_event: WriteConsoleInput"))
					log = false;

				// If ncp-disable is used, this warnings are useless.
				if ((message.Contains("WARNING: 'cipher' is used inconsistently")) && (m_configWithPush.ExistsDirective("ncp-disable")))
					log = false;
				if ((message.Contains("WARNING: 'keysize' is used inconsistently")) && (m_configWithPush.ExistsDirective("ncp-disable")))
					log = false;

				// OpenVPN <2.4 don't have ncp-disable, and throw the same warnings if cipher specified client-side it's different from the server-side.
				if ((message.Contains("WARNING: 'cipher' is used inconsistently")) && (Engine.Instance.GetOpenVpnTool().VersionUnder("2.4")))
					log = false;
				if ((message.Contains("WARNING: 'keysize' is used inconsistently")) && (Engine.Instance.GetOpenVpnTool().VersionUnder("2.4")))
					log = false;

				if (message.StartsWithInv("Options error:"))
				{
					if (log)
					{
						List<string> matches = message.RegExMatchSingle("Options error\\: Unrecognized option or missing parameter\\(s\\) in (.*?)\\:\\d+\\:(.*?)\\(.*\\)");
						if ((matches != null) && (matches.Count == 2))
						{
							string context = matches[0].Trim();
							string unrecognizedOption = matches[1].Trim();

							if (context != "[PUSH-OPTIONS]")
							{
								Engine.Instance.Logs.Log(LogType.Fatal, LanguageManager.GetText("DirectiveError", unrecognizedOption));
								DumpConfigToLog();
								Session.SetReset("FATAL");
							}
						}
					}
				}

				// Forse qui
				if (log)
					Engine.Instance.Logs.Log(logType, "OpenVPN > " + message);

				// Actions - Remember, dump log and after actions

				if (message.StartsWithInv("Data Channel: "))
				{
					string c = message.Substring("Data Channel: ".Length);
					string c2 = c.RegExMatchOne("using negotiated cipher '(.+?)'");
					if (c2 != "")
						c = c2;
					DataChannel = c;
				}

				if (message.StartsWithInv("Control Channel: "))
				{
					ControlChannel = message.Substring("Control Channel: ".Length);
				}

				if (message.IndexOfInv("Connection reset, restarting") != -1)
				{
					Session.SetReset("ERROR");
				}

				if (message.IndexOfInv("Exiting due to fatal error") != -1)
				{
					Session.SetReset("ERROR");
				}

				if (message.IndexOfInv("SIGTERM[soft,ping-exit]") != -1) // 2.2
				{
					Session.SetReset("ERROR");
				}

				if (message.IndexOfInv("SIGUSR1[soft,tls-error] received, process restarting") != -1)
				{
					Session.SetReset("ERROR");
				}

				if (message.IndexOfInv("SIGUSR1[soft,tls-error] received, process restarting") != -1)
				{
					Session.SetReset("ERROR");
				}

				Match matchSigReceived = Regex.Match(message, "SIG(.*?)\\[(.*?),(.*?)\\] received", regexOptions);
				if (matchSigReceived.Success)
				{
					Session.SetReset("ERROR");
				}

				if (message.IndexOfInv("AUTH_FAILED") != -1)
				{
					Engine.Instance.CurrentServer.Provider.OnAuthFailed();

					Session.SetReset("AUTH_FAILED");
				}

				if (message.IndexOfInv("MANAGEMENT: TCP Socket listening on") != -1)
				{
				}

				if (message.IndexOfInv("TLS: tls_process: killed expiring key") != -1)
				{
					Engine.Instance.Logs.Log(LogType.Verbose, LanguageManager.GetText("RenewingTls"));
				}

				if (message.IndexOfInv("Initialization Sequence Completed With Errors") != -1)
				{
					Session.SetReset("ERROR");
				}

				// Detect connection (OpenVPN >2.4)
				// TOFIX - Check if need anymore
				if (messageLower.RegExMatchOne("peer connection initiated with \\[af_inet6?\\]([0-9a-f\\.\\:]+?):(\\d+?)") != "")
				{
					if ((Transport != "SSH") && (Transport != "SSL")) // Avoid localhost
					{
						string t = messageLower;
						t = t.Replace("[nonblock]", "").Trim();
						List<string> fields = t.RegExMatchSingle("\\[af_inet6?\\]([a-z90-9\\.\\:]+?):(\\d+?)$");
						if ((fields != null) && (fields.Count == 2))
						{
							EntryIP = fields[0];
							EntryPort = Conversions.ToInt32(fields[1]);
						}
					}
				}

				// Detect TCP connection (OpenVPN <2.4)
				// TOFIX - Check if need anymore
				if (message.IndexOfInv("Attempting to establish TCP connection with [AF_INET]") != -1)
				{
					if ((Transport != "SSH") && (Transport != "SSL")) // Avoid localhost
					{
						string t = message;
						t = t.Replace("Attempting to establish TCP connection with", "").Trim();
						t = t.Replace("[nonblock]", "").Trim();
						t = t.Replace("[AF_INET]", "").Trim();
						string[] parts = t.Split(':');
						if (parts.Length == 2)
						{
							EntryIP = parts[0];
							EntryPort = Convert.ToInt32(parts[1]);
						}
					}
				}

				// Detect UDP connection (OpenVPN <2.4)
				// TOFIX - Check if need anymore
				if (message.IndexOfInv("UDPv4 link remote: [AF_INET]") != -1)
				{
					string t = message;
					t = t.Replace("UDPv4 link remote:", "").Trim();
					t = t.Replace("[AF_INET]", "").Trim();
					string[] parts = t.Split(':');
					if (parts.Length == 2)
					{
						EntryIP = parts[0];
						EntryPort = Convert.ToInt32(parts[1]);
					}
				}

				if (message.IndexOfInv("Initialization Sequence Completed") != -1)
				{
					Session.ConnectedStep();
				}

				// Windows
				if (Platform.Instance.IsUnixSystem() == false)
				{
					// Note: Windows allow [] chars in interface name, but OpenVPN use ] to close the name and don't escape it, so "\\sopened" it's required for lazy regex.

					// TAP-WIN32 - OpenVPN 2.5
					{
						Match match = Regex.Match(message, "TAP-WIN32 device \\[(.*?)\\] opened", regexOptions);
						if (match.Success)
							SearchTunNetworkInterfaceByName(match.Groups[1].Value); // Note: Name, not ID.
					}

					// tap-windows6 - OpenVPN 2.5
					{
						Match match = Regex.Match(message, "tap-windows6 device \\[(.*?)\\] opened", regexOptions);
						if (match.Success)
							SearchTunNetworkInterfaceByName(match.Groups[1].Value); // Note: Name, not ID.
					}

					// Wintap - OpenVPN 2.5
					{
						Match match = Regex.Match(message, "Wintun device \\[(.*?)\\] opened", regexOptions);
						if (match.Success)
							SearchTunNetworkInterfaceByName(match.Groups[1].Value); // Note: Name, not ID.
					}

					// Compatibility, can be probably removed. Ininfluent if rule above already match.
					{
						List<string> matchInterface = message.RegExMatchSingle("TAP-.*\\\\(.+?).tap");
						if (matchInterface != null)
							SearchTunNetworkInterfaceByName(matchInterface[0]);
					}

					// Compatibility, can be probably removed. Ininfluent if rule above already match.
					{
						List<string> matchName = message.RegExMatchSingle("TAP-.*\\sdevice\\s\\[(.*?)\\]\\sopened");
						if (matchName != null)
							SearchTunNetworkInterfaceByName(matchName[0]);
					}
				}

				// Unix
				if (Platform.Instance.IsUnixSystem())
				{
					Match match = Regex.Match(message, "TUN/TAP device (.*?) opened", regexOptions);
					if (match.Success)
						SearchTunNetworkInterfaceByName(match.Groups[1].Value);
				}

				// OSX
				{
					Match match = Regex.Match(message, "Opened utun device (.*?)$", regexOptions);
					if (match.Success)
						SearchTunNetworkInterfaceByName(match.Groups[1].Value);
				}

				if (Platform.Instance.IsWindowsSystem())
				{
					// Workaround (2018/01/28) for this bug: https://airvpn.org/topic/25139-why-exists-pull-filter-ignore-dhcp-option-dns6/
					if (message.IndexOfInv("metric 0") != -1) // To catch only one, the main
					{
						IpAddress ipv6rangeIp = new IpAddress(message.RegExMatchOne("add_route_ipv6\\((.*?)\\s"));
						if (ipv6rangeIp.Valid)
						{
							string routeIPv6 = SystemExec.Exec2(Platform.Instance.LocateExecutable("route.exe"), "-6", "PRINT");
							string iFace = routeIPv6.CleanSpace().ToLowerInv().RegExMatchOne("(\\d+?)\\s\\d+\\s" + ipv6rangeIp.ToCIDR() + "\\son-link");
							if (iFace != "")
							{
								Engine.Instance.Logs.LogVerbose("Detected an OpenVPN bug (On-Link route on VPN range), autofix.");
								Engine.Instance.Elevated.DoCommandSync("windows-workaround-25139", "cidr", ipv6rangeIp.ToCIDR(), "iface", iFace);
							}
						}
					}
				}

				// Push directives management                
				{
					bool pushLog = false;
					string pushDirectivesLine = message.RegExMatchOne("PUSH: Received control message: '(.*?)'");
					if (pushDirectivesLine != "")
					{
						pushLog = true;
						List<string> pushDirectives = pushDirectivesLine.StringToList(",");
						foreach (string directive in pushDirectives)
						{
							if (directive == "PUSH_REPLY")
								continue;
							m_pendingPushDetected.Add(directive);

							if (directive.ToLowerInvariant().StartsWithInv("dhcp-option dns"))
							{
								if (Engine.Instance.Options.GetBool("dns.delegate") == false)
								{
									IpAddresses dnsCustom = new IpAddresses(Engine.Instance.Options.Get("dns.servers"));
									if (dnsCustom.Count == 0)
									{
										if (directive.ToLowerInvariant().StartsWithInv("dhcp-option dns "))
											m_dns.Add(directive.Substring("dhcp-option dns ".Length));
										else if (directive.ToLowerInvariant().StartsWithInv("dhcp-option dns6 "))
											m_dns.Add(directive.Substring("dhcp-option dns6 ".Length));
									}
								}
							}
						}
					}

					string pushDirectivesFiltered = message.RegExMatchOne("Pushed option removed by filter: '(.*?)'");
					if (pushDirectivesFiltered != "")
					{
						pushLog = true;
						m_pendingPushDetected.Remove(pushDirectivesFiltered);
					}

					// OpenVPN2 log "PUSH: Received control message:" and after a series of "Pushed option removed by filter:".
					// So, if the series of "Pushed option removed by filter:" ends, accept the push list as definitive.
					if ((pushLog == false) && (m_pendingPushDetected.Count != 0))
					{
						foreach (string directive in m_pendingPushDetected)
							m_configWithPush.AppendDirectives(directive, "Push");
						m_pendingPushDetected.Clear();
					}
				}
			}
			else if (source == "SSH")
			{
				bool log = true;

				// Windows PuTTY
				if (message.IndexOfInv("enter \"y\" to update PuTTY's cache and continue connecting") != -1)
					TransportSendYes();

				// Linux/MacOS SSH - Not sure if used really
				if (message.IndexOfInv("If you trust this host, enter \"y\" to add the key to") != -1)
					TransportSendYes();

				if (message == "Access granted") // PLink Windows
				{
					VpnStartProcess();
				}

				if (message.StartsWithInv("Authenticated to")) // SSH Linux
				{
					VpnStartProcess();
				}

				if (log)
					Engine.Instance.Logs.Log(LogType.Verbose, source + " > " + message);
			}
			else if (source == "SSL")
			{
				bool log = true;

				if (message.IndexOfInv("Configuration successful") != -1)
				{
					VpnStartProcess();
				}

				if (log)
					Engine.Instance.Logs.Log(LogType.Verbose, source + " > " + message);
			}
			else
			{
				base.OnLogEvent(source, message);
			}
		}

		public override bool NeedCredentials()
		{
			if (m_configStartup.ExistsDirective("auth-user-pass"))
			{
				if (m_configStartup.GetOneDirectiveText("auth-user-pass") == "") // If empty
				{
					m_configStartup.RemoveDirective("auth-user-pass");
					return true;
				}
			}

			return base.NeedCredentials();
		}

		public override void SetCredentialsUserPass(string username, string password)
		{
			base.SetCredentialsUserPass(username, password);

			m_configStartup.RemoveDirective("auth-user-pass");

			if (m_fileAuthPassword != null)
			{
				m_fileAuthPassword.Close();
				m_fileAuthPassword = null;
			}

			m_fileAuthPassword = new TemporaryFile("ppw");
			string fileNameAuthOvpn = m_fileAuthPassword.Path;
			string fileNameData = username + "\n" + password + "\n";

			Platform.Instance.FileContentsWriteText(fileNameAuthOvpn, fileNameData, Encoding.Default); // TOFIX: Check if OpenVPN expect UTF-8
			Platform.Instance.FileEnsurePermission(fileNameAuthOvpn, "600");

			m_configStartup.AppendDirective("auth-user-pass", m_configStartup.EncodePath(fileNameAuthOvpn), "Auth");
		}

		public override void CheckForWarnings()
		{
			base.CheckForWarnings();

			if (m_configStartup != null)
			{
				if ((m_configStartup.ExistsDirective("<tls-crypt>")) && (Engine.Instance.GetOpenVpnTool().VersionUnder("2.4")))
					Info.WarningAdd(LanguageManager.GetText("ConnectionWarningOlderOpenVpnTlsCrypt"), ConnectionInfoWarning.WarningType.Error);
				if ((m_configStartup.ExistsDirective("tls-crypt")) && (Engine.Instance.GetOpenVpnTool().VersionUnder("2.4")))
					Info.WarningAdd(LanguageManager.GetText("ConnectionWarningOlderOpenVpnTlsCrypt"), ConnectionInfoWarning.WarningType.Error);
			}
		}

		public override string AdaptMessage(string str)
		{
			str = base.AdaptMessage(str);

			// Remove STunnel timestamp
			str = System.Text.RegularExpressions.Regex.Replace(str, "^\\d{4}\\.\\d{2}\\.\\d{2}\\s+\\d{2}:\\d{2}:\\d{2}\\s+LOG\\d{1}\\[\\d{0,6}:\\d{0,60}\\]:\\s+", "");

			// Example OpenVPN<2.5: "Sat Oct 12 10:13:38 2019 /sbin/ip route add 0.0.0.0/1 via 10.20.6.1"
			str = System.Text.RegularExpressions.Regex.Replace(str, "^\\w{3}\\s+\\w{3}\\s+\\d{1,2}\\s+\\d{1,2}:\\d{1,2}:\\d{1,2}\\s+\\d{2,4}\\s+", "");

			// Example OpenVPN>=2.5: "OpenVPN > 2020-10-09 16:11:17 Socket Buffers"
			str = System.Text.RegularExpressions.Regex.Replace(str, "^\\d{2,4}-\\d{1,2}-\\d{1,2}\\s+\\d{1,2}:\\d{1,2}:\\d{1,2}\\s+", "");

			// Example SSL Linux: 2020.10.12 14:00:27 LOG6[0]: TLS connected: new session negotiated
			str = System.Text.RegularExpressions.Regex.Replace(str, "^\\d{2,4}\\.\\d{1,2}\\.\\d{1,2}\\s+\\d{1,2}:\\d{1,2}:\\d{1,2}\\s+", "");

			return str;
		}

		public override void OverrideElevatedCommandParameters()
		{

		}

		public override void EnsureDriver()
		{
			// Remember: WireGuard ALWAYS create and destroy adapter, for this the code below is OpenVPN-specific.
			if (Engine.Instance.Options.GetBool("advanced.skip_tun_detect") == false)
			{
				string driverRequested = Platform.Instance.GetConnectionTunDriver(this);

				Engine.Instance.WaitMessageSet(LanguageManager.GetText("OsDriverInstall", driverRequested), false);

				string interfaceName = Core.Engine.Instance.Options.Get("network.iface.name");
				if (interfaceName == "")
				{
					System.Net.NetworkInformation.NetworkInterface adapter = Platform.Instance.SearchAdapter(driverRequested);
					if (adapter != null)
						interfaceName = adapter.Name;
				}
				if (interfaceName == "")
				{
					interfaceName = "Eddie";
				}

				Platform.Instance.OpenVpnEnsureDriverAndAdapterAvailable(driverRequested, interfaceName);
			}
		}

		public override IpAddresses GetDns()
		{
			if (Engine.Instance.Options.GetBool("dns.delegate"))
				return m_configWithPush.ExtractDns();
			else
				return m_dns;
		}

		public override IpAddresses GetVpnIPs()
		{
			return m_configWithPush.ExtractVpnIPs();
		}

		public override bool GetProxyMode()
		{
			return ((m_configWithPush.ExistsDirective("socks-proxy")) || (m_configWithPush.ExistsDirective("http-proxy")));
		}

		public override string GetProtocolDescription()
		{
			string t = m_configStartup.GetOneDirectiveText("proto").ToUpperInvariant();
			if (Transport != "")
				t += " (" + Transport + ")";
			return t;
		}

		public override string ExportConfigStartup()
		{
			return m_configStartup.Build();
		}

		public override string ExportConfigPush()
		{
			return m_configWithPush.Build();
		}

		// -----------------------------------------
		// Virtual
		// -----------------------------------------

		public virtual ConfigBuilder.OpenVPN GenerateConfigBuilder()
		{
			return new ConfigBuilder.OpenVPN();
		}

		// -----------------------------------------
		// Public
		// -----------------------------------------

		public ConfigBuilder.OpenVPN ConfigStartup
		{
			get
			{
				return m_configStartup;
			}
		}

		// -----------------------------------------
		// Private
		// -----------------------------------------

		protected void DumpConfigToLog()
		{
			string t = "-- Start OpenVPN config dump\n" + ExportConfigPush() + "\n-- End OpenVPN config dump";
			t = Regex.Replace(t, "<ca>(.*?)</ca>", "<ca>omissis</ca>", RegexOptions.Singleline);
			t = Regex.Replace(t, "<key>(.*?)</key>", "<key>omissis</key>", RegexOptions.Singleline);
			t = Regex.Replace(t, "<cert>(.*?)</cert>", "<cert>omissis</cert>", RegexOptions.Singleline);
			t = Regex.Replace(t, "<tls-auth>(.*?)</tls-auth>", "<tls-auth>omissis</tls-auth>", RegexOptions.Singleline);
			t = Regex.Replace(t, "<tls-crypt>(.*?)</tls-crypt>", "<tls-crypt>omissis</tls-crypt>", RegexOptions.Singleline);
			Engine.Instance.Logs.Log(LogType.Verbose, t);
		}

		void SetNetworkLockSoftwareExceptionPath(string path)
		{
			m_networkLockSoftwareExceptionPath = path;
			if (Engine.Instance.NetworkLockManager != null)
				Engine.Instance.NetworkLockManager.AllowProgram(path);
		}

		void VpnStartProcess()
		{
			m_fileConfig = new TemporaryFile("ovpn");
			Platform.Instance.FileContentsWriteText(m_fileConfig.Path, m_configStartup.Build(), Encoding.UTF8);

			string path = Engine.Instance.GetOpenVpnTool().Path;

			if (m_processTransport == null)
				SetNetworkLockSoftwareExceptionPath(path);

			m_elevatedCommand = new Elevated.Command();
			m_elevatedCommand.Parameters["command"] = "openvpn";
			m_elevatedCommand.Parameters["action"] = "start";
			m_elevatedCommand.Parameters["id"] = Id;
			m_elevatedCommand.Parameters["path"] = path;
			m_elevatedCommand.Parameters["config"] = m_fileConfig.Path;
			OverrideElevatedCommandParameters();

			if (Platform.Instance.NeedExecuteOutsideAppPath(m_elevatedCommand.Parameters["path"]))
			{
				string tempPathToDelete = Platform.Instance.FileTempName(Platform.Instance.FileGetNameFromPath(m_elevatedCommand.Parameters["path"]));

				if (Platform.Instance.FileExists(tempPathToDelete))
					Platform.Instance.FileDelete(tempPathToDelete);
				System.IO.File.Copy(m_elevatedCommand.Parameters["path"], tempPathToDelete);

				m_elevatedCommand.Parameters["path"] = tempPathToDelete;

				m_deleteAfterStart = true;
			}

			m_elevatedCommand.ExceptionEvent += delegate (Elevated.Command cmd, string message)
			{
				Session.AddLogEvent("Tunnel", "Error: " + message);
			};

			m_elevatedCommand.ReceiveEvent += delegate (Elevated.Command cmd, string data)
			{
				if (data.StartsWithInv("stdout:"))
				{
					m_stdout.Write(data.Substring(7));
				}
				else if (data.StartsWithInv("stderr:"))
				{
					m_stderr.Write(data.Substring(7));
				}
				else if (data.StartsWithInv("procid:"))
				{
					//m_pid = Conversions.ToInt32(data.Substring(7));
					if (m_deleteAfterStart)
					{
						Platform.Instance.FileDelete(m_elevatedCommand.Parameters["path"]);
					}
				}
				else if (data.StartsWithInv("return:"))
				{
					m_stdout.Stop();
					m_stderr.Stop();
					Session.SetResetError();
				}
				else
					Session.AddLogEvent("Tunnel", data);
			};

			m_elevatedCommand.CompleteEvent += delegate (Elevated.Command cmd)
			{
				Session.SetResetError();
			};

			m_stdout.LineEvent += delegate (string line)
			{
				Session.AddLogEvent("Tunnel", line);
			};

			m_stderr.LineEvent += delegate (string line)
			{
				Session.AddLogEvent("Tunnel", "Error: " + line);
			};

			m_elevatedCommand.DoASync();
		}

		// Note: 'Transport' are tunnel over the VPN tunnel. It's here (and not object-oriented) because actually only OpenVPN (TCP) support it.

		void TransportStartProcessSSH()
		{
			string sshToolPath = Software.GetTool("ssh").Path;
			bool isPlink = (sshToolPath.ToLowerInvariant().EndsWith("plink.exe", StringComparison.InvariantCulture));

			if (m_processTransport != null) // Unexpected
				return;

			string fileKeyExtension = "key";
			if (isPlink)
				fileKeyExtension = "ppk";

			m_fileSshKey = new TemporaryFile(fileKeyExtension);
			Platform.Instance.FileContentsWriteText(m_fileSshKey.Path, Info.Provider.GetTransportSshKey(fileKeyExtension), Encoding.ASCII);

			if ((Platform.Instance.IsWindowsSystem()) && (fileKeyExtension == "key"))
			{
				// For Windows10 SSH
				Platform.Instance.FileEnsureCurrentUserOnly(m_fileSshKey.Path);
			}

			if (Platform.Instance.IsUnixSystem())
			{
				Platform.Instance.FileEnsurePermission(m_fileSshKey.Path, "600");
			}

			string arguments = "";
			arguments += " -i \"" + m_fileSshKey.Path + "\"";
			arguments += " -L " + Conversions.ToString(TransportSshLocalPort);
			if (EntryIP.IsV6)
				arguments += ":[::1]";
			else
				arguments += ":127.0.0.1";
			arguments += ":" + Conversions.ToString(TransportSshPortDestination);
			arguments += " sshtunnel@" + EntryIP;

			if (isPlink == false)
				arguments += " -p"; // ssh use -p
			else
				arguments += " -P"; // plink use -P
			arguments += " " + EntryPort;

			// TOOPTIMIZE: To bypass key confirmation. Not the best approach.
			// TOFIX: Maybe provide a UserKnownHostsFile...
			if (fileKeyExtension == "key")
				arguments += " -o UserKnownHostsFile=/dev/null -o StrictHostKeyChecking=no";

			arguments += " -N -T -v";

			SetNetworkLockSoftwareExceptionPath(sshToolPath);

			m_processTransport = new Process();
			m_processTransport.StartInfo.FileName = Platform.Instance.FileAdaptProcessExec(sshToolPath);
			m_processTransport.StartInfo.Arguments = arguments;
			m_processTransport.StartInfo.WorkingDirectory = Platform.Instance.DirectoryTemp();

			m_processTransport.StartInfo.Verb = "run";
			m_processTransport.StartInfo.CreateNoWindow = true;
			m_processTransport.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			m_processTransport.StartInfo.UseShellExecute = false;
			m_processTransport.StartInfo.RedirectStandardInput = true;
			m_processTransport.StartInfo.RedirectStandardError = true;
			m_processTransport.StartInfo.RedirectStandardOutput = true;

			m_processTransport.ErrorDataReceived += TransportProcessSshOutputDataReceived;
			m_processTransport.OutputDataReceived += TransportProcessSshOutputDataReceived;
			m_processTransport.Exited += TransportProcessExited;

			m_processTransport.Start();

			m_processTransport.BeginOutputReadLine();
			m_processTransport.BeginErrorReadLine();
		}

		void TransportStartProcessSSL()
		{
			if (m_processTransport != null) // Unexpected
				return;

			m_fileSslCrt = new TemporaryFile("crt");
			Platform.Instance.FileContentsWriteText(m_fileSslCrt.Path, Info.Provider.GetTransportSslCrt(), Encoding.ASCII);

			m_fileSslConfig = new TemporaryFile("ssl");

			string sslConfig = "";

			if (Platform.Instance.IsUnixSystem())
			{
				//sslConfig += "output = /dev/stdout\n"; // With this, with stunnel 5.01, we have duplicated output dump.
				sslConfig += "foreground = yes\n";  // Without this, the process fork and it's exit can't be detected.
			}
			if (Engine.Instance.Options.Get("ssl.options") != "")
				sslConfig += "options = " + Engine.Instance.Options.Get("ssl.options") + "\n";
			sslConfig += "client = yes\n";
			sslConfig += "debug = 6\n";
			if (Platform.Instance.IsUnixSystem())
				sslConfig += "pid = " + Engine.Instance.GetPathInData("stunnel.pid"); // Added 2.18.3. Note: don't like quoted path
			sslConfig += "\n";
			sslConfig += "[openvpn]\n";
			sslConfig += "accept = 127.0.0.1:" + Conversions.ToString(TransportSslLocalPort) + "\n";
			sslConfig += "connect = " + EntryIP + ":" + EntryPort + "\n";
			sslConfig += "TIMEOUTclose = 0\n";
			if (Engine.Instance.Options.GetInt("ssl.verify") != -1)
				sslConfig += "verify = " + Engine.Instance.Options.GetInt("ssl.verify").ToString() + "\n";
			sslConfig += "CAfile = " + m_fileSslCrt.Path + "\n"; // Note: don't like quoted path
			sslConfig += "\n";

			string sslConfigPath = m_fileSslConfig.Path;
			Platform.Instance.FileContentsWriteText(sslConfigPath, sslConfig, Encoding.UTF8);

			SetNetworkLockSoftwareExceptionPath(Software.GetTool("ssl").Path);

			m_processTransport = new Process();
			m_processTransport.StartInfo.FileName = Platform.Instance.FileAdaptProcessExec(Software.GetTool("ssl").Path);
			m_processTransport.StartInfo.Arguments = "\"" + Encoding.Default.GetString(Encoding.UTF8.GetBytes(sslConfigPath)).EscapeQuote() + "\""; // encoding workaround, stunnel expect utf8
			m_processTransport.StartInfo.WorkingDirectory = Platform.Instance.DirectoryTemp();

			m_processTransport.StartInfo.Verb = "run";
			m_processTransport.StartInfo.CreateNoWindow = true;
			m_processTransport.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			m_processTransport.StartInfo.UseShellExecute = false;
			m_processTransport.StartInfo.RedirectStandardInput = true;
			m_processTransport.StartInfo.RedirectStandardError = true;
			m_processTransport.StartInfo.RedirectStandardOutput = true;

			m_processTransport.ErrorDataReceived += TransportProcessSslOutputDataReceived;
			m_processTransport.OutputDataReceived += TransportProcessSslOutputDataReceived;
			m_processTransport.Exited += TransportProcessExited;

			m_processTransport.Start();

			m_processTransport.BeginOutputReadLine();
			m_processTransport.BeginErrorReadLine();
		}

		void TransportSendYes()
		{
			m_processTransport.StandardInput.WriteLine("y");
		}

		void TransportProcessExited(object sender, EventArgs e)
		{
			Session.SetResetError();
		}

		void TransportProcessSslOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
			{
				Session.AddLogEvent("SSL", e.Data.ToString());
			}
		}

		void TransportProcessSshOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
			{
				Session.AddLogEvent("SSH", e.Data.ToString());
			}
		}
	}
}
