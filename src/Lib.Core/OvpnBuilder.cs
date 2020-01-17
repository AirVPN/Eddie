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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Core
{
	public class OvpnBuilder
	{
		public static string AllowedCharsInDirectiveName = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_<>";

		public class Directive
		{
			public string Text = "";
			public string Comment = "";
		}

		public Dictionary<string, List<Directive>> Directives = new Dictionary<string, List<Directive>>();

		public bool IsMultipleDirective(string name)
		{
			// If a directive can be specified more time.
			if (
				(name == "remote") ||
				(name == "route") ||
				(name == "route-ipv6") ||
				(name == "dhcp-option") ||
				(name == "plugin") ||
				(name == "x509-track") ||
				(name == "http-proxy-option") ||
				(name == "pull-filter") ||
				(name == "ignore-unknown-option") ||
				(name == "setenv")
			  )
				return true;

			return false;
		}

		public bool IsGroupAllowScriptSecurity(string name)
		{
			if (
			    (name == "script-security") || 
				(name == "plugin") ||
				(name == "up") ||
				(name == "down") ||
				(name == "client-connect") ||
				(name == "client-disconnect") ||
				(name == "learn-address") ||
				(name == "auth-user-pass-verify") ||
				(name == "tls-verify") ||
				(name == "ipchange") ||
				(name == "iproute") ||
				(name == "route-up") ||
				(name == "route-pre-down")
			  )
				return true;

			return false;
		}

		public static int DirectiveOrder(string name)
		{
			// Some directive, for example 'max-routes', must be before the other 'route' directives.
			// Some ordering it's only for readability.

			if (name == "dev")
				return 0;
			else if (name == "proto")
				return 1;
			else if (name == "remote")
				return 2;
			else if (name == "max-routes")
				return 100;
			else if (name == "ca")
				return 10000;
			else if (name == "cert")
				return 10001;
			else if (name == "key")
				return 10001;
			else if (name.StartsWith("<", StringComparison.InvariantCulture))
				return 10010;
			else
				return 1000;
		}

		public static int CompareDirectiveOrder(string d1, string d2)
		{
			int w1 = DirectiveOrder(d1);
			int w2 = DirectiveOrder(d2);
			return w1.CompareTo(w2);
		}

		public OvpnBuilder Clone()
		{
			// Note: only directives
			OvpnBuilder n = new OvpnBuilder();
			foreach (KeyValuePair<string, List<Directive>> kp in Directives)
			{
				foreach (Directive d in kp.Value)
				{
					n.AppendDirective(kp.Key, d.Text, d.Comment);
				}
			}
			return n;
		}

		public string Get()
		{
			string result = "";

			DateTime now = DateTime.UtcNow;

			result += "# " + Engine.Instance.GenerateFileHeader() + " - " + now.ToLongDateString() + " " + now.ToLongTimeString() + " UTC\n";

			// Obtain directive key list
			List<string> directives = new List<string>();
			foreach (KeyValuePair<string, List<Directive>> kp in Directives)
			{
				directives.Add(kp.Key);
			}

			// Sorting
			directives.Sort(CompareDirectiveOrder);

			foreach (string directiveKey in directives)
			{
				List<Directive> directivesKey = Directives[directiveKey];
				foreach (Directive value in directivesKey)
				{
					if (directiveKey.StartsWithInv("<"))
					{
						result += directiveKey + "\n" + value.Text.Trim() + "\n" + directiveKey.Replace("<", "</");
					}
					else
					{
						if (value.Text.Trim() != "")
							result += directiveKey + " " + value.Text.Trim();
						else
							result += directiveKey; 
					}

					if( (Engine.Instance.GetOpenVpnTool() is Tools.Hummingbird) == false) // Known Hummingbird bug: don't trim correctly # comments
						if (value.Comment != "")
						    result += " # " + value.Comment;
					result += "\n";
				}
			}

			return result;
		}

		public void AppendDirective(string name, string body, string comment)
		{
			AppendDirective(name, body, comment, false);
		}

		public void AppendDirective(string name, string body, string comment, bool force)
		{
			name = name.Trim().PruneCharsNotIn(AllowedCharsInDirectiveName);

			// Eddie-special: If start with -, remove.
			if (name.StartsWithInv("-"))
			{
				if (Directives.ContainsKey(name.Substring(1)))
					Directives.Remove(name.Substring(1));
			}
			else
			{
				if (IsMultipleDirective(name))
				{
					if (Directives.ContainsKey(name) == false)
						Directives[name] = new List<Directive>();
				}
				else
				{
					Directives[name] = new List<Directive>();
				}

				if(force == false)
					//if (Engine.Instance.Storage.GetBool("openvpn.allow.script-security") == false)
						if (IsGroupAllowScriptSecurity(name))
							return;
				
				Directive d = new Directive();
				d.Text = body.Trim();
				d.Comment = comment.Trim();
				Directives[name].Add(d);
			}
		}

		public bool ExistsDirective(string name)
		{
			return Directives.ContainsKey(name);
		}

		public void RemoveDirective(string name)
		{
			if (Directives.ContainsKey(name))
				Directives.Remove(name);
		}

		public List<Directive> GetDirectiveList(string name)
		{
			if (Directives.ContainsKey(name) == false)
				return null;
			else
				return Directives[name];
		}

		public Directive GetOneDirective(string name)
		{
			if (Directives.ContainsKey(name) == false)
				return null;
			if (Directives[name].Count == 0)
				return null;
			return Directives[name][0];
		}

		public string GetOneDirectiveText(string name)
		{
			if (Directives.ContainsKey(name) == false)
				return "";
			if (Directives[name].Count == 0)
				return "";
			return Directives[name][0].Text;
		}

		public void AppendDirectives(string directives, string comment)
		{
			string text = directives;

			// Cleaning			
			text = "\n" + directives.Replace("\r", "\n") + "\n";

			for (; ; )
			{
				string originalText = text;

				text = text.Replace("\n\n", "\n");
				text = text.Replace("  ", " ");
				text = text.Replace("\t", " ");
				text = text.Replace("\u2028", "\n"); // macOS Hack  // TOCLEAN

				int posComment1 = text.IndexOf("#", StringComparison.InvariantCulture);
				if (posComment1 != -1)
				{
					int posEndOfLine = text.IndexOf("\n", posComment1, StringComparison.InvariantCulture);
					text = text.Substring(0, posComment1) + text.Substring(posEndOfLine);
				}

				/* Removed in 2.14.3
				int posComment2 = text.IndexOf("\n;");
				if (posComment2 != -1)
				{
					int posEndOfLine = text.IndexOf("\n", posComment2);
					text = text.Substring(0, posComment2) + text.Substring(posEndOfLine);
				}
				*/

				if (text == originalText)
					break;
			}

			for (; ; )
			{
				text = text.Trim();

				if (text == "")
					break;

				string directiveName;
				string directiveBody;

				if (text.Substring(0, 1) == "<")
				{
					int posEndStartTag = text.IndexOf('>');
					if (posEndStartTag == -1)
						throw new Exception("Syntax error"); // TOTRANSLATE

					directiveName = text.Substring(0, posEndStartTag + 1);
					string endTag = directiveName.Replace("<", "</");
					int posEndTag = text.IndexOf(endTag, StringComparison.InvariantCulture);
					if (posEndTag == -1)
						throw new Exception("Syntax error"); // TOTRANSLATE
					directiveBody = text.Substring(posEndStartTag + 1, posEndTag - posEndStartTag - 1);
					text = text.Substring(posEndTag + endTag.Length);
				}
				else
				{
					int posEndLine = text.IndexOf("\n", StringComparison.InvariantCulture);

					// v2
					string textL = "";
					if (posEndLine == -1)
					{
						textL = text;
						text = "";
					}
					else
					{
						textL = text.Substring(0, posEndLine);
						text = text.Substring(posEndLine + 1);
					}

					if (textL.StartsWith(";", StringComparison.InvariantCulture))
						continue;

					// Presume there isn't any directive name with two or more words.
					int posSpace = textL.IndexOf(" ", StringComparison.InvariantCulture);
					if (posSpace == -1)
					{
						directiveName = textL;
						directiveBody = "";
					}
					else
					{
						directiveName = textL.Substring(0, posSpace);
						directiveBody = textL.Substring(posSpace + 1);
					}
					/*
					int posSpace = text.IndexOf(" ");
					int posEndLine = text.IndexOf("\n");
					if (posSpace == -1)
					{
						directiveName = text;
						directiveBody = "";
						text = "";
					}
					else if( (posEndLine != -1) && (posEndLine < posSpace) )
					{
						directiveName = text.Substring(0, posEndLine);
						directiveBody = "";
						text = text.Substring(posEndLine);
					}
					else
					{
						directiveName = text.Substring(0, posSpace);
						
						if (posEndLine == -1)
						{
							directiveBody = text.Substring(posSpace +1);
							text = "";
						}
						else
						{
							directiveBody = text.Substring(posSpace + 1, posEndLine - posSpace - 1);
							text = text.Substring(posEndLine);
						}
					}
					*/
				}

				AppendDirective(directiveName.Trim(), directiveBody.Trim(), comment);
			}
		}

		public IpAddresses ExtractVpnIPs()
		{
			IpAddresses result = new IpAddresses();
			if (ExistsDirective("ifconfig"))
			{
				string ip = GetOneDirectiveText("ifconfig");
				result.Add(ip);
			}

			if (ExistsDirective("ifconfig-ipv6"))
			{
				string[] fields = GetOneDirectiveText("ifconfig-ipv6").Split(' ');
				if (fields.Length == 2)
					result.Add(fields[0]);
			}

			return result;
		}

		public IpAddresses ExtractDns()
		{
			IpAddresses result = new IpAddresses();
			List<Directive> directives = GetDirectiveList("dhcp-option");
			if (directives != null)
			{
				foreach (Directive d in directives)
				{
					string[] fields = d.Text.Split(' ');
					if (fields.Length != 2)
						continue;
					if (fields[0] == "DNS")
						result.Add(fields[1]);
					if (fields[0] == "DNS6")
						result.Add(fields[1]);
				}
			}

			return result;
		}

		public IpAddresses ExtractGateway()
		{
			IpAddresses result = new IpAddresses();
			if (ExistsDirective("route-gateway"))
			{
				string ip = GetOneDirectiveText("route-gateway");
				result.Add(ip);
			}

			if (ExistsDirective("ifconfig-ipv6"))
			{
				string[] fields = GetOneDirectiveText("ifconfig-ipv6").Split(' ');
				if (fields.Length == 2)
					result.Add(fields[1]);
			}

			return result;
		}

		public string ExtractCipher()
		{
			return GetOneDirectiveText("cipher");
		}
		
		// Normalize path if relative
		public void NormalizeRelativePath(string path)
		{
			// NB: Assume that path it's the first field of directive body.
			NormalizeRelativePathDirective("auth-user-pass", path);
			NormalizeRelativePathDirective("ca", path);
			NormalizeRelativePathDirective("cert", path);
			NormalizeRelativePathDirective("crl-verify", path);
			NormalizeRelativePathDirective("dh", path);
			NormalizeRelativePathDirective("extra-certs", path);
			NormalizeRelativePathDirective("http-proxy-user-pass", path);
			NormalizeRelativePathDirective("key", path);
			NormalizeRelativePathDirective("pkcs12", path);
			NormalizeRelativePathDirective("secret", path);
			NormalizeRelativePathDirective("tls-auth", path);
			NormalizeRelativePathDirective("tls-crypt", path);
		}

		public void NormalizeRelativePathDirective(string name, string basePath)
		{
			List<Directive> list = GetDirectiveList(name);
			if (list != null)
			{
				foreach (Directive d in list)
				{
					string body = d.Text.Trim();
					if (body == "")
						continue;
					List<string> fields = body.StringToList(" ");
					if (fields.Count < 1)
						return;
					string path = fields[0];
					fields.RemoveAt(0);
					if ((path.StartsWith("\"", StringComparison.InvariantCulture)) && (path.EndsWith("\"", StringComparison.InvariantCulture)))
						path = path.Substring(1, path.Length - 2);
					path = Platform.Instance.FileGetAbsolutePath(path, basePath);
					d.Text = EncodePath(path) + " " + String.Join(" ", fields.ToArray());
					d.Text = d.Text.Trim();
				}
			}
		}

		// Apply some fixes
		public void Normalize()
		{
			// TOOPTIMIZE: Currently Eddie don't work well with verb>3
			AppendDirective("verb", "3", "");

			// Eddie have it's own Lock DNS (based on WFP, same as block-outside-dns)
			if (ExistsDirective("block-outside-dns"))
				RemoveDirective("block-outside-dns");

			// explicit-exit-notify works only with udp
			if (GetOneDirectiveText("proto").ToLowerInvariant().StartsWith("udp", StringComparison.InvariantCulture) == false)
				RemoveDirective("explicit-exit-notify");

			// If exists "keepalive", don't use any ping-* directives
			if (ExistsDirective("keepalive"))
			{
				RemoveDirective("ping");
				RemoveDirective("ping-exit");
				RemoveDirective("ping-restart");
			}

			// OpenVPN < 2.4 allows 100 route directives max by default.
			// Since Eddie can't know here how many routes are pulled from an OpenVPN server, it uses some tolerance. In any case manual setting is possible.
			if (Engine.Instance.GetOpenVpnTool().VersionUnder("2.4")) // max-routes is deprecated in 2.4
			{
				if (ExistsDirective("max-routes") == false) // Only if not manually specified
				{
					if (ExistsDirective("route"))
					{
						List<Directive> routes = Directives["route"];
						if ((routes != null) && (routes.Count > 50))
						{
							AppendDirective("max-routes", (routes.Count + 100).ToString(), "Automatic");
						}
					}
				}
			}
		}

		// Utils
		public string EncodePath(string path)
		{
			path = path.Replace("\\", "\\\\"); // Escaping for Windows
			return "\"" + path + "\"";
		}
	}
}
