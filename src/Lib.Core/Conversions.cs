// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2023 AirVPN (support@airvpn.org) / https://airvpn.org
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
using System.Globalization;
using System.Xml;

namespace Eddie.Core
{
	public static class Conversions
	{
		public static Int32 ToInt32(object v, int def)
		{
			try
			{
				if ((v is String) && (v.ToString() == ""))
					return def;
				return Convert.ToInt32(v, CultureInfo.InvariantCulture);
			}
			catch
			{
				return def;
			}
		}

		public static Int32 ToInt32(string v)
		{
			Int32 vo;
			if (Int32.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out vo))
				return vo;
			else
				return 0;
		}

		public static Int32 ToInt32(float v)
		{
			return Convert.ToInt32(v, CultureInfo.InvariantCulture);
		}

		public static Int32 ToInt32(double v)
		{
			return Convert.ToInt32(v, CultureInfo.InvariantCulture);
		}

		public static UInt32 ToUInt32(string v)
		{
			UInt32 vo;
			if (UInt32.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out vo))
				return vo;
			else
				return 0;
		}

		public static Int64 ToInt64(string v)
		{
			Int64 vo;
			if (Int64.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out vo))
				return vo;
			else
				return 0;
		}

		public static Int64 ToInt64(object v)
		{
			try
			{
				if ((v is String) && (v.ToString() == ""))
					return 0;
				return Convert.ToInt64(v, CultureInfo.InvariantCulture);
			}
			catch
			{
				return 0;
			}
		}

		public static Int64 ToInt64(float v)
		{
			return Convert.ToInt64(v, CultureInfo.InvariantCulture);
		}

		public static Int64 ToInt64(double v)
		{
			return Convert.ToInt64(v, CultureInfo.InvariantCulture);
		}

		public static UInt64 ToUInt64(string v)
		{
			UInt64 vo;
			if (UInt64.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out vo))
				return vo;
			else
				return 0;
		}

		public static UInt32 ToUInt32(object v, UInt32 def)
		{
			try
			{
				if ((v is String) && (v.ToString() == ""))
					return def;
				return Convert.ToUInt32(v, CultureInfo.InvariantCulture);
			}
			catch
			{
				return def;
			}
		}

		public static float ToFloat(string v)
		{
			float vo;
			if (float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out vo))
				return vo;
			else
				return 0;
		}

		public static double ToDouble(object v)
		{
			try
			{
				return Convert.ToDouble(v);
			}
			catch
			{
				return 0;
			}
		}

		public static double ToDouble(string v)
		{
			double vo;
			if (double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out vo))
				return vo;
			else
				return 0;
		}

		public static bool ToBool(string v)
		{
			if (v == "1")
				return true;
			if (v.ToLowerInvariant() == "true")
				return true;
			if (v.ToLowerInvariant() == "yes")
				return true;
			return false;
		}

		public static bool ToBool(object o)
		{
			return (ToString(o) == "true");
		}

		public static string ToString(object o)
		{
			if (o == null)
				return "";
			else if (o is bool)
			{
				bool b = (bool)o;
				return (b ? "true" : "false");
			}
			else if (o is string[])
				return string.Join(",", o as string[]);
			else if (o is float)
			{
				float f = (float)o;
				return f.ToString(CultureInfo.InvariantCulture);
			}
			else
				return o.ToString();

		}

		public static DateTime ToDateTime(Int64 unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
			return dtDateTime;
		}

		public static int ToUnixTime(DateTime dt)
		{
			return Convert.ToInt32((dt - new DateTime(1970, 1, 1)).TotalSeconds, CultureInfo.InvariantCulture);
		}

		public static Int64 ToUnixTimeMs(DateTime dt)
		{
			return Convert.ToInt64((dt - new DateTime(1970, 1, 1)).TotalMilliseconds, CultureInfo.InvariantCulture);
		}

		public static string StringToBase64(string s)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(s);
			return System.Convert.ToBase64String(bytes);
		}

		public static string Base64ToString(string s)
		{
			return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(s));
		}

		public static Json ToJson(XmlElement xml)
		{
			/*
			 * Comments are ignored.
			 * Node renames in json conversion can be forced with attribute json-convert-name.
			 * Key name are lowered-case.
			 * 
			 * Can fail if:
			 * 1- There are duplicate child names. Example:
			 * <test>
			 *	<alfa/>
			 *	<beta/>
			 *	<alfa/>
			 * </test>
			 * If all childs have the same name, works (detected as Array)
			 * 
			 * 2- There are nested texts. Example:
			 * <test>
			 *	mytext1
			 *	<alfa/>
			 *	mytext2
			 * </test>
			 * 			 
			 */

			Json result = new Json();

			foreach (XmlAttribute attr in xml.Attributes)
			{
				string keyName = attr.Name.ToLowerInvariant();
				if (keyName.StartsWithInv("json-convert-"))
					continue;

				// Try Cast?
				if (result.HasKey(keyName))
					throw new Exception("Cannot convert.");

				object value = attr.Value;

				if (attr.Value.ToLowerInvariant() == "true")
					value = true;
				else if (attr.Value.ToLowerInvariant() == "true")
					value = false;

				result.SetKey(keyName, value);
			}

			// Exception: if not have attributes, and childs are XmlElement with all the same name, use Json Array.			
			bool isArray = false;
			if (xml.Attributes.Count == 0)
			{
				isArray = true;
				string commonName = "";
				int nChildsWithName = 0; // Not really used yet
				foreach (XmlNode child in xml.ChildNodes)
				{
					if (child is XmlComment)
					{
						// Ignore
					}
					else if (child is XmlText)
					{
						// No Array
						isArray = false;
						break;
					}
					else if (child is XmlElement)
					{
						XmlElement xmlElement = child as XmlElement;
						string keyName = child.Name.ToLowerInvariant();
						if (xmlElement.HasAttribute("json-convert-name"))
							keyName = xmlElement.GetAttribute("json-convert-name");

						if (commonName == "")
							commonName = keyName;
						else if (commonName != keyName)
						{
							// No Array
							isArray = false;
							break;
						}
						nChildsWithName++;
					}
					else
					{
						throw new Exception("Xml node unknown type");
					}
				}
			}

			foreach (XmlNode child in xml.ChildNodes)
			{
				if (child is XmlComment)
				{
					// Ignore
				}
				else if (child is XmlText)
				{
					/*
					if (result.HasKey(child.ParentNode.Name))
						throw new Exception("Cannot convert.");
					result.SetKey(child.ParentNode.Name, child.InnerText);
					*/
				}
				else if (child is XmlElement)
				{
					XmlElement xmlChild = child as XmlElement;

					// Exception: if contain text
					bool textFound = false;
					foreach (XmlNode xmlChild2 in xmlChild.ChildNodes)
					{
						if (xmlChild2 is XmlText)
						{
							result.SetKey(xmlChild.Name.ToLowerInvariant(), xmlChild.InnerText);
							textFound = true;
							break;
						}
					}
					if (textFound)
						continue;

					// Exception: if have only two attribute 'name' and 'value'					
					if ((xmlChild.Attributes.Count == 2) && (xmlChild.HasAttribute("name")) && (xmlChild.HasAttribute("value")))
					{
						if (result.HasKey(xmlChild.GetAttribute("name")))
							throw new Exception("Cannot convert.");
						result.SetKey(xmlChild.GetAttribute("name").ToLowerInvariant(), xmlChild.GetAttribute("value"));
					}
					else
					{
						XmlElement xmlElement = child as XmlElement;
						string keyName = child.Name.ToLowerInvariant();
						if (xmlElement.HasAttribute("json-convert-name"))
							keyName = xmlElement.GetAttribute("json-convert-name");
						if ((isArray == false) && (result.HasKey(keyName)))
							throw new Exception("Cannot convert.");

						Json jChild = ToJson(xmlElement);

						if (isArray)
							result.Append(jChild);
						else
							result.SetKey(keyName, jChild);
					}
				}
				else
				{
					throw new Exception("Xml node unknown type");
				}

			}

			return result;
		}
	}
}
