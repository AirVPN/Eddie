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
using System.Dynamic;
using System.Globalization;
using System.Text;

using JsonArray = System.Collections.Generic.List<object>;
using JsonDictionary = System.Collections.Generic.Dictionary<string, object>;
using JsonDictionaryKeyValuePair = System.Collections.Generic.KeyValuePair<string, object>;

namespace Eddie.Core
{
	public class JsonValue
	{
		private Json m_json;
		private string m_key;
		private int? m_index;

		public JsonValue(Json json, string key, int? index)
		{
			m_json = json;
			m_key = key;
			m_index = index;
		}

		public JsonValue this[string key]
		{
			get
			{
				// This implementation changes the content of the Json in case of a simple read operator (i.e.: json["alfa"]["beta"])
				// that is wrong since we need to keep the content immutable in case of a simple read
				/*
				Json json = Json;
				if(json == null)
				{
					json = new Json();
					m_json.SetKey(m_key, json);
				}

				return json[key];
				*/

				Json json = Json;
				return json != null ? json[key] : null;
			}
		}

		public JsonValue this[int index]
		{
			get
			{
				// Please read the notes in "this[string key]"
				/*
				Json json = Json;
				if(json == null)
				{
					json = new Json();
					m_json.SetIndex(index, json);
				}

				return json[index];
				*/

				Json json = Json;
				return json != null ? json[index] : null;
			}
		}

		public object Value
		{
			get
			{
				return m_index != null ? m_json.GetIndex(m_index.Value) : m_json.GetKey(m_key);
			}

			set
			{
				if (m_index != null)
					m_json.SetIndex(m_index.Value, value);
				else
					m_json.SetKey(m_key, value);
			}
		}

		public Json Json
		{
			get
			{
				return Value as Json;
			}
		}

		public string ValueString
		{
			get
			{
				return Conversions.ToString(Value);
			}
		}

		public bool ValueBool
		{
			get
			{
				return Conversions.ToBool(Value);
			}
		}

		public int ValueInt
		{
			get
			{
				return Conversions.ToInt32(ValueString);
			}
		}

		public Int64 ValueInt64
		{
			get
			{
				return Conversions.ToInt64(Value);
			}
		}

		public override bool Equals(object obj)
		{
			// <Rule Id="CA1065" Action="None" /> <!-- Microsoft.Design : 'JsonValue.Equals(object)' creates an exception of type 'Exception'. Exceptions should not be raised in this type of method. If this exception instance might be raised, change this method's logic so it no longer raises an exception. -->
			// throw new Exception("Do not compare JsonValue directly (use the Value property instead)");
			return false;
		}

		public override int GetHashCode()
		{
			// Alway returns 0 because the Equals method raises an unconditional exception
			return 0;
		}
	}

	public class Json : DynamicObject
	{
		public object m_value = null;

		#region Constructors

		public Json()
		{

		}

		public Json(object v)
		{
			m_value = v;
		}

		#endregion

		#region Static

		public static Json Parse(string s)
		{
			Json j = new Json();
			j.FromJson(s);
			return j;
		}

		public static bool TryParse(string s, out Json j)
		{
			object v;
			string r;
			bool k = FromJson(s, false, out v, out r);
			j = v as Json;
			return k;
		}

		#endregion

		#region Methods

		public void InitAsDictionary()
		{
			m_value = new JsonDictionary();
		}

		public bool IsDictionary()
		{
			return m_value is JsonDictionary;
		}

		public JsonDictionary GetDictionary()
		{
			return m_value as JsonDictionary;
		}

		public void EnsureDictionary()
		{
			if (!IsDictionary())
				InitAsDictionary();
		}

		public void InitAsArray()
		{
			m_value = new JsonArray();
		}

		public bool IsArray()
		{
			return m_value is JsonArray;
		}

		public JsonArray GetArray()
		{
			return m_value as JsonArray;
		}

		public void EnsureArray()
		{
			if (!IsArray())
				InitAsArray();
		}

		public void SetKey(string k, object v)
		{
			EnsureDictionary();
			(m_value as JsonDictionary)[k] = v;
		}

		public object GetKey(string k, object defValue = null)
		{
			if (!IsDictionary())
				return defValue;

			object value = null;    // Do not initialize value to defValue because TryGetValue reset its value to null in case of failure
			if (!(m_value as JsonDictionary).TryGetValue(k, out value))
				return defValue;

			return value;
		}

		public bool HasKey(string k)
		{
			if (!IsDictionary())
				return false;

			return (m_value as JsonDictionary).ContainsKey(k);
		}

		public bool RemoveKey(string k)
		{
			if (!IsDictionary())
				return false;

			return (m_value as JsonDictionary).Remove(k);
		}

		public bool RenameKey(string oldName, string newName)
		{
			if (!IsDictionary())
				return false;

			JsonDictionary dictionary = m_value as JsonDictionary;

			object value = null;
			if (!dictionary.TryGetValue(oldName, out value))
				return false;

			if (!dictionary.Remove(oldName))
				return false;

			dictionary.Add(newName, value);
			return true;
		}

		public void SetIndex(int i, object v)
		{
			EnsureArray();
			(m_value as JsonArray)[i] = v;
		}

		public object GetIndex(int i)
		{
			if (!IsArray())
				return null;

			return (m_value as JsonArray)[i];
		}

		public void Append(object v)
		{
			EnsureArray();
			(m_value as JsonArray).Add(v);
		}

		public string ToJsonPretty()
		{
			StringBuilder sb = new StringBuilder();
			ToJson(sb, true, 1);
			return sb.ToString();
		}

		public string ToTextPretty()
		{
			// Pretty print to simple text, for reporting
			return ToJsonPretty();
		}

		public string ToJson()
		{
			StringBuilder sb = new StringBuilder();
			ToJson(sb, false, 0);
			return sb.ToString();
		}

		public void FromJson(string s)
		{
			object result = null;
			string remain;
			FromJson(s, true, out result, out remain);

			if (result != null)
			{
				if (result is Json)
					m_value = (result as Json).m_value;
				else
					m_value = result;
			}
			else
			{
				m_value = null;
			}
		}

		public Json Clone() // TOOPTIMIZE
		{
			return Json.Parse(this.ToJson());
		}

		#endregion

		#region Private Methods

		private void ToJson(StringBuilder sb, bool indent, int indentN)
		{
			if (m_value is Json)
			{
				(m_value as Json).ToJson(sb, indent, indentN);
			}
			else if (IsDictionary())
			{
				sb.Append('{');
				if (indent)
					sb.Append('\n');

				int c = 0;
				JsonDictionary valueDictionary = GetDictionary();
				foreach (JsonDictionaryKeyValuePair kp in valueDictionary)
				{
					if (indent)
						sb.Append('\t', indentN);
					sb.Append('\"');
					sb.Append(EncodeString(kp.Key.ToString()));
					sb.Append('\"');
					sb.Append(':');
					if (indent)
						sb.Append(' ');
					new Json(kp.Value).ToJson(sb, indent, indentN + 1);
					if (c < (valueDictionary.Count - 1))
						sb.Append(',');
					if (indent)
						sb.Append('\n');
					c++;
				}
				if (indent)
					sb.Append('\t', indentN - 1);
				sb.Append('}');
			}
			else if (IsArray())
			{
				sb.Append('[');
				int c = 0;
				JsonArray valueArray = GetArray();
				foreach (object v in valueArray)
				{
					if (indent)
						sb.Append('\n');
					if (indent)
						sb.Append('\t', indentN);
					new Json(v).ToJson(sb, indent, indentN + 1);
					if (c < (valueArray.Count - 1))
						sb.Append(',');
					c++;
				}
				if (valueArray.Count > 0)
				{
					if (indent)
						sb.Append('\n');
					if (indent)
						sb.Append('\t', indentN - 1);
				}
				sb.Append(']');
			}
			else if (m_value == null)
			{
				sb.Append("null");
			}
			else if (m_value is bool)
			{
				sb.Append(((bool)m_value) ? "true" : "false");
			}
			else if (m_value is int)
			{
				sb.Append(((int)m_value).ToString(CultureInfo.InvariantCulture));
			}
			else if (m_value is long)
			{
				sb.Append(((long)m_value).ToString(CultureInfo.InvariantCulture));
			}
			else if (m_value is float)
			{
				sb.Append(((float)m_value).ToString(CultureInfo.InvariantCulture));
			}
			else if (m_value is double)
			{
				sb.Append(((double)m_value).ToString(CultureInfo.InvariantCulture));
			}
			else if (m_value is string)
			{
				sb.Append('\"');
				sb.Append(EncodeString((string)m_value));
				sb.Append('\"');
			}
			else
			{
				throw new Exception("Unsupported type: " + m_value.GetType().ToString());
			}
		}

		private static bool FromJson(string s, bool canThrowException, out object result, out string remain)
		{
			try
			{
				s = s.TrimStart();

				if (s == "")
					throw new Exception("Empty value");

				if (s[0] == '[')
				{
					s = s.Substring(1).TrimStart();
					Json a = new Json();
					a.InitAsArray();
					for (; ; )
					{
						if (s[0] == ']')
							break;
						object v;
						FromJson(s, canThrowException, out v, out s);
						a.Append(v);
						s = s.TrimStart();
						if (s[0] == ',')
							s = s.Substring(1).TrimStart();
					}
					result = a;
					remain = s.Substring(1).TrimStart();
					return true;
				}
				else if (s[0] == '{')
				{
					s = s.Substring(1).TrimStart();
					Json a = new Json();
					a.InitAsDictionary();
					for (; ; )
					{
						if (s[0] == '}')
							break;
						object k;
						FromJson(s, canThrowException, out k, out s);
						s = s.TrimStart();
						if (s[0] == ':')
						{
							s = s.Substring(1).TrimStart();
							object v;
							FromJson(s, canThrowException, out v, out s);
							a.SetKey(k as string, v);
							s = s.TrimStart();
							if (s[0] == ',')
								s = s.Substring(1).TrimStart();
						}
						else
							throw new Exception("Syntax error");
					}
					result = a;
					remain = s.Substring(1).TrimStart();
					return true;
				}
				else
				{
					// Direct value
					bool inQuote = false;
					bool inEscape = false;

					int i = 0;
					for (i = 0; i < s.Length + 1; i++)
					{
						char ch = (char)0;
						if (i < s.Length)
							ch = s[i];

						if (inQuote)
						{
							if ((ch == '\"') && (inEscape == false))
								inQuote = false;

							if ((ch == '\\') && (inEscape == false))
								inEscape = true;
							else
								inEscape = false;
						}
						else
						{
							if ((ch == '\"') && (inEscape == false))
								inQuote = true;
							else if ((ch == (char)0) ||
								(ch == ',') ||
								(ch == ':') ||
								(ch == '}') ||
								(ch == ']'))
							{
								string value = s.Substring(0, i).Trim();

								if ((value.StartsWith("\"")) && (value.EndsWith("\"")))
								{
									result = DecodeString(value.Substring(1, value.Length - 2));
									remain = s.Substring(i);
									return true;
								}

								if (value == "null")
								{
									result = null;
									remain = s.Substring(i);
									return true;
								}

								if (value == "true")
								{
									result = true;
									remain = s.Substring(i);
									return true;
								}

								if (value == "false")
								{
									result = false;
									remain = s.Substring(i);
									return true;
								}

								int dI = 0;
								if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out dI))
								{
									result = dI;
									remain = s.Substring(i);
									return true;
								}

								double dD = 0;
								if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out dD))
								{
									result = dD;
									remain = s.Substring(i);
									return true;
								}

								throw new Exception("Cannot detect type of value '" + value + "'");
							}
						}
					}
					throw new Exception("Syntax error");
				}
			}
			catch (Exception ex)
			{
				if (canThrowException)
					throw new Exception("JsonParser:" + ex.Message);
			}
			result = null;
			remain = s;
			return false;
		}

		#endregion
		#region Overrides, Operators

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!IsDictionary())
			{
				result = null;
				return false;
			}

			result = GetKey(binder.Name);
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			SetKey(binder.Name, value);
			return true;
		}

		public override string ToString()
		{
			return ToJson();
		}

		public JsonValue this[string key]
		{
			get
			{
				//return GetKey(key);

				// A new JsonValue must always be returned to allow assignment operations like json["key"] = value
				return new JsonValue(this, key, null);
			}
			/*
			set
			{
				SetKey(key, value);
			}
			*/
		}

		public JsonValue this[int index]
		{
			get
			{
				//return GetIndex(index);
				return new JsonValue(this, null, index);
			}
			/*
			set
			{
				SetIndex(index, value);
			}
			*/
		}

		public override bool Equals(object obj)
		{
			Json j2 = obj as Json;
			if (j2 == null)
				return false;

			if (this == j2)
				return true;

			return ToJson() == j2.ToJson();
		}

		public override int GetHashCode()
		{
			return ToJson().GetHashCode();
		}

		#endregion

		#region Encoding

		// Based on http://www.ecma-international.org/publications/files/ECMA-ST/ECMA-404.pdf

		public static string EncodeString(string s)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char ch in s.ToCharArray())
			{
				if (ch == '\"')
					sb.Append("\\\"");
				else if (ch == '\\')
					sb.Append("\\\\");
				else if (ch == '/')
					sb.Append("\\/");
				else if (ch == '\b')
					sb.Append("\\b");
				else if (ch == '\f')
					sb.Append("\\f");
				else if (ch == '\n')
					sb.Append("\\n");
				else if (ch == '\r')
					sb.Append("\\r");
				else if (ch == '\t')
					sb.Append("\\t");
				else if (ch < 128)
					sb.Append(ch);
				else
					sb.Append("\\u" + ((int)ch).ToString("X4").ToLowerInvariant());
			}
			return sb.ToString();
		}

		public static string DecodeString(string s)
		{
			StringBuilder sb = new StringBuilder();
			int i = 0;
			for (; i < s.Length; i++)
			{
				if (s[i] != '\\')
					sb.Append(s[i]);
				else if (i + 1 < s.Length)
				{
					if (s[i + 1] == '\"')
						sb.Append('\"');
					else if (s[i + 1] == '\\')
						sb.Append('\\');
					else if (s[i + 1] == '/')
						sb.Append('/');
					else if (s[i + 1] == 'b')
						sb.Append('\b');
					else if (s[i + 1] == 'f')
						sb.Append('\f');
					else if (s[i + 1] == 'n')
						sb.Append('\n');
					else if (s[i + 1] == 'r')
						sb.Append('\r');
					else if (s[i + 1] == 't')
						sb.Append('\t');
					else if ((s[i + 1] == 'u') && (i + 5 < s.Length))
					{
						string code = s.Substring(i + 2, 4);
						int uI = int.Parse(code, System.Globalization.NumberStyles.HexNumber);
						sb.Append((char)uI);
						i += 4;
					}
					i++;
				}
			}
			return sb.ToString();
		}
		#endregion
	}
}
