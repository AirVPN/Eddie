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

using System;

namespace Eddie.Core
{
	/// <summary>
	/// MCP (Model Context Protocol) server facade. Stateless JSON-RPC over the /mcp HTTP endpoint,
	/// exposing a small set of tools mapped onto existing UiManager/ApiManager commands.
	/// </summary>
	public static class Mcp
	{
		private const string ProtocolVersionDefault = "2025-06-18";

		// Dedicated, unregistered client: tool calls go through the normal command queue
		// (processed on the engine main thread) without receiving broadcasts.
		private static readonly UiClient m_client = new UiClient();

		/// <summary>
		/// Processes a single JSON-RPC message. Returns the response body (empty for notifications
		/// and errors handled via HTTP status). statusCode is the HTTP status to emit.
		/// </summary>
		public static string Handle(string body, out int statusCode)
		{
			statusCode = 200;

			Json request;
			try
			{
				request = Json.Parse(body);
			}
			catch (Exception)
			{
				statusCode = 400;
				return BuildError(null, -32700, "Parse error");
			}

			if ((request == null) || (request.IsDictionary() == false))
			{
				statusCode = 400;
				return BuildError(null, -32600, "Invalid Request");
			}

			string method = request["method"].Value as string;
			bool hasId = request.HasKey("id");
			object id = hasId ? request["id"].Value : null;

			// Notifications (no id) require no response body.
			if (hasId == false)
			{
				statusCode = 202;
				return "";
			}

			if (string.IsNullOrEmpty(method))
				return BuildError(id, -32600, "Invalid Request");

			if (method == "initialize")
				return BuildResult(id, BuildInitialize(request));
			else if (method == "tools/list")
				return BuildResult(id, BuildToolsList());
			else if (method == "tools/call")
				return BuildResult(id, BuildToolsCall(request));
			else if (method == "ping")
				return BuildResult(id, EmptyObject());

			return BuildError(id, -32601, "Method not found: " + method);
		}

		#region JSON-RPC envelope

		private static string BuildResult(object id, Json result)
		{
			Json resp = new Json();
			resp["jsonrpc"].Value = "2.0";
			resp["id"].Value = id;
			resp["result"].Value = result;
			return resp.ToJson();
		}

		private static string BuildError(object id, int code, string message)
		{
			Json resp = new Json();
			resp["jsonrpc"].Value = "2.0";
			resp["id"].Value = id;
			Json err = new Json();
			err["code"].Value = code;
			err["message"].Value = message;
			resp["error"].Value = err;
			return resp.ToJson();
		}

		private static Json EmptyObject()
		{
			Json o = new Json();
			o.EnsureDictionary();
			return o;
		}

		#endregion

		#region Handshake

		private static Json BuildInitialize(Json request)
		{
			Json result = new Json();

			string requestedVersion = null;
			Json p = request["params"].Json;
			if (p != null)
				requestedVersion = p["protocolVersion"].Value as string;
			result["protocolVersion"].Value = string.IsNullOrEmpty(requestedVersion) ? ProtocolVersionDefault : requestedVersion;

			Json capabilities = new Json();
			capabilities["tools"].Value = EmptyObject();
			result["capabilities"].Value = capabilities;

			Json serverInfo = new Json();
			serverInfo["name"].Value = "Eddie";
			serverInfo["version"].Value = Constants.VersionDesc;
			result["serverInfo"].Value = serverInfo;

			return result;
		}

		#endregion

		#region Tools listing

		private static Json BuildToolsList()
		{
			Json tools = new Json();
			tools.EnsureArray();

			tools.Append(Tool("status",
				"Return the current Eddie VPN status and statistics (connection state, server, exit IP, traffic).",
				null));

			{
				Json props = new Json();
				props.EnsureDictionary();
				props["country"].Value = Prop("string", "Optional ISO 3166-1 country code (e.g. NL, FR) to filter servers by country.");
				props["name"].Value = Prop("string", "Optional server name to filter by (case-insensitive substring match, e.g. 'Phaet').");
				props["limit"].Value = Prop("integer", "Optional maximum number of servers to return (default 20). 'count' always reflects the total number of matches; 'truncated' is true when there are more matches than returned.");
				tools.Append(Tool("servers_query",
					"List available VPN servers, optionally filtered by country and/or name. Returns the total match 'count' and a capped 'servers' list. Each server includes: code, name, country_code, location, load (percent, or -1 if n/a), users (current connected users, or -1 if n/a), ping (ms, or -1 if n/a), can_connect, connected.",
					props));
			}

			{
				Json props = new Json();
				props.EnsureDictionary();
				props["country"].Value = Prop("string", "Optional ISO 3166-1 country code to connect to the best eligible server in that country.");
				props["code"].Value = Prop("string", "Optional explicit server code (from servers_query) to connect to. Takes precedence over country.");
				tools.Append(Tool("connect",
					"Start a VPN connection. With 'code' connects to that specific server; with 'country' connects to the best eligible server in that country; with no arguments connects to the best eligible server overall. Asynchronous: returns immediately with state 'connecting' (call status afterwards to check progress). Requires mcp.allow_control.",
					props));
			}

			tools.Append(Tool("disconnect",
				"Disconnect the current VPN connection. Requires mcp.allow_control.",
				null));

			{
				Json props = new Json();
				props.EnsureDictionary();
				props["enabled"].Value = Prop("boolean", "true to activate the network lock, false to deactivate it.");
				Json tool = Tool("network_lock_set",
					"Activate or deactivate the network lock (kill switch). Requires mcp.allow_control.",
					props);
				Json schema = tool["inputSchema"].Json;
				Json required = new Json();
				required.EnsureArray();
				required.Append("enabled");
				schema["required"].Value = required;
				tools.Append(tool);
			}

			Json result = new Json();
			result["tools"].Value = tools;
			return result;
		}

		private static Json Tool(string name, string description, Json properties)
		{
			Json tool = new Json();
			tool["name"].Value = name;
			tool["description"].Value = description;

			Json schema = new Json();
			schema["type"].Value = "object";
			if (properties == null)
				properties = EmptyObject();
			schema["properties"].Value = properties;
			tool["inputSchema"].Value = schema;

			return tool;
		}

		private static Json Prop(string type, string description)
		{
			Json p = new Json();
			p["type"].Value = type;
			p["description"].Value = description;
			return p;
		}

		#endregion

		#region Tools invocation

		private static Json BuildToolsCall(Json request)
		{
			Json p = request["params"].Json;
			string name = (p != null) ? (p["name"].Value as string) : null;
			Json args = (p != null) ? p["arguments"].Json : null;
			if (args == null)
				args = EmptyObject();

			if (string.IsNullOrEmpty(name))
				return ToolResult(TextError("Missing tool name"), true);

			if (name == "status")
				return ToolStatus();
			else if (name == "servers_query")
				return ToolServersQuery(args);
			else if (name == "connect")
				return ToolConnect(args);
			else if (name == "disconnect")
				return ToolDisconnect();
			else if (name == "network_lock_set")
				return ToolNetworkLockSet(args);

			return ToolResult(TextError("Unknown tool: " + name), true);
		}

		private static Json ToolStatus()
		{
			Json cmd = new Json();
			cmd["command"].Value = "stats.list";
			Json resp = m_client.Command(cmd);

			Json outp = new Json();
			outp["stats"].Value = (resp != null) ? resp["list"].Value : null;
			return ToolResult(outp, false);
		}

		private static Json ToolServersQuery(Json args)
		{
			string country = args["country"].Value as string;
			string name = args["name"].Value as string;
			int limit = args.HasKey("limit") ? args["limit"].ValueInt : 20;
			if (limit <= 0)
				limit = 20;

			Json cmd = new Json();
			cmd["command"].Value = "servers.list";
			cmd["all"].Value = true;
			Json resp = m_client.Command(cmd);

			Json servers = (resp != null) ? resp["servers"].Json : null;

			Json arr = new Json();
			arr.EnsureArray();
			int count = 0;
			if ((servers != null) && (servers.IsArray()))
			{
				foreach (object o in servers.GetArray())
				{
					Json s = o as Json;
					if (s == null)
						continue;

					if (string.IsNullOrEmpty(country) == false)
					{
						string cc = s["country_code"].Value as string;
						if ((cc == null) || (string.Equals(cc, country, StringComparison.OrdinalIgnoreCase) == false))
							continue;
					}

					if (string.IsNullOrEmpty(name) == false)
					{
						string sn = s["name"].Value as string;
						if ((sn == null) || (sn.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0))
							continue;
					}

					count++;
					if (count <= limit)
					{
						Json item = new Json();
						item["code"].Value = s["code"].Value;
						item["name"].Value = s["name"].Value;
						item["country_code"].Value = s["country_code"].Value;
						item["location"].Value = s["location"].Value;
						item["load"].Value = s["load"].Value;
						item["users"].Value = s["users"].Value;
						item["ping"].Value = s["ping"].Value;
						item["can_connect"].Value = s["can_connect"].Value;
						item["connected"].Value = s["connected"].Value;
						arr.Append(item);
					}
				}
			}

			Json outp = new Json();
			outp["count"].Value = count;
			if (string.IsNullOrEmpty(country) == false)
				outp["country"].Value = country.ToUpperInvariant();
			if (string.IsNullOrEmpty(name) == false)
				outp["name"].Value = name;
			outp["truncated"].Value = (count > limit);
			outp["servers"].Value = arr;
			return ToolResult(outp, false);
		}

		private static Json ToolConnect(Json args)
		{
			if (ControlAllowed() == false)
				return ToolResult(TextError("Control is disabled. Enable option 'mcp.allow_control' to allow this action."), true);

			string code = args["code"].Value as string;
			string country = args["country"].Value as string;

			if (string.IsNullOrEmpty(code) && (string.IsNullOrEmpty(country) == false))
			{
				code = PickBestServerCode(country);
				if (code == null)
					return ToolResult(TextError("No eligible server found in country '" + country + "'."), true);
			}

			Json cmd = new Json();
			if (string.IsNullOrEmpty(code) == false)
			{
				cmd["command"].Value = "servers.connect";
				cmd["code"].Value = code;
			}
			else
			{
				cmd["command"].Value = "mainaction.connect";
			}
			m_client.Command(cmd);

			Json outp = new Json();
			outp["accepted"].Value = true;
			outp["state"].Value = "connecting";
			if (string.IsNullOrEmpty(code) == false)
				outp["code"].Value = code;
			return ToolResult(outp, false);
		}

		private static Json ToolDisconnect()
		{
			if (ControlAllowed() == false)
				return ToolResult(TextError("Control is disabled. Enable option 'mcp.allow_control' to allow this action."), true);

			Json cmd = new Json();
			cmd["command"].Value = "mainaction.disconnect";
			m_client.Command(cmd);

			Json outp = new Json();
			outp["accepted"].Value = true;
			return ToolResult(outp, false);
		}

		private static Json ToolNetworkLockSet(Json args)
		{
			if (ControlAllowed() == false)
				return ToolResult(TextError("Control is disabled. Enable option 'mcp.allow_control' to allow this action."), true);

			if (args.HasKey("enabled") == false)
				return ToolResult(TextError("Missing required argument 'enabled'."), true);

			bool enabled = args["enabled"].ValueBool;

			Json cmd = new Json();
			cmd["command"].Value = enabled ? "netlock.activate" : "netlock.deactivate";
			m_client.Command(cmd);

			Json outp = new Json();
			outp["enabled"].Value = enabled;
			return ToolResult(outp, false);
		}

		// Mirrors Eddie's own ranking: GetConnections(false) is sorted best-first, so the first
		// eligible match in the requested country is the server Eddie would pick.
		private static string PickBestServerCode(string country)
		{
			Json cmd = new Json();
			cmd["command"].Value = "servers.list";
			cmd["all"].Value = false;
			Json resp = m_client.Command(cmd);
			if (resp == null)
				return null;

			Json servers = resp["servers"].Json;
			if ((servers == null) || (servers.IsArray() == false))
				return null;

			foreach (object o in servers.GetArray())
			{
				Json s = o as Json;
				if (s == null)
					continue;

				string cc = s["country_code"].Value as string;
				if ((cc == null) || (string.Equals(cc, country, StringComparison.OrdinalIgnoreCase) == false))
					continue;

				if (Conversions.ToBool(s["can_connect"].Value) == false)
					continue;

				return s["code"].Value as string;
			}

			return null;
		}

		private static bool ControlAllowed()
		{
			return Engine.Instance.ProfileOptions.GetBool("mcp.allow_control");
		}

		private static Json ToolResult(Json structured, bool isError)
		{
			Json result = new Json();

			Json content = new Json();
			content.EnsureArray();
			Json textItem = new Json();
			textItem["type"].Value = "text";
			textItem["text"].Value = structured.ToJson();
			content.Append(textItem);

			result["content"].Value = content;
			result["isError"].Value = isError;
			return result;
		}

		private static Json TextError(string message)
		{
			Json o = new Json();
			o["error"].Value = message;
			return o;
		}

		#endregion
	}
}
