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
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using System.Xml;
using Eddie.Common;

// ClodoTemp: Missing feature, a token access

namespace Eddie.Core
{
	public class WebServer
	{
		public string ListenUrl;

		private HttpListener m_listener = new HttpListener();

		private List<Json> m_pullItems = new List<Json>();

		private WebServerClient m_client = new WebServerClient();

		public static string GetPath()
		{
			string pathRoot = Platform.Instance.NormalizePath(Engine.Instance.LocateResource("webui"));
			if (Platform.Instance.DirectoryExists(pathRoot))
				return pathRoot;
			else
				return "";
		}

		public void Init(string prefix)
		{
			Engine.Instance.UiManager.Add(m_client);

			if (GetPath() == "")
				return;

			if (!HttpListener.IsSupported)
				throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

			m_listener.Prefixes.Add(prefix);

			m_listener.Start();
		}

		public void Run()
		{
			ThreadPool.QueueUserWorkItem((o) =>
			{
				try
				{
					while (m_listener.IsListening)
					{
						ThreadPool.QueueUserWorkItem((c) =>
						{
							var context = c as HttpListenerContext;
							try
							{
								SendResponse(context);
							}
							catch 
							{
								context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
							}
							finally
							{
								context.Response.OutputStream.Close();
							}
						}, m_listener.GetContext());
					}
				}
				catch 
				{ 
				}
			});
		}

		void WriteFile(HttpListenerContext ctx, string path, bool asDownload)
		{
			var response = ctx.Response;
			using (FileStream fs = File.OpenRead(path))
			{
				string filename = Path.GetFileName(path);
				//response is HttpListenerContext.Response...
				response.ContentLength64 = fs.Length;
				response.SendChunked = false;
				response.AddHeader("Last-Modified", File.GetLastWriteTime(path).ToString("r"));
				if (asDownload)
				{
					response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
					response.AddHeader("Content-disposition", "attachment; filename=" + filename);
				}
				else
				{
					string mime = "";
					int posLastDot = path.LastIndexOf('.');
					if (posLastDot != -1)
					{
						string ext = path.Substring(posLastDot + 1);
						if (Engine.Instance.Manifest["mime_types"]["extension_to_type"].Json.HasKey(ext))
							mime = Engine.Instance.Manifest["mime_types"]["extension_to_type"][ext].Value as string;
						else
							mime = Engine.Instance.Manifest["mime_types"]["extension_to_type"]["*"].Value as string;
					}

					if (mime != "")
						response.ContentType = mime;

					if( (mime.StartsWith("text/")) || (mime == "application/javascript") )
						response.ContentEncoding = Encoding.UTF8;
				}

				byte[] buffer = new byte[64 * 1024];
				int read;
				using (BinaryWriter bw = new BinaryWriter(response.OutputStream))
				{
					while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
					{
						bw.Write(buffer, 0, read);
						bw.Flush(); //seems to have no effect
					}

					bw.Close();
				}
								
				response.StatusCode = (int)HttpStatusCode.OK;
				response.StatusDescription = "OK";
				response.OutputStream.Close();
			}
		}

		public void Stop()
		{
			m_listener.Stop();
			m_listener.Close();
		}

		public void Start()
		{
			ListenUrl = "http://" + Engine.Instance.Storage.Get("webui.ip") + ":" + Engine.Instance.Storage.Get("webui.port");
			Init(ListenUrl + "/");
			Run();
		}

		public void SendResponse(HttpListenerContext context)
		{
			// string physicalPath = GetPath() + request.RawUrl;
			string bodyResponse = ""; // If valorized, always a dynamic response
			Dictionary<string, string> requestHeaders = new Dictionary<string, string>();
			foreach (var key in context.Request.Headers.AllKeys)
				requestHeaders[key.ToLowerInvariant()] = context.Request.Headers[key];
			string requestHttpMethod = context.Request.HttpMethod.ToLowerInvariant().Trim();

			context.Response.Headers["Server"] = Constants.Name + " " + Constants.VersionShow;
			context.Response.Headers["Access-Control-Allow-Origin"] = ListenUrl;
			context.Response.Headers["Vary"] = "Origin";

			string origin = context.Request.Headers["Origin"];
			if( (requestHeaders.ContainsKey("origin")) && (requestHeaders["origin"].StartsWith(ListenUrl) == false))
			{
				List<string> hostsAllowed = new List<string>(); // Option?
				hostsAllowed.Add("127.0.0.1");
				hostsAllowed.Add("localhost");
				Uri uriOrigin = new Uri(requestHeaders["origin"]);
				if (hostsAllowed.Contains(uriOrigin.Host) == false)
				{
					context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
					return;
				}
			}

			if (requestHttpMethod == "options")
			{				
				Engine.Instance.Logs.LogVerbose(origin);
				context.Response.StatusCode = (int)HttpStatusCode.NoContent;
			}

			if(context.Request.Url.AbsolutePath == "/api/command/")
			{
				if (requestHttpMethod == "post")
				{
					// Pull mode
					var data = new StreamReader(context.Request.InputStream).ReadToEnd();
					Json ret = Receive(data);
					if (ret != null)
						bodyResponse = ret.ToJson();
					else
						bodyResponse = "null";
				}
				else
				{
					context.Response.StatusCode = (int)HttpStatusCode.NoContent;
				}
			}
			else if (context.Request.Url.AbsolutePath == "/api/pull/")
			{
				if (requestHttpMethod == "post")
				{
					lock (m_client.Pendings)
					{
						if (m_client.Pendings.Count == 0)
						{
							bodyResponse = "null";
						}
						else
						{
							Json data = m_client.Pendings[0];
							m_client.Pendings.RemoveAt(0);
							bodyResponse = data.ToJson();
						}
					}
				}
				else
				{
					context.Response.StatusCode = (int)HttpStatusCode.NoContent;
				}
			}
			else
			{
				string urlPath = context.Request.Url.LocalPath;
				if (urlPath == "/")
					urlPath = "/index.html";
				string localPath = GetPath() + urlPath;
				if (Platform.Instance.FileExists(localPath))
				{
					if (context.Request.HttpMethod == "GET")
						WriteFile(context, localPath, false);
					else
						throw new Exception("Unexpected");
				}
				else
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				}
			}

			if(bodyResponse != "") // Always dynamic
			{
				context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
				context.Response.Headers["Pragma"] = "no-cache";

				byte[] buf = Encoding.UTF8.GetBytes(bodyResponse);
				context.Response.ContentLength64 = buf.Length;
				context.Response.OutputStream.Write(buf, 0, buf.Length);
			}
		}

		public Json Receive(string data)
		{
			Json jData = Json.Parse(data);
			return m_client.Command(jData["data"].Value as Json);
		}
	}
}