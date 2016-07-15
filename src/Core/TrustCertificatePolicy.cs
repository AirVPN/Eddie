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
using System.Net;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Eddie.Core
{
	public static class TrustCertificatePolicy
    {
        private static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
			// Data exchange security are NOT based on SSL. Look "AirExchange.cs".

			// The only SSL sessions are for checking tunnel and DNS, with a request to the VPN server itself.

			// Mono doesn't include by default root certificates: http://www.mono-project.com/docs/faq/security/

			// This is used only from Linux, not used under Windows or OS X.

			// TOFIX
			
            return true;
        }

        public static void Activate()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ValidateCertificate;
        }
    }
}
