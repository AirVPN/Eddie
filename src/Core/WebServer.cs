// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
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

#if (EDDIE3)
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Xml;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace Eddie.Core
{
    public class WebServer
    {
		private static HttpServer m_server;

        public static string GetPath()
        {
            string pathRoot = "";
            if (Engine.Instance.DevelopmentEnvironment)
                pathRoot = Platform.Instance.GetProjectPath();
            else
                pathRoot = Platform.Instance.GetProgramFolder();
            pathRoot += "//webui//";
            if (System.IO.Directory.Exists(pathRoot))
                return pathRoot;
            else
                return "";
        }

		public static void Start()
		{
            if (GetPath() == "")
                return;

			/* Create a new instance of the HttpServer class.
       *
       * If you would like to provide the secure connection, you should create the instance
       * with the 'secure' parameter set to true.
       */
			if (Engine.Instance.Storage.GetBool("webui.enabled") == false)
				return;

			m_server = new HttpServer(Engine.Instance.Storage.GetInt("webui.port"), false);



			//httpsv = new HttpServer (4649, true);
#if DEBUG
			// To change the logging level.
			m_server.Log.Level = LogLevel.Trace;

			// To change the wait time for the response to the WebSocket Ping or Close.
			m_server.WaitTime = TimeSpan.FromSeconds(2);
#endif
            /*
      var cert = ConfigurationManager.AppSettings["ServerCertFile"];
      var passwd = ConfigurationManager.AppSettings["CertFilePassword"];
      httpsv.SslConfiguration.ServerCertificate = new X509Certificate2 (cert, passwd);
	  */

            /* To provide the HTTP Authentication (Basic/Digest).
			httpsv.AuthenticationSchemes = AuthenticationSchemes.Basic;
			httpsv.Realm = "WebSocket Test";
			httpsv.UserCredentialsFinder = id => {
			  var name = id.Name;

			  // Return user name, password, and roles.
			  return name == "nobita"
					 ? new NetworkCredential (name, "password", "gunfighter")
					 : null; // If the user credentials aren't found.
			};
			 */
             
            m_server.RootPath = GetPath(); 

			// To set the HTTP GET method event.
			m_server.OnGet += new EventHandler<HttpRequestEventArgs>(OnGet);
			

			// Not to remove the inactive WebSocket sessions periodically.
			//httpsv.KeepClean = false;

			// To resolve to wait for socket in TIME_WAIT state.
			//httpsv.ReuseAddress = true;

			// Add the WebSocket services.
			m_server.AddWebSocketService<WebSocketService>("/eddie");

			
			//httpsv.AddWebSocketService<Chat> ("/Chat");

			/* Add the WebSocket service with initializing.
			httpsv.AddWebSocketService<Chat> (
			  "/Chat",
			  () => new Chat ("Anon#") {
				Protocol = "chat",
				// To validate the Origin header.
				OriginValidator = val => {
				  // Check the value of the Origin header, and return true if valid.
				  Uri origin;
				  return !val.IsNullOrEmpty () &&
						 Uri.TryCreate (val, UriKind.Absolute, out origin) &&
						 origin.Host == "localhost";
				},
				// To validate the Cookies.
				CookiesValidator = (req, res) => {
				  // Check the Cookies in 'req', and set the Cookies to send to the client with 'res'
				  // if necessary.
				  foreach (Cookie cookie in req) {
					cookie.Expired = true;
					res.Add (cookie);
				  }

				  return true; // If valid.
				}
			  });
			 */

			m_server.Start();
			if (m_server.IsListening)
			{
				Console.WriteLine("Listening on port {0}, and providing WebSocket services:", m_server.Port);
				foreach (string path in m_server.WebSocketServices.Paths)
					Console.WriteLine("- {0}", path);
			}



			Engine.Instance.Logs.LogEvent += new LogsManager.LogEventHandler(LogsLogEvent);
		}

		static void OnGet(object sender, HttpRequestEventArgs e)
		{
			
			HttpListenerRequest req = e.Request;
			HttpListenerResponse res = e.Response;

            string path = req.Url.LocalPath;
            if (path == "/")
				path += "index.html";

			byte[] content = m_server.GetFile(path);
			if (content == null)
			{
				res.StatusCode = (int)HttpStatusCode.NotFound;
				return;
			}

			if (path.EndsWith(".html"))
			{
				res.ContentType = "text/html";
				res.ContentEncoding = Encoding.UTF8;
			}

			if (path.EndsWith(".css"))
			{
				res.ContentType = "text/css";
				res.ContentEncoding = Encoding.UTF8;
			}

			if (path.EndsWith(".js"))
			{
				res.ContentType = "text/javascript";
				res.ContentEncoding = Encoding.UTF8;
			}

			if (path.EndsWith(".png"))
			{
				res.ContentType = "image/png";
			}

			if (path.EndsWith(".gif"))
			{
				res.ContentType = "image/gif";
			}
			
			res.WriteContent(content);
		}

		public static void Stop()
		{
			if (m_server != null)
			{
				Engine.Instance.Logs.LogEvent -= new LogsManager.LogEventHandler(LogsLogEvent);

				m_server.Stop();
			}
		}

		public static WebSocketSessionManager Sessions
		{
			get
			{
				return m_server.WebSocketServices["/eddie"].Sessions;
			}
		}

		public static void Send(XmlElement nodeMessage)
		{
            if (m_server != null)
                Sessions.Broadcast(nodeMessage.OwnerDocument.OuterXml);
		}

		public static XmlElement CreateMessage(string type)
		{
			XmlDocument doc = new XmlDocument();
			XmlElement nodeRoot = doc.CreateElement("message");
			doc.AppendChild(nodeRoot);
			Utils.XmlSetAttributeString(nodeRoot, "type", type);
			XmlElement nodeData = doc.CreateElement("data");
			nodeRoot.AppendChild(nodeData);
			return nodeData;
		}

		public static void OnMessage(MessageEventArgs e)
		{
			if (e.Data == "connect")
			{
				ConnectEvent();
			}
            else
            {
                Engine.Instance.Command(e.Data);
            }
		}

        public static void OnCommand(CommandLine cmd)
        {
            if (m_server == null)
                return;

            XmlElement xmlCommand = CreateMessage(cmd.Get("action","Unknown"));
            cmd.WriteXml(xmlCommand);
            Send(xmlCommand);
        }

		static void LogsLogEvent(LogEntry e)
		{
			XmlElement result = CreateMessage("log");
			e.WriteXML(result);
			Send(result);
		}

		static void ConnectEvent()
		{
			XmlElement result = CreateMessage("connect");

			Send(result);
		}
    }
}

#endif