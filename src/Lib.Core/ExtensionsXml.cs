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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Core
{
	public static class ExtensionsXml
	{
		public static string GetAttributeString(this XmlNode node, string name, string def)
		{
			XmlNode nodeAttr = node.Attributes[name];
			if (nodeAttr == null)
				return def;
			else
				return nodeAttr.Value;
		}

        public static string[] GetAttributeStringArray(this XmlNode node, string name)
        {
            XmlNode nodeAttr = node.Attributes[name];
            if (nodeAttr == null)
                return new string[0];
            else if (nodeAttr.Value == "") // New in 2.8
                return new string[0];
            else
                return nodeAttr.Value.Split(',');
        }

        public static bool GetAttributeBool(this XmlNode node, string name, bool def)
        {
            XmlNode nodeAttr = node.Attributes[name];
            if (nodeAttr == null)
                return def;
            else
                return Conversions.ToBool(nodeAttr.Value);
        }

        public static int GetAttributeInt(this XmlNode node, string name, int def)
        {
            XmlNode nodeAttr = node.Attributes[name];
            if (nodeAttr == null)
                return def;
            else
                return Conversions.ToInt32(nodeAttr.Value);
        }

        public static Int64 GetAttributeInt64(this XmlNode node, string name, Int64 def)
        {
            XmlNode nodeAttr = node.Attributes[name];
            if (nodeAttr == null)
                return def;
            else
                return Conversions.ToInt64(nodeAttr.Value);
        }

        public static float GetAttributeFloat(this XmlNode node, string name, float def)
        {
            XmlNode nodeAttr = node.Attributes[name];
            if (nodeAttr == null)
                return def;
            else
                return Conversions.ToFloat(nodeAttr.Value);
        }

        public static void SetAttributeString(this XmlElement node, string name, string val)
		{
			node.SetAttribute(name, val);
		}

		public static void SetAttributeStringArray(this XmlElement node, string name, string[] val)
		{
			if ((val == null) || (val.Length == 0)) // Added in 2.8
				node.SetAttribute(name, "");
			else
				node.SetAttribute(name, String.Join(",", val)); // TODO: Escaping
		}

		public static void SetAttributeBool(this XmlElement node, string name, bool val)
		{
			node.SetAttribute(name, Conversions.ToString(val));
		}

		public static void SetAttributeInt(this XmlElement node, string name, int val)
		{
			node.SetAttribute(name, Conversions.ToString(val));
		}

		public static void SetAttributeInt64(this XmlElement node, string name, Int64 val)
		{
			node.SetAttribute(name, Conversions.ToString(val));
		}

		public static void SetAttributeFloat(this XmlElement node, string name, float val)
		{
			node.SetAttribute(name, Conversions.ToString(val));
		}

		public static void SetAttributeDouble(this XmlElement node, string name, double val)
		{
			node.SetAttribute(name, Conversions.ToString(val));
		}

		public static bool ExistsAttribute(this XmlNode node, string name)
		{
			XmlNode nodeAttr = node.Attributes[name];
			if (nodeAttr == null)
				return false;
			else
				return true;
		}

		public static XmlElement GetFirstElementByTagName(this XmlElement node, string name)
		{
			foreach (XmlElement xmlChild in node.ChildNodes)
				if (xmlChild.Name == name)
					return xmlChild;
			return null;			
		}

		// Utils, can be extension (TODO)

		public static void ZZZZ_XmlRenameTagName(XmlElement parent, string oldName, string newName) // TOCLEAN
		{
			foreach (XmlElement e in parent.GetElementsByTagName(oldName))
			{
				// TODO
			}
		}

		public static string ZZZZ_XmlGetBody(XmlElement node) // TOCLEAN
		{
			if (node == null)
				return "";
			return node.InnerText;
		}

		// Utils, not extension

		public static void XmlCopyElement(XmlElement source, XmlElement parentDestination)
		{
			XmlNode xmlClone = parentDestination.OwnerDocument.ImportNode(source, true);
			parentDestination.AppendChild(xmlClone);
		}

		public static XmlElement XmlCreateElement(string name)
		{
			XmlDocument xmlDoc = new XmlDocument();
			return xmlDoc.CreateElement(name);
		}

		// Only dictionary, only string supported right now.
		public static void JsonToXml(Json source, XmlElement destination)
		{
			if(source.IsDictionary())
			{
				foreach(KeyValuePair<string, object> kp in source.GetDictionary())
				{
					if(kp.Value is string)
					{
						destination.SetAttribute(kp.Key as string, kp.Value as string);
					}
				}
			}
		}

		// Only dictionary, only string supported right now.
		public static void XmlToJson(XmlElement source, Json destination)
		{
			foreach(XmlAttribute attr in source.Attributes)
			{
				destination[attr.Name].Value = attr.Value;
			}
		}
	}
}

