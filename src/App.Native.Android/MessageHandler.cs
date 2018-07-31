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

using Android.OS;

namespace Eddie.NativeAndroidApp
{
	public class MessageHandler : Handler
	{
		private IMessageHandler m_handler = null;
	
		public MessageHandler(IMessageHandler handler)
		{
			m_handler = handler;
		}

		public override void HandleMessage(Message msg)
		{
            if(msg == null)
                return;

			base.HandleMessage(msg);

			if(m_handler != null)
                m_handler.OnMessage(msg);
		}
	}
}
