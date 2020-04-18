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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Eddie.Core;

namespace Eddie.Core.Tools
{
	public class Curl : Tool
	{
		public string minVersionRequired = "7.21.7";

		public override void OnNormalizeVersion()
		{
			Version = ExtensionsString.RegExMatchOne(Version, "^curl\\s(.*?)\\s");
		}

		public override void ExceptionIfRequired()
		{
			if (Available() == false)
				throw new Exception(LanguageManager.GetText("ToolsCurlRequired"));

			if (Version.VersionCompare(minVersionRequired) == -1)
				throw new Exception(GetRequiredVersionMessage());
		}

		public override string GetFileName()
		{
			if (Platform.Instance.IsWindowsSystem())
			{
				return "curl.exe";
			}
			else
				return base.GetFileName();
		}

		public override string GetVersionArgument()
		{
			return "--version";
		}

		public string GetRequiredVersionMessage()
		{
			return LanguageManager.GetText("ToolsCurlVersionNotSupported", Version, minVersionRequired);
		}

		public HttpResponse Fetch(HttpRequest request)
		{
			HttpResponse response = new HttpResponse();

			ExceptionIfRequired();

			ProgramScope programScope = new ProgramScope(this.GetPath(), "curl");

			// Don't use proxy if connected to the VPN, or in special cases (checking) during connection.
			bool bypassProxy = request.BypassProxy;
			if (bypassProxy == false)
				bypassProxy = Engine.Instance.IsConnected();

			string dataParameters = "";
			if (request.Parameters.Count > 0)
			{
				foreach (string k in request.Parameters.Keys)
				{
					if (dataParameters != "")
						dataParameters += "&";
					dataParameters += SystemShell.EscapeAlphaNumeric(k) + "=" + Uri.EscapeUriString(request.Parameters[k]);
				}
			}

			string args = "";
			if (bypassProxy == false)
			{
				string proxyMode = Engine.Instance.Storage.GetLower("proxy.mode");
				string proxyWhen = Engine.Instance.Storage.GetLower("proxy.when");
				string proxyHost = Engine.Instance.Storage.Get("proxy.host");
				int proxyPort = Engine.Instance.Storage.GetInt("proxy.port");
				string proxyAuth = Engine.Instance.Storage.Get("proxy.auth").ToLowerInvariant();
				string proxyLogin = Engine.Instance.Storage.Get("proxy.login");
				string proxyPassword = Engine.Instance.Storage.Get("proxy.password");

				if ((proxyWhen == "none") || (proxyWhen == "openvpn"))
					proxyMode = "none";

				if (proxyMode == "detect")
					throw new Exception(LanguageManager.GetText("ProxyDetectDeprecated"));

				if (proxyMode == "tor")
				{
					proxyMode = "socks";
					proxyAuth = "none";
					proxyLogin = "";
					proxyPassword = "";
				}

				if (proxyMode == "http")
				{
					args += " --proxy http://" + SystemShell.EscapeHost(proxyHost) + ":" + proxyPort.ToString();
				}
				else if (proxyMode == "socks")
				{
					// curl support different types of proxy. OpenVPN not, only socks5. So, it's useless to support other kind of proxy here.
					args += " --proxy socks5://" + SystemShell.EscapeHost(proxyHost) + ":" + proxyPort.ToString();
				}

				if ((proxyMode != "none") && (proxyAuth != "none"))
				{
					if (proxyAuth == "basic")
						args += " --proxy-basic";
					else if (proxyAuth == "ntlm")
						args += " --proxy-ntlm";

					if (SystemShell.EscapeInsideQuoteAcceptable(proxyLogin) == false)
						throw new Exception(LanguageManager.GetText("UnacceptableCharacters", "Proxy Login"));

					if (SystemShell.EscapeInsideQuoteAcceptable(proxyPassword) == false)
						throw new Exception(LanguageManager.GetText("UnacceptableCharacters", "Proxy Password"));

					if ((proxyLogin != "") && (proxyPassword != ""))
						args += " --proxy-user \"" + SystemShell.EscapeInsideQuote(proxyLogin) + "\":\"" + SystemShell.EscapeInsideQuote(proxyPassword) + "\"";
				}
			}

			args += " \"" + SystemShell.EscapeUrl(request.Url) + "\"";
			args += " -sS"; // -s Silent mode, -S with errors
			args += " --max-time " + Engine.Instance.Storage.GetInt("http.timeout").ToString();

			string pathCacert = Engine.Instance.LocateResource("cacert.pem");
			if(pathCacert != "")
				args += " --cacert \"" + SystemShell.EscapePath(pathCacert) + "\"";

			if (request.ForceResolve != "")
				args += " --resolve " + request.ForceResolve;

			if (dataParameters != "")
				args += " --data \"" + dataParameters + "\"";

			if (request.IpLayer == "4")
				args += " -4";
			if (request.IpLayer == "6")
				args += " -6"; 

			args += " -i";

            // http-100-continue issue: the body above are parsed uncorrectly. 2.19.1
            // Workaround: for now, force header to avoid
            args += " -H 'Expect:'";

			string error = "";
			try
			{
				using(Process p = new Process())
				{
					p.StartInfo.FileName = SystemShell.EscapePath(this.GetPath());
					p.StartInfo.Arguments = args;
					p.StartInfo.WorkingDirectory = "";

					p.StartInfo.CreateNoWindow = true;
					p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.RedirectStandardOutput = true;
					p.StartInfo.RedirectStandardError = true;

					p.Start();
					
					{
						using(System.IO.MemoryStream Stream = new System.IO.MemoryStream())
						{
							byte[] bufferHeader = default(byte[]);

							using (System.IO.MemoryStream StreamHeader = new System.IO.MemoryStream())
							{
								using(System.IO.MemoryStream StreamBody = new System.IO.MemoryStream())
								{
									byte[] buffer = new byte[4096];
									int read;
									while((read = p.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
									{
										Stream.Write(buffer, 0, read);
									}

									if(Stream.Length >= 4)
									{
										byte[] buffer2 = Stream.ToArray();
										int i = 0;
										for(; i < Stream.Length - 4; i++)
										{
											if((buffer2[i] == 13) && (buffer2[i + 1] == 10) && (buffer2[i + 2] == 13) && (buffer2[i + 3] == 10))
											{
												StreamHeader.Write(buffer2, 0, i);
												StreamBody.Write(buffer2, i + 4, (int)Stream.Length - i - 4);
												break;
											}
										}

										if(StreamHeader.Length == 0)
											StreamHeader.Write(buffer2, 0, (int)Stream.Length);
									}
									else
									{
										StreamHeader.Write(Stream.ToArray(), 0, (int)Stream.Length);
									}

									bufferHeader = StreamHeader.ToArray();
									response.BufferData = StreamBody.ToArray();
								}								
							}							

							string headers = System.Text.Encoding.ASCII.GetString(bufferHeader);
							string[] headersLines = headers.Split('\n');
							for(int l = 0; l < headersLines.Length; l++)
							{
								string line = headersLines[l];
								if (l == 0)
									response.Status = line.Trim();
								else
								{
									int posSep = line.IndexOfInv(":");
									if (posSep != -1)
									{
										string k = line.Substring(0, posSep);
										string v = line.Substring(posSep + 1);
										response.Headers.Add(new KeyValuePair<string, string>(k.ToLowerInvariant().Trim(), v.Trim()));
									}
								}
							}

						}
					}

					error = p.StandardError.ReadToEnd();

					p.WaitForExit();
				}
			}
			catch (Exception e)
			{
				error = e.Message;
			}

			programScope.End();

			if (error != "")
				throw new Exception(error.Trim());

			return response;
		}
	}
}
