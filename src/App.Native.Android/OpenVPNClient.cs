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

namespace Eddie.NativeAndroidApp
{
	public class OpenVPNClient
	{
		private OpenVPNClient()
		{
		}

		public static OpenVPNClient Create(ref NativeMethods.ovpn3_client ci)
		{
			NativeMethods.EddieLibraryResult result = NativeMethods.EddieLibrary.CreateClient(ref ci);

            if(result.code == NativeMethods.ResultCode.SUCCESS)
                return new OpenVPNClient();
            else
            {
                EddieLogger.Error("Eddie Library: Failed to create a new OpenVPN Client. {0}", result.description);

                return null;
            }
		}

		public NativeMethods.EddieLibraryResult GetTransportStats(ref NativeMethods.ovpn3_transport_stats stats)
		{
			return NativeMethods.EddieLibrary.GetClientTransportStats(ref stats);
		}

		public NativeMethods.EddieLibraryResult LoadProfileFile(string filename)
		{
			return NativeMethods.EddieLibrary.LoadProfileToClient(filename);
		}

		public NativeMethods.EddieLibraryResult LoadProfileString(string str)
		{
			return NativeMethods.EddieLibrary.LoadStringProfileToClient(str);
		}

		public NativeMethods.EddieLibraryResult SetOption(string option, string value)
		{
			return NativeMethods.EddieLibrary.SetClientOption(option, value);
		}

		public NativeMethods.EddieLibraryResult Start()
		{
			return NativeMethods.EddieLibrary.StartClient();
		}

		public NativeMethods.EddieLibraryResult Stop()
		{
			return NativeMethods.EddieLibrary.StopClient();
		}

		public NativeMethods.EddieLibraryResult Pause(string reason)
		{
			return NativeMethods.EddieLibrary.PauseClient(reason);
		}

		public NativeMethods.EddieLibraryResult Resume()
		{
			return NativeMethods.EddieLibrary.ResumeClient();
		}
		
		public NativeMethods.EddieLibraryResult Destroy()
		{
			return NativeMethods.EddieLibrary.DisposeClient();
		}
	}
}
