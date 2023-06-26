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

namespace Eddie.Core.ConfigBuilder
{
	public class Hummingbird : OpenVPN
	{
		public override bool GetSupportCommentInConfig()
		{
			// Hummingbird bug: don't trim correctly # comments
			return false;
		}

		public override void Adaptation()
		{
			base.Adaptation();

			// Report here directive not supported

			// Throw fatal error
			//RemoveDirective("ping-exit"); // Removed in 2.23.0, supported in HB 1.3.0
			//RemoveDirective("pull-filter"); // Removed in 2.23.0, supported in HB 1.3.0

			// Throw notice "Unsupported option (ignored) - Based on HB 1.3.0
			RemoveDirective("explicit-exit-notify");
			RemoveDirective("connect-retry-max");
			RemoveDirective("persist-tun");
			RemoveDirective("persist-key");
			RemoveDirective("resolv-retry");
			RemoveDirective("auth-nocache");
			RemoveDirective("data-ciphers-fallback");
		}
	}
}
