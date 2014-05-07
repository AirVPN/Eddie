// <airvpn_source_header>
// This file is part of AirVPN Client software.
// Copyright (C)2014-2014 AirVPN (support@airvpn.org) / https://airvpn.org )
//
// AirVPN Client is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// AirVPN Client is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with AirVPN Client. If not, see <http://www.gnu.org/licenses/>.
// </airvpn_source_header>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AirVPN.Core
{
    public static class Debug
    {
        static object SpinLock = new object();

        // Useful only in development. Log on console.
        public static void Trace(string msg)
        {
			if (Engine.Instance.DevelopmentEnvironment == false)
				return;

            lock (SpinLock)
            {
                if (Platform.Instance is Platforms.Windows)
                    Console.WriteLine(msg);
            }
        }

        // Useful only in development. Log on file.
        public static void TraceOnLogFile(string msg)
        {
			if (Engine.Instance.DevelopmentEnvironment == false)
				return;

            lock (SpinLock)
            {
                StreamWriter w = File.AppendText("debug.log");
                w.WriteLine(msg);
                w.Close();
            }
        }


        // Useful only in development. Log on console. Fatal event, breakpoint reccomended.
        public static void Fatal(string msg)
        {
            Trace(msg);
        }

        public static void Trace(Exception ex)
        {
            Trace(ex.Message);
        }
    }
}
