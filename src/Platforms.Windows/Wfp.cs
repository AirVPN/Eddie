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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.NetworkInformation;
using System.Management;
using System.Security.Principal;
using System.Xml;
using System.Text;
using System.Threading;
using AirVPN.Core;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace AirVPN.Platforms
{
    public class Wfp
    {
        private static List<WfpItem> Items = new List<WfpItem>();

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void LibPocketFirewallInit(string name);

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibPocketFirewallStart();

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibPocketFirewallStop();

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern UInt64 LibPocketFirewallAddRule(string xml);

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool LibPocketFirewallRemoveRule(UInt64 id);

        [DllImport("LibPocketFirewall.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr LibPocketFirewallGetLastError();

        public static string LibPocketFirewallGetLastError2()
        {
            IntPtr result = LibPocketFirewallGetLastError();
            string s = Marshal.PtrToStringAnsi(result);
            return s;
        }

        public static void RemoveItem(WfpItem item)
        {
            if (item == null)
                return;

            Items.Remove(item);
        }

        public static WfpItem AddItem(XmlElement xml)
        {
            WfpItem item = new WfpItem();

            Items.Add(item);
            return item;
        }
    }
}
