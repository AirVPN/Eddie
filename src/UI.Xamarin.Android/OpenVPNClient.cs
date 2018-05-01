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

namespace Eddie.Droid
{
	public class OpenVPNClient
	{
		private int m_handle;

		private OpenVPNClient(int handle)
		{
			m_handle = handle;
		}

		public static OpenVPNClient Create(ref NativeMethods.ovpn3_client ci)
		{
			int handle = NativeMethods.OVPN3.ClientCreate(ref ci);
			return NativeMethods.Succeeded(handle) ? new OpenVPNClient(handle) : null;
		}
	
		public int Handle
		{
			get
			{
				return m_handle;
			}
		}

		public bool GetTransportStats(ref NativeMethods.ovpn3_transport_stats stats)
		{
			return NativeMethods.Succeeded(NativeMethods.OVPN3.ClientGetTransportStats(Handle, ref stats));			
		}

		public bool LoadProfileFile(string filename)
		{
			return NativeMethods.Succeeded(NativeMethods.OVPN3.ClientLoadProfileFile(Handle, filename));
		}

		public bool LoadProfileString(string str)
		{
			return NativeMethods.Succeeded(NativeMethods.OVPN3.ClientLoadProfileString(Handle, str));
		}

		public bool SetOption(string option, string value)
		{
			return NativeMethods.Succeeded(NativeMethods.OVPN3.ClientSetOption(Handle, option, value));
		}

		public bool Start()
		{
			return NativeMethods.Succeeded(NativeMethods.OVPN3.ClientStart(Handle));
		}

		public bool Stop()
		{
			return NativeMethods.Succeeded(NativeMethods.OVPN3.ClientStop(Handle));
		}

		public bool Pause(string reason)
		{
			return NativeMethods.Succeeded(NativeMethods.OVPN3.ClientPause(Handle, reason));
		}

		public bool Resume()
		{
			return NativeMethods.Succeeded(NativeMethods.OVPN3.ClientResume(Handle));
		}
		
		public bool Destroy()
		{
			return NativeMethods.Succeeded(NativeMethods.OVPN3.ClientDestroy(Handle));
		}
	}
}
