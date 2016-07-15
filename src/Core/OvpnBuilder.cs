﻿// <eddie_source_header>
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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Eddie.Core
{
	public class OvpnBuilder
	{
        public class Directive
        {
            public string Text;
            public string Comment;
        }

		public Dictionary<string, List<Directive>> Directives = new Dictionary<string, List<Directive>>();

		public bool IsMultipleDirective(string name)
		{
			// If a directive can be specified more time.
			if (
				(name == "remote") ||
				(name == "route") ||
				(name == "plugin") ||
				(name == "x509-track") ||
				(name == "http-proxy-option") ||
				(name == "ignore-unknown-option")
			  )
				return true;

			return false;
		}

		public string Get()
		{
			string result = "";

			DateTime now = DateTime.UtcNow;

			result += "# " + Engine.Instance.GenerateFileHeader() + "\n";
			result += "# " + now.ToLongDateString() + " " + now.ToLongTimeString() + " UTC" + "\n";

			foreach (KeyValuePair<string, List<Directive>> kp in Directives)
			{
				foreach (Directive value in kp.Value)
				{
                    if (kp.Key.StartsWith("<"))
                    {
                        result += kp.Key + "\n" + value.Text.Trim() + "\n" + kp.Key.Replace("<", "</");
                    }
                    else
                    {
                        result += kp.Key + " " + value.Text.Trim();
                    }
                    if(value.Comment != "")
                        result += " # " + value.Comment;
                    result += "\n";
				}
			}

			return result;
		}

		public void AppendDirective(string name, string body, string comment)
		{
			if (IsMultipleDirective(name))
			{
				if (Directives.ContainsKey(name) == false)
					Directives[name] = new List<Directive>();
			}
			else
			{
				Directives[name] = new List<Directive>();
			}

            Directive d = new Directive();
            d.Text = body.Trim();
            d.Comment = comment.Trim();
            Directives[name].Add(d);			
		}

        public bool ExistsDirective(string name)
        {
            return Directives.ContainsKey(name);
        }

		public void RemoveDirective(string name)
		{
			if (Directives.ContainsKey(name))
				Directives.Remove(name);
		}

		public Directive GetDirective(string name)
		{
			if (Directives.ContainsKey(name) == false)
				return new Directive();
			if(Directives[name].Count != 1)
				return new Directive();
			
			return Directives[name][0];			
		}

        public Directive GetOneDirective(string name)
        {
            if (Directives.ContainsKey(name) == false)
                return new Directive();
            if (Directives[name].Count == 0)
                return new Directive();

            return Directives[name][0];
        }

        public void AppendDirectives(string directives, string comment)
		{
			string text = directives;

			// Cleaning			
			text = "\n" + directives.Replace("\r","\n") + "\n";
						
			for (; ; )
			{
				string originalText = text;
				
				text = text.Replace("\n\n", "\n");
				text = text.Replace("  ", " ");
				text = text.Replace("\t", " ");

				int posComment1 = text.IndexOf("#");
				if (posComment1 != -1)
				{
					int posEndOfLine = text.IndexOf("\n", posComment1);
					text = text.Substring(0, posComment1) + text.Substring(posEndOfLine);
				}

				int posComment2 = text.IndexOf("\n;");
				if (posComment2 != -1)
				{
					int posEndOfLine = text.IndexOf("\n", posComment2);
					text = text.Substring(0, posComment2) + text.Substring(posEndOfLine);
				}

				if (text == originalText)
					break;
			}

			for (; ; )
			{
				text = text.Trim();

				if (text == "")
					break;

				string directiveName;
				string directiveBody;

				if (text.Substring(0, 1) == "<")
				{
					int posEndStartTag = text.IndexOf('>');
					if (posEndStartTag == -1)
						throw new Exception("Syntax error"); // TOTRANSLATE

					directiveName = text.Substring(0, posEndStartTag + 1);
					string endTag = directiveName.Replace("<", "</");
					int posEndTag = text.IndexOf(endTag);
					if(posEndTag == -1)
						throw new Exception("Syntax error"); // TOTRANSLATE
					directiveBody = text.Substring(posEndStartTag + 1, posEndTag - posEndStartTag - 1);
					text = text.Substring(posEndTag + endTag.Length);
				}
				else
				{
					int posSpace = text.IndexOf(" ");
					int posEndLine = text.IndexOf("\n");
					if (posSpace == -1)
					{
						directiveName = text;
						directiveBody = "";
						text = "";
					}
					else if( (posEndLine != -1) && (posEndLine < posSpace) )
					{
						directiveName = text.Substring(0, posEndLine);
						directiveBody = "";
						text = text.Substring(posEndLine);
					}
					else
					{
						directiveName = text.Substring(0, posSpace);
						
						if (posEndLine == -1)
						{
							directiveBody = text.Substring(posSpace +1);
							text = "";
						}
						else
						{
							directiveBody = text.Substring(posSpace + 1, posEndLine - posSpace - 1);
							text = text.Substring(posEndLine);
						}
					}
				}

				AppendDirective(directiveName.Trim(), directiveBody.Trim(), comment);
			}



			
		}

	}
}
