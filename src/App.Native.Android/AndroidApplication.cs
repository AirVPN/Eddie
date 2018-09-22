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

using System;
using Android.App;
using Android.Runtime;

namespace Eddie.NativeAndroidApp
{
	[Application]
	class AndroidApplication : global::Android.App.Application
	{
		public static string LOG_TAG = "Eddie.Android";

		private bool m_initialized = false;

		public AndroidApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle,transfer)
        {
		}

		public bool Initialized
		{
			get
			{
				return m_initialized;
			}
		}
				
		public override void OnCreate()
		{
			base.OnCreate();

			if(!m_initialized)
			{
                EddieLogger.Init(this);

                EddieLogger.Info("Initializing eddie library");
                EddieLogger.Info("{0} - {1}", NativeMethods.EddieLibrary.LibraryQualifiedName(), NativeMethods.EddieLibrary.LibraryReleaseDate());
                EddieLogger.Info("Eddie Library API level {0}", NativeMethods.EddieLibrary.LibraryApiLevel());

				NativeMethods.EddieLibraryResult result = NativeMethods.EddieLibrary.Init();

                if(result.code == NativeMethods.ResultCode.SUCCESS)
				{
					m_initialized = true;
					
                    EddieLogger.Info("Eddie Library: OpenVPN initialization succeeded");
				}					
				else
				{
					EddieLogger.Error("Eddie Library: OpenVPN initialization failed. {0}", result.description);
				}					
			}				
		}

		public override void OnTerminate()
		{
			if(m_initialized)
			{
				m_initialized = false;

				NativeMethods.EddieLibraryResult result = NativeMethods.EddieLibrary.Cleanup();
				
                if(result.code != NativeMethods.ResultCode.SUCCESS)
					EddieLogger.Error("Eddie Library: OpenVPN cleanup failed. {0}", result.description);

                result = NativeMethods.EddieLibrary.DisposeClient();
                
                if(result.code != NativeMethods.ResultCode.SUCCESS)
                    EddieLogger.Error("Eddie Library: Failed to dispose OpenVPN client. '{0}'", result.description);
			}

			base.OnTerminate();
		}
	}
}
