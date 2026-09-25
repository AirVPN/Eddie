// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2026 AirVPN (support@airvpn.org) / https://airvpn.org
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
	public class WebserverClient : Core.UiClient
	{
		public List<Json> Pendings = new List<Json>();

		public override void OnReceive(Json data)
		{
			base.OnReceive(data);

			lock (Pendings)
				Pendings.Add(Redact(data));
		}

		// The browser is an untrusted client: blank the value of any option flagged as Secret
		// before it reaches the pull queue. The broadcast Json is shared across clients, so a
		// clone is required to avoid altering the copy delivered to trusted (embedded) clients.
		private Json Redact(Json data)
		{
			if (data == null)
				return data;

			string command = data["command"].Value as string;

			if (command == "ui.boot")
			{
				Json redacted = data.Clone();
				Json options = redacted["options"].Json;
				if ((options != null) && (options.IsDictionary()))
				{
					foreach (string name in new List<string>(options.GetDictionary().Keys))
					{
						JsonValue option = options[name];
						if (Conversions.ToBool(option["secret"].Value))
							option["value"].Value = "";
					}
				}
				return redacted;
			}
			else if (command == "option.change")
			{
				string name = data["name"].Value as string;
				ProfileOption option = (name != null) ? Engine.Instance.ProfileOptions.GetOption(name) : null;
				if ((option != null) && option.Secret)
				{
					Json redacted = data.Clone();
					redacted["value"].Value = "";
					return redacted;
				}
			}

			return data;
		}
	}
}