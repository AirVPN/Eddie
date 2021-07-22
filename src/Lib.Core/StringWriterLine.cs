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
using System.Text;

namespace Eddie.Core
{
	// Receive piece of texts, buffer, ensure an event for each line
	public class StringWriterLine
	{
		private string m_buffer = ""; // TOOPTIMIZE with a StringBuffer

		public bool IgnorePending = false;

		public delegate void LineHandler(string line);
		public event LineHandler LineEvent;

		public void Write(string data)
		{
			m_buffer += data;

			for (; ; )
			{
				int posEndLine = m_buffer.IndexOf('\n');
				if (posEndLine != -1)
				{
					string line = m_buffer.Substring(0, posEndLine);
					m_buffer = m_buffer.Substring(posEndLine + 1);
					if (LineEvent != null)
						LineEvent(line);
				}
				else
					break;
			}
		}

		public void Stop()
		{
			if (IgnorePending == false)
				if (m_buffer != "")
					if (LineEvent != null)
						LineEvent(m_buffer);
			m_buffer = "";
		}
	}
}
