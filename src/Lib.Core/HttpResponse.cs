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

using System.Collections.Generic;

namespace Eddie.Core
{
	public class HttpResponse
	{
		public string Status;
		public byte[] BufferData = default(byte[]);
		public List<KeyValuePair<string, string>> Headers = new List<KeyValuePair<string, string>>();
		public int HttpCode = 0;

		public void FromJson(Json jResponse)
		{
			if (jResponse.HasKey("headers"))
			{
				string[] lines = jResponse["headers"].ValueString.Split('\n');
				Status = "";
				foreach (string line in lines)
				{
					string l = line.Trim();
					if (l == "")
						continue;

					if (Status == "")
					{
						Status = l.Trim();
					}
					else
					{
						int posSep = l.IndexOfInv(":");
						if (posSep != -1)
						{
							string k = "";
							string v = "";
							k = l.Substring(0, posSep).ToLowerInvariant().Trim();
							v = l.Substring(posSep + 1).Trim();
							if (k != "")
								Headers.Add(new KeyValuePair<string, string>(k, v));
						}
					}
				}
			}

			if (jResponse.HasKey("body"))
			{
				BufferData = ExtensionsString.HexToBytes(jResponse["body"].ValueString);
			}

			if (jResponse.HasKey("response_code"))
				HttpCode = jResponse["response_code"].ValueInt;
		}

		public string GetBodyAscii()
		{
			return System.Text.Encoding.ASCII.GetString(BufferData);
		}

		public string GetBody()
		{
			return System.Text.Encoding.UTF8.GetString(BufferData);
		}

		public string GetHeader(string k)
		{
			foreach (KeyValuePair<string, string> kp in Headers)
			{
				if (k.ToLowerInvariant() == kp.Key)
					return kp.Value;
			}
			return "";
		}

		public string GetLineReport()
		{
			string t = "";
			t += "Status: " + Status.ToString();
			t += " - Headers: ";
			foreach (KeyValuePair<string, string> kp in Headers)
			{
				t += kp.Key + ":" + kp.Value + ";";
			}
			t += " - Body (" + BufferData.LongLength.ToString() + " bytes): ";
			t += System.Text.Encoding.ASCII.GetString(BufferData);
			return t;
		}
	}

}
