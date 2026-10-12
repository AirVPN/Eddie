using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Eddie.Core.ConfigBuilder
{
	public class awgprofile
	{
		public static readonly string[] awgkeys = { "Jc", "Jmin", "Jmax", "S1", "S2", "S3", "S4", "H1", "H2", "H3", "H4", "I1", "I2", "I3", "I4", "I5" };
		public static readonly string[] interfacekeys = { "PrivateKey", "Address", "DNS", "ListenPort", "MTU", "FwMark", "Table" };
		public static readonly string[] peerkeys = { "PublicKey", "PresharedKey", "Endpoint", "AllowedIPs", "PersistentKeepalive" };
		public readonly Dictionary<string, string> iface = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		public readonly List<Dictionary<string, string>> peers = new List<Dictionary<string, string>>();
		public int version;

		public static awgprofile create(int protocol)
		{
			var profile = new awgprofile();
			profile.version = protocol;
			profile.peers.Add(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
			profile.peers[0]["AllowedIPs"] = "0.0.0.0/0, ::/0";
			profile.peers[0]["PersistentKeepalive"] = "15";
			if (protocol > 0) profile.defaults();
			return profile;
		}

		public void defaults()
		{
			iface["Jc"] = "5";
			iface["Jmin"] = "30";
			iface["Jmax"] = "100";
			for (int i = 1; i <= 4; i++)
			{
				iface["S" + i] = "0";
				iface["H" + i] = i.ToString(CultureInfo.InvariantCulture);
			}
			if (version == 1) { iface.Remove("S3"); iface.Remove("S4"); }
		}

		public static string get(Dictionary<string, string> values, string key)
		{
			string value;
			return values.TryGetValue(key, out value) ? value : "";
		}

		public static bool isawgkey(string key)
		{
			return Array.Exists(awgkeys, item => item.Equals(key, StringComparison.OrdinalIgnoreCase));
		}

		public static awgprofile parse(string text)
		{
			var profile = new awgprofile();
			Dictionary<string, string> section = null;
			bool sawinterface = false;
			int number = 0;
			foreach (string original in text.TrimStart('\ufeff').Split('\n'))
			{
				number++;
				string line = original.Split('#')[0].Trim();
				if (line == "") continue;
				if (line.Equals("[Interface]", StringComparison.OrdinalIgnoreCase))
				{
					if (sawinterface || profile.peers.Count > 0) throw new FormatException("duplicate or misplaced interface section");
					sawinterface = true;
					section = profile.iface;
					continue;
				}
				if (line.Equals("[Peer]", StringComparison.OrdinalIgnoreCase))
				{
					if (!sawinterface) throw new FormatException("peer section must follow interface");
					section = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
					profile.peers.Add(section);
					continue;
				}
				int equals = line.IndexOf('=');
				if (section == null || equals <= 0) throw new FormatException("invalid configuration at line " + number);
				string key = line.Substring(0, equals).Trim();
				string value = line.Substring(equals + 1).Trim();
				if (section.ContainsKey(key))
				{
					if (key.Equals("Address", StringComparison.OrdinalIgnoreCase) || key.Equals("DNS", StringComparison.OrdinalIgnoreCase) || key.Equals("AllowedIPs", StringComparison.OrdinalIgnoreCase))
						section[key] += ", " + value;
					else throw new FormatException("duplicate parameter " + key.ToLowerInvariant());
				}
				else section[key] = value;
				if (isawgkey(key)) profile.version = Math.Max(profile.version, key.Equals("S3", StringComparison.OrdinalIgnoreCase) || key.Equals("S4", StringComparison.OrdinalIgnoreCase) || key.StartsWith("I", StringComparison.OrdinalIgnoreCase) || (key.StartsWith("H", StringComparison.OrdinalIgnoreCase) && value.Contains("-")) ? 2 : 1);
			}
			if (!sawinterface) throw new FormatException("missing interface section");
			return profile;
		}

		public string build()
		{
			var output = new StringBuilder();
			output.Append("[Interface]\n");
			write(output, iface, interfacekeys);
			if (version > 0) write(output, iface, awgkeys);
			writeunknown(output, iface, interfacekeys, awgkeys);
			foreach (var peer in peers)
			{
				output.Append("\n[Peer]\n");
				write(output, peer, peerkeys);
				writeunknown(output, peer, peerkeys, new string[0]);
			}
			return output.ToString();
		}

		private static void write(StringBuilder output, Dictionary<string, string> values, string[] keys)
		{
			foreach (string key in keys)
			{
				string value = get(values, key);
				if (value != "") output.Append(key + " = " + value + "\n");
			}
		}

		private static void writeunknown(StringBuilder output, Dictionary<string, string> values, string[] first, string[] second)
		{
			foreach (var pair in values)
				if (!Array.Exists(first, key => key.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)) && !Array.Exists(second, key => key.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)))
					output.Append(pair.Key + " = " + pair.Value + "\n");
		}

		public List<string> validate()
		{
			var errors = new List<string>();
			checkkey(errors, iface, "PrivateKey", true);
			checkips(errors, get(iface, "Address"), "address", true, true);
			if (get(iface, "DNS") != "")
				foreach (string item in get(iface, "DNS").Split(','))
					if (!IPAddress.TryParse(item.Trim(), out _) && Uri.CheckHostName(item.Trim()) != UriHostNameType.Dns) errors.Add("dns: invalid address or domain");
			number(errors, iface, "ListenPort", 0, 65535);
			number(errors, iface, "MTU", 576, 65535);
			string mark = get(iface, "FwMark");
			if (mark != "" && !uint.TryParse(mark.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? mark.Substring(2) : mark, mark.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture, out _)) errors.Add("fwmark: expected uint32");
			string table = get(iface, "Table");
			if (table != "" && table != "off" && table != "auto" && (!uint.TryParse(table, out uint tablevalue) || tablevalue == 0)) errors.Add("table: expected auto, off or a positive number");
			if (peers.Count == 0) errors.Add("missing peer section");
			foreach (var peer in peers)
			{
				checkkey(errors, peer, "PublicKey", true);
				checkkey(errors, peer, "PresharedKey", false);
				checkips(errors, get(peer, "AllowedIPs"), "allowedips", true, true);
				number(errors, peer, "PersistentKeepalive", 0, 65535);
				string endpoint = get(peer, "Endpoint");
				int colon = endpoint.LastIndexOf(':');
				if (colon <= 0 || !int.TryParse(endpoint.Substring(colon + 1), out int port) || port < 1 || port > 65535) errors.Add("endpoint: expected host:port or [ipv6]:port, port 1–65535");
				else
				{
					string host = endpoint.Substring(0, colon);
					if (host.Contains(":"))
					{
						if (!host.StartsWith("[") || !host.EndsWith("]") || !IPAddress.TryParse(host.Trim('[', ']'), out IPAddress ip) || ip.AddressFamily != AddressFamily.InterNetworkV6) errors.Add("endpoint: ipv6 must be enclosed in brackets");
					}
					else if (Uri.CheckHostName(host) == UriHostNameType.Unknown) errors.Add("endpoint: invalid hostname");
				}
				checkfields(errors, peer, peerkeys, false);
			}
			checkfields(errors, iface, interfacekeys, true);
			if (version == 0)
			{
				foreach (string key in awgkeys) if (iface.ContainsKey(key)) errors.Add("select amneziawg for " + key.ToLowerInvariant());
				return errors;
			}
			errors.AddRange(validateawg());
			return errors;
		}

		public List<string> validateawg()
		{
			var errors = new List<string>();
			number(errors, iface, "Jc", version == 1 ? 0 : 1, 128);
			number(errors, iface, "Jmin", version == 1 ? 0 : 1, 1280);
			number(errors, iface, "Jmax", version == 1 ? 0 : 1, 1500);
			if (int.TryParse(get(iface, "Jmin"), out int jmin) && int.TryParse(get(iface, "Jmax"), out int jmax) && jmin > jmax) errors.Add("jmin must not exceed jmax");
			for (int i = 1; i <= 4; i++) number(errors, iface, "S" + i, 0, 65535);
			var ranges = new List<Tuple<uint, uint>>();
			for (int i = 1; i <= 4; i++)
			{
				string value = get(iface, "H" + i);
				if (value == "") value = i.ToString(CultureInfo.InvariantCulture);
				string[] bounds = value.Split('-');
				uint low, high;
				if (bounds.Length > 2 || !uint.TryParse(bounds[0], NumberStyles.None, CultureInfo.InvariantCulture, out low) || low == 0 || !uint.TryParse(bounds[bounds.Length - 1], NumberStyles.None, CultureInfo.InvariantCulture, out high) || high < low) errors.Add("h" + i + ": expected integer 1–4294967295 or min-max range");
				else
				{
					foreach (var other in ranges) if (low <= other.Item2 && high >= other.Item1) errors.Add("h1–h4 ranges must not overlap");
					ranges.Add(Tuple.Create(low, high));
				}
			}
			bool gap = false;
			for (int i = 1; i <= 5; i++)
			{
				string value = get(iface, "I" + i);
				if (value == "") gap = true;
				else
				{
					if (gap) errors.Add("cps: i1–i5 must be consecutive");
					string error = validatesignature(value);
					if (error != "") errors.Add("i" + i + ": " + error);
				}
			}
			if (version == 1)
			{
				foreach (string key in new[] { "S3", "S4", "I1", "I2", "I3", "I4", "I5" }) if (get(iface, key) != "") errors.Add(key.ToLowerInvariant() + ": requires amneziawg 2.0");
				for (int i = 1; i <= 4; i++) if (get(iface, "H" + i).Contains("-")) errors.Add("h ranges require amneziawg 2.0");
			}
			return errors;
		}

		private static void checkfields(List<string> errors, Dictionary<string, string> values, string[] supported, bool isinterface)
		{
			foreach (var pair in values)
			{
				if (!Array.Exists(supported, key => key.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)) && !(isinterface && isawgkey(pair.Key))) errors.Add("unsupported parameter " + pair.Key.ToLowerInvariant());
				if (pair.Value.Contains("\n") || pair.Value.Contains("\r") || pair.Value.Contains("#")) errors.Add("parameter " + pair.Key.ToLowerInvariant() + " contains a newline or comment character");
			}
		}

		public string buildawg()
		{
			var output = new StringBuilder();
			write(output, iface, awgkeys);
			return output.ToString();
		}

		public static awgprofile airvpn(string directives)
		{
			var profile = create(2);
			if (directives.Trim() != "")
			{
				var custom = parse("[Interface]\n" + directives);
				if (custom.peers.Count != 0) throw new FormatException("airvpn settings must contain only awg interface directives");
				foreach (var pair in custom.iface)
				{
					if (!isawgkey(pair.Key)) throw new FormatException("unsupported airvpn awg parameter " + pair.Key.ToLowerInvariant());
					profile.iface[pair.Key] = pair.Value;
				}
			}
			var errors = profile.validateawg();
			for (int i = 1; i <= 4; i++)
			{
				if (get(profile.iface, "S" + i) != "0") errors.Add("airvpn servers require s1-s4 = 0");
				if (get(profile.iface, "H" + i) != i.ToString(CultureInfo.InvariantCulture)) errors.Add("airvpn servers require h1-h4 = 1, 2, 3, 4");
			}
			if (errors.Count > 0) throw new FormatException(string.Join("; ", errors));
			return profile;
		}

		public static List<string[]> presets()
		{
			var result = new List<string[]>();
			using (var stream = typeof(awgprofile).Assembly.GetManifestResourceStream("awg.cps.csv"))
			using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
			{
				while (!reader.EndOfStream)
				{
					string[] values = reader.ReadLine().Split(',');
					if (values.Length != 7) continue;
					string[] compact = compactcps(values[2], values[3], values[4], values[5], values[6]);
					if (compact[0] == "") continue;
					Array.Copy(compact, 0, values, 2, 5);
					bool valid = true;
					for (int i = 2; i < 7; i++) if (values[i] != "" && validatesignature(values[i]) != "") valid = false;
					if (valid) result.Add(values);
				}
			}
			return result;
		}

		private static void checkkey(List<string> errors, Dictionary<string, string> values, string key, bool required)
		{
			string value = get(values, key);
			if (value == "" && !required) return;
			try { if (value.Length != 44 || Convert.FromBase64String(value).Length != 32) errors.Add(key.ToLowerInvariant() + ": expected a base64 encoded 32-byte key"); }
			catch (FormatException) { errors.Add(key.ToLowerInvariant() + ": invalid base64 key"); }
		}

		private static void checkips(List<string> errors, string value, string key, bool prefix, bool required)
		{
			if (value == "" && !required) return;
			foreach (string item in value.Split(','))
			{
				string[] parts = item.Trim().Split('/');
				if (!IPAddress.TryParse(parts[0], out IPAddress ip) || parts.Length > 2 || (prefix && parts.Length != 2) || (parts.Length == 2 && (!int.TryParse(parts[1], out int bits) || bits < 0 || bits > (ip.AddressFamily == AddressFamily.InterNetwork ? 32 : 128)))) errors.Add(key + ": invalid address or prefix");
			}
		}

		private static void number(List<string> errors, Dictionary<string, string> values, string key, long min, long max)
		{
			string value = get(values, key);
			if (value != "" && (!long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out long parsed) || parsed < min || parsed > max)) errors.Add(key.ToLowerInvariant() + ": expected integer " + min + "–" + max);
		}

		public static string validatesignature(string signature)
		{
			var matches = Regex.Matches(signature, "<(b 0x[0-9a-fA-F]+|t|(r|rc|rd) [0-9]+)>");
			int end = 0;
			foreach (Match match in matches)
			{
				if (match.Index != end) return "invalid content between cps tags";
				string tag = match.Groups[1].Value;
				if (tag.StartsWith("b 0x") && (tag.Length - 4) % 2 != 0) return "hex must contain an even number of characters";
				if (tag.StartsWith("r") && (!int.TryParse(tag.Substring(tag.IndexOf(' ') + 1), out int length) || length < 1 || length > 1000)) return "r, rc, rd length must be 1–1000";
				end = match.Index + match.Length;
			}
			return matches.Count > 0 && end == signature.Length ? "" : "invalid cps signature";
		}

		public static string[] compactcps(params string[] values)
		{
			var compact = new List<string>();
			foreach (string value in values)
			{
				string lower = value.ToLowerInvariant();
				if (Regex.IsMatch(lower, "^<b 0x.*>$")) compact.Add(lower);
			}
			while (compact.Count < 5) compact.Add("");
			return compact.ToArray();
		}

		private static int random(int min, int max)
		{
			using (var rng = RandomNumberGenerator.Create())
			{
				var bytes = new byte[4];
				uint size = (uint)(max - min), limit = uint.MaxValue - uint.MaxValue % size, value;
				do { rng.GetBytes(bytes); value = BitConverter.ToUInt32(bytes, 0); } while (value >= limit);
				return min + (int)(value % size);
			}
		}

		public void randomjunk()
		{
			iface["Jc"] = random(1, 128).ToString(CultureInfo.InvariantCulture);
			int min = random(1, 101);
			iface["Jmin"] = min.ToString(CultureInfo.InvariantCulture);
			iface["Jmax"] = random(min, 1280).ToString(CultureInfo.InvariantCulture);
		}

		public static string privatekey()
		{
			var key = new byte[32];
			using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(key);
			key[0] &= 248;
			key[31] = (byte)((key[31] & 127) | 64);
			return Convert.ToBase64String(key);
		}

		private static string hex(int length)
		{
			var bytes = new byte[(length + 1) / 2];
			using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
			return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
		}

		private static double fraction() { return random(0, 16777216) / 16777216.0; }

		private static string rtags(double factor)
		{
			string[] tags = { "r", "rc", "rd" };
			for (int i = tags.Length - 1; i > 0; i--) { int j = random(0, i + 1); string swap = tags[i]; tags[i] = tags[j]; tags[j] = swap; }
			var result = new StringBuilder();
			foreach (string tag in tags)
			{
				int length = tag == "r" ? (fraction() * factor > 0.5 ? random(80, 201) : random(10, 31)) : random(1, 1000);
				if (fraction() * factor > 0.5) result.Append("<" + tag + " " + length + ">");
			}
			return result.ToString();
		}

		public static string quic(bool initial)
		{
			return "<b 0x" + (random(0, 16) | (initial ? 0xc0 : 0xd0)).ToString("x2") + "00000001><b 0x" + hex(4) + ">" + rtags(fraction()) + "<t>" + rtags(fraction());
		}

		public static string junk()
		{
			string signature = rtags(fraction()) + "<b 0x" + hex(random(4, 10)) + ">" + rtags(fraction());
			if (fraction() < 0.5) signature += "<t>";
			if (fraction() < 0.7) signature += rtags(fraction()) + "<b 0x" + hex(4) + ">" + rtags(fraction());
			if (fraction() < 0.5) signature += "<t>";
			return signature + rtags(fraction());
		}

		public static string dns()
		{
			string[] flags = { "0000", "0100", "0120", "0020", "0110", "0010", "0130", "0030", "0140", "0040" };
			string[] domains = { "www.google.com", "www.cloudflare.com", "cdn.jsdelivr.net", "example.com", "amazon.com", "www.bing.com", "api.github.com", "images.unsplash.com", "news.ycombinator.com", "static.wikipedia.org", "firebase.google.com", "fonts.googleapis.com", "blog.reddit.com", "support.microsoft.com", "www.apple.com", "docs.docker.com", "s3.amazonaws.com", "api.spotify.com", "www.linkedin.com", "help.dropbox.com", "mail.yahoo.com", "status.slack.com", "downloads.opera.com", "metrics.twitch.tv", "www.hulu.com", "cloud.digitalocean.com", "cdnjs.com", "assets.medium.com", "graph.facebook.com", "api.twitter.com", "raw.githubusercontent.com", "example.org" };
			string[] types = { "0001", "001c", "000f", "0005", "0002", "0010" };
			var wire = new StringBuilder();
			foreach (string part in domains[random(0, domains.Length)].Split('.'))
			{
				wire.Append(part.Length.ToString("x2"));
				foreach (char ch in part) wire.Append(((int)ch).ToString("x2"));
			}
			wire.Append("000000291000000080000000" + types[random(0, types.Length)] + "0001");
			return "<b 0x" + hex(4) + flags[random(0, flags.Length)] + "0001000000000001" + wire + "><b 0x" + hex(4) + "><t>" + rtags(fraction());
		}
	}
}
