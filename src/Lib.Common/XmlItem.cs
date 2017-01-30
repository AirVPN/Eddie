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
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Lib.Common
{
    public class XmlItem
    {
        XmlElement m_element;

        public XmlItem(string value)
        {
            if (value.StartsWith("<"))
            {
                // Try parse XML
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);
                m_element = xmlDoc.DocumentElement;
            }
            else
            {
                // Try parse CommandLine
                Init("command");
                CommandLine cmd = new CommandLine(value, false, true);

                foreach (KeyValuePair<string, string> item in cmd.Params)
                    SetAttribute(item.Key, item.Value);
            }
        }

        public XmlItem(XmlElement xmlElement)
        {
            m_element = xmlElement;
        }

        public override string ToString()
        {
            return m_element.OuterXml;
        }

        // Management

        public void Init(string name)
        {
            XmlDocument xmlDoc = new XmlDocument();
            m_element = xmlDoc.CreateElement(name);
        }

        public bool HasAttribute(string name)
        {
            return m_element.HasAttribute(name);
        }

        public string GetAttribute(string name, string def)
        {
            if (HasAttribute(name))
                return m_element.GetAttribute(name);
            else
                return def;
        }

        public string GetAttribute(string name)
        {
            return GetAttribute(name, "");
        }

        public bool GetAttributeBool(string name, bool def)
        {
            if (HasAttribute(name))
            {
                string v = GetAttribute(name).ToLowerInvariant();
                if (v == "false")
                    return false;
                else if (v == "true")
                    return true;
                else if (v == "off")
                    return false;
                else if (v == "on")
                    return true;
                else
                {
                    int i;
                    if (int.TryParse(v, out i))
                    {
                        return (i != 0);
                    }
                    else
                        return false;
                }
            }
            else
                return def;
        }

        public int GetAttributeInt(string name, int def)
        {
            if (HasAttribute(name))
            {
                string v = GetAttribute(name);
                int i;
                if (int.TryParse(v, out i))
                {
                    return i;
                }
                else
                    return 0;
            }
            else
                return def;
        }

        public void SetAttribute(string name, string value)
        {
            m_element.SetAttribute(name, value);
        }

        public void SetAttributeInt(string name, int value)
        {
            SetAttribute(name, value.ToString(CultureInfo.InvariantCulture));
        }

        public void SetAttributeBool(string name, bool value)
        {
            SetAttribute(name, value ? "true" : "false");
        }

        public void SetAttributeInt64(string name, Int64 value)
        {
            SetAttribute(name, value.ToString());
        }

        XmlItem GetFirstChildWithName(string name)
        {
            return null;
        }

        XmlItem GetFirstChild()
        {
            return null;
        }

        public XmlItem CreateChild(string name)
        {
            XmlElement xmlChild = m_element.OwnerDocument.CreateElement(name);
            m_element.AppendChild(xmlChild);
            return new XmlItem(xmlChild);
        }
    }
}
