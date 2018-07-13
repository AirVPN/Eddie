// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2018 AirVPN (support@airvpn.org) / https://airvpn.org
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
//
// 20 June 2018 - author: promind - initial release. Based on revised code from com.eddie.android. (a tribute to the 1859 Perugia uprising occurred on 20 June 1859 and in memory of those brave inhabitants who fought for the liberty of Perugia)

using Eddie.Common.Log;
using System;
using System.Threading;

namespace Eddie.NativeAndroidApp
{
	public class OpenVPNDispatcher : IRunnable
	{
		//private static int MAX_PACKET_SIZE = short.MaxValue;
		private static int THREAD_DELAY = 1000;

		private OpenVPNTunnel m_tunnel = null;
		private CancellationToken m_cancellationToken;

		public OpenVPNDispatcher(OpenVPNTunnel tunnel, CancellationToken cancellationToken)
		{
			m_tunnel = tunnel;
			m_cancellationToken = cancellationToken;
		}

		public VPNService Service
		{
			get
			{
				return m_tunnel.Service;
			}
		}

		public void Run()
		{
			try
			{
				DoUpdate();
				Service.HandleThreadStopped();
			}
			catch(Exception e)
			{
				Service.HandleThreadException(e);
			}			
		}
		/*
		private ParcelFileDescriptor WaitTun()
		{
			while(!m_cancellationToken.IsCancellationRequested)
			{
				ParcelFileDescriptor tun = m_channel.FileDescriptor;
				if(tun != null)
					return tun;

				Utils.Sleep(THREAD_DELAY);
			}

			return null;
		}
		*/	
		private void DoUpdate()
		{
			LogsManager.Instance.Debug("DoUpdate - Begin");

			while(!m_cancellationToken.IsCancellationRequested)
			{
				SupportTools.Sleep(THREAD_DELAY);

				NativeMethods.ovpn3_transport_stats stats = m_tunnel.GetTransportStats();
				//	LogsManager.Instance.Debug("Tunnel stats: bytes_in={0}, bytes_out={1}, packets_in={2}, packets_out={3}, last_packet_received={4}", stats.bytes_in, stats.bytes_out, stats.packets_in, stats.packets_out, stats.last_packet_received);
			}

			LogsManager.Instance.Debug("DoUpdate - End");

			/*
			FileInputStream channelIn = null;
			FileOutputStream channelOut = null;
			FileInputStream tunnelIn = null;
			FileOutputStream tunnelOut = null;
			
			try
			{
				string inError = null;
				string outError = null;
			
				channelIn = new FileInputStream(m_channelFileDescriptor.FileDescriptor);
				channelOut = new FileOutputStream(m_channelFileDescriptor.FileDescriptor);

				tunnelIn = new FileInputStream(m_tunnelFileDescriptor.FileDescriptor);
				tunnelOut = new FileOutputStream(m_tunnelFileDescriptor.FileDescriptor);

				TaskEx taskIn = Service.TasksManager.Add((CancellationToken c) =>
				{
					ReadPackets(channelIn, tunnelOut, c, out inError);
				});

				TaskEx taskOut = Service.TasksManager.Add((CancellationToken c) =>
				{
					WritePackets(tunnelIn, channelOut, c, out outError);
				});

				bool loop = true;

				while(loop)
				{
					Utils.Sleep(THREAD_DELAY);

					// TODO: update stats

					if(taskIn.IsCompleted || taskOut.IsCompleted || m_cancellationToken.IsCancellationRequested)
						loop = false;					
				}
		
				LogsManager.Instance.Debug("ExchangeData2");

				taskIn.Cancel();
				taskIn.Wait();

				LogsManager.Instance.Debug("ExchangeData3");

				taskOut.Cancel();
				taskOut.Wait();

				LogsManager.Instance.Debug("ExchangeData4");

				if(Utils.Empty(inError) == false)
					throw new Exception(inError);

				if(Utils.Empty(outError) == false)
					throw new Exception(outError);
			}
			finally
			{
				Utils.SafeDispose(channelIn);
				Utils.SafeDispose(channelOut);

				Utils.SafeDispose(tunnelIn);
				Utils.SafeDispose(tunnelOut);				
			}
	
			LogsManager.Instance.Debug("ExchangeData5");
			*/

		}
		/*
		private void ReadPackets(InputStream channelIn, OutputStream tunnelOut, CancellationToken c, out string error)
		{
			error = null;

			LogsManager.Instance.Debug("ReadPackets start");

			try
			{
				ReadPacketsImpl(channelIn, tunnelOut, c, ref error);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("ReadPackets exception: {0}", e.Message);
				error = e.Message;
			}

			LogsManager.Instance.Debug("ReadPackets end");
		}
	
		private void ReadPacketsImpl(InputStream channelIn, OutputStream tunnelOut, CancellationToken c, ref string error)
		{
			byte[] packet = new byte[MAX_PACKET_SIZE];
			
			while(!c.IsCancellationRequested)
			{
				LogsManager.Instance.Debug("channelIn.Read BEGIN");
				int length = channelIn.Read(packet);
				LogsManager.Instance.Debug("channelIn.Read END");

				if(length > 0)
				{
					LogsManager.Instance.Debug("channelIn.Read({0})", length);

					tunnelOut.Write(packet, 0, length);					
				}
				else if(length < 0)
				{
					error = "TUN read error";
					break;
				}
				else
				{
					// TODO: temp
					Utils.Sleep(1000);
					LogsManager.Instance.Debug("channelIn.Read SLEEP");
				}
			}
		}

		private void WritePackets(InputStream tunnelIn, OutputStream channelOut, CancellationToken c, out string error)
		{
			error = null;

			LogsManager.Instance.Debug("WritePackets start");

			try
			{
				WritePacketsImpl(tunnelIn, channelOut, c, ref error);
			}
			catch(Exception e)
			{
				LogsManager.Instance.Error("WritePackets exception: {0}", e.Message);
				error = e.Message;
			}

			LogsManager.Instance.Debug("WritePackets end");
		}
	
		private void WritePacketsImpl(InputStream tunnelIn, OutputStream channelOut, CancellationToken c, ref string error)
		{
			byte[] packet = new byte[MAX_PACKET_SIZE];

			while(!c.IsCancellationRequested)
			{
				LogsManager.Instance.Debug("tunnelIn.Read BEGIN");
				int length = tunnelIn.Read(packet);
				LogsManager.Instance.Debug("tunnelIn.Read BEGIN");

				if(length > 0)
				{
					LogsManager.Instance.Debug("tunnelIn.Read({0})", length);

					channelOut.Write(packet, 0, length);
				}
				else if(length < 0)
				{
					error = "channel read error";
					break;
				}
				else
				{
					// TODO: temp
					Utils.Sleep(1000);
					LogsManager.Instance.Debug("tunnelIn.Read SLEEP");
				}
			}
		}
		*/
	}
}
