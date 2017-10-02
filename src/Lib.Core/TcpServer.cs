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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Xml;
using Eddie.Lib.Common;

namespace Eddie.Core
{
    public class TcpServer
    {
        // Thread signal.
        public ManualResetEvent AllDone = new ManualResetEvent(false);
        
        // Clients
        public List<StateObject> Clients = new List<StateObject>();

        public ManualResetEvent SignalConnection = new ManualResetEvent(false);

        private Socket m_listener;

        public void Start()
        {
            StartListening();

            Engine.Instance.CommandEvent += OnCommandEvent;            
        }

        public void Stop()
        {
            StopListening();

            CloseAll();
        }

        private void OnCommandEvent(XmlItem data)
        {
            lock (Clients)
            {
                CheckStillConnected();

                foreach (StateObject so in Clients)
                {
                    string x = data.ToString();
                    Send(so.workSocket, x + "\n");
                }
            }
        }

        public void CloseAll()
        {
            lock (Clients)
            {
                for (;;)
                {
                    try
                    {
                        foreach (StateObject so in Clients)
                        {
                            if (so.workSocket.Connected)
                            {
                                so.workSocket.Close();
                                continue;
                            }
                        }

                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void CheckStillConnected()
        {
            List<StateObject> Alive = new List<StateObject>();

            lock (Clients)
            {
                foreach (StateObject so in Clients)
                {
                    if (so.workSocket.Connected)
                        Alive.Add(so);
                    else
                        Console.Write("Socket dead.");
                }

                Clients = Alive;
            }
        }

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.                        
            IPAddress ipAddress = IPAddress.Loopback;
            string address = Engine.Instance.Storage.Get("tcpserver.ip");
            int port = Engine.Instance.Storage.GetInt("tcpserver.port");
            if (address == "localhost")
                ipAddress = IPAddress.Loopback;
            else if (address == "0.0.0.0")
                ipAddress = IPAddress.Any;
            else
            {
                IPAddress[] addresses = Dns.GetHostAddresses(address);
                if (addresses.Length > 0)
                    ipAddress = addresses[0];
                else
                {
                    Engine.Instance.Logs.Log(LogType.Warning, MessagesFormatter.Format(Messages.TcpServerNoBindAddress, address));
                }
            }
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                m_listener.Bind(localEndPoint);
                m_listener.Listen(100);

                //while (true)
                {
                    // Set the event to nonsignaled state.
                    AllDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    m_listener.BeginAccept(new AsyncCallback(AcceptCallback), m_listener);

                    // Wait until a connection is made before continuing.
                    //allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Engine.Instance.Logs.Log(e);
            }            
        }

        public void StopListening()
        {
            if (m_listener != null)
                m_listener.Close();
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            lock (Clients)
            {
                // Signal the main thread to continue.
                AllDone.Set();

                // Signal a connection
                SignalConnection.Set();

                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                // Create the state object.
                StateObject state = new StateObject(handler.ReceiveBufferSize);
                state.workSocket = handler;
                Clients.Add(state);
                handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallback), state);

                // Wait for other
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket
                // from the asynchronous state object.
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                }

                for (;;)
                {
                    string content = state.sb.ToString();
                    int posRN = content.IndexOf("\n");
                    if (posRN == -1)
                        break;

                    string line = content.Substring(0, posRN).Trim();
                    state.sb.Remove(0, posRN + 1);

                    Engine.Instance.Command(line, true);                    
                }

                handler.BeginReceive(state.buffer, 0, state.buffer.Length, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                Engine.Instance.Logs.Log(e);
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (Exception e)
            {
                Engine.Instance.Logs.Log(e);
            }
        }
    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Receive buffer.
        public byte[] buffer = null;
        // Received data string.
        public StringBuilder sb = new StringBuilder();

        public StateObject(int bufferSize)
        {
            buffer = new byte[bufferSize];
        }
    }
}