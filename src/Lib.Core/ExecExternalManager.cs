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

namespace Eddie.Core
{
	public class ExecExternalManager
	{
		public static bool CheckAllow(string path)
		{
			return true; // TOFIX, for next alpha version
			/*
			bool result = AllowPath(path);
			if (result == false)
				Engine.Instance.Logs.Log(LogType.Error, "Path " + path + " denied.");
			return result;
			*/
		}

		private static bool AllowPathX(string path)
		{
			string sha256 = "";
			string signId = "";

			if (Platform.Instance.FileExists(path) == false)
				return false;

			List<string> trustedPaths = Platform.Instance.GetTrustedPaths();
			foreach (string trustedPath in trustedPaths)
			{
				if (path.StartsWith(trustedPath, StringComparison.InvariantCulture))
					return true;
			}

			// Avoid if possible any exec before the storage init.
			if (Engine.Instance.Storage == null)
				return false;

			Json rulesCustom = Engine.Instance.Options.GetJson("external.rules");
			for (int r = 0; r < 2; r++)
			{
				Json rules = null;
				if (r == 0)
				{
					if (Engine.Instance.Options.GetBool("external.rules.recommended"))
						rules = Engine.Instance.Manifest["external-rules-recommended"].Value as Json;
					else
						continue;
				}
				else if (r == 1)
					rules = rulesCustom;

				foreach (Json rule in rules.GetArray())
				{
					string type = rule["type"].Value as string;
					if (type == "all")
						return true;
					if (type == "sign")
					{
						if (signId == "")
							signId = Platform.Instance.FileGetSignedId(path);

						if (rule["id"].Value as string == signId)
							return true;
					}

					if (type == "sha256")
					{
						if (sha256 == "")
							sha256 = Crypto.Manager.HashSHA256File(path);

						if (rule["hash"].Value as string == sha256)
							return true;
					}
					if (type == "path")
					{
						if (rule["path"].Value as string == path)
							return true;
					}
				}
			}

			// Ensure compute, Report and result
			if (signId == "")
				signId = Platform.Instance.FileGetSignedId(path);
			if (sha256 == "")
				sha256 = Crypto.Manager.HashSHA256File(path);

			Json askToUi = new Json();
			askToUi["sha256"].Value = sha256;
			askToUi["sign-id"].Value = signId;
			askToUi["path"].Value = path;

			// Propose to add rule to UI
			Json replyUi = Engine.Instance.OnAskExecExternalPermission(askToUi);

			if (replyUi.HasKey("allow"))
				if (Convert.ToBoolean(replyUi["allow"].Value) == false)
					return false;

			if (replyUi.HasKey("type"))
			{
				replyUi.RemoveKey("allow");
				rulesCustom.Append(replyUi);
				Engine.Instance.Options.SetJson("external.rules", rulesCustom);

				return AllowPathX(path);
			}

			if (replyUi.HasKey("allow"))
				if (Convert.ToBoolean(replyUi["allow"].Value) == true)
					return true;

			//Engine.Instance.Storage.SetJson("external.rules", rules);

			return false;

		}
	}

}
