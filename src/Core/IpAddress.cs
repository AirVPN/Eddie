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
using System.Text;
using System.Xml;

namespace Eddie.Core
{
	public class IpAddress
	{
		public string Value = "";

		public IpAddress()
		{
			Value = "";
		}

		public IpAddress(string value)
		{
			Value = value.Trim();
		}

		public static implicit operator IpAddress(string value)
		{
			return new IpAddress(value);
		}

		public bool Empty
		{
			get
			{
				return (Value == "");
			}
		}

		public bool Valid
		{
			get
			{
				return System.Text.RegularExpressions.Regex.IsMatch(Value, @"\b((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$\b");
			}
		}

		public void Clear()
		{
			Value = "";
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			IpAddress two = obj as IpAddress;
			if (two == null)
				return false;

			return Value == two.Value;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value;
		}
	}
}
