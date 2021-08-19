Name: eddie-ui
Version: {@version}
Release: 0

License: see /usr/share/doc/eddie-ui/copyright
Summary: Eddie - VPN Tunnel - UI
Packager: Eddie.website <maintainer@eddie.website>
URL: https://eddie.website
Group: net
Requires: {@requires}
AutoReqProv: no
Obsoletes: airvpn < 2.14.4

%define _rpmdir ../
%define _rpmfilename %%{NAME}-%%{VERSION}-%%{RELEASE}.%%{ARCH}.rpm
%define _unpackaged_files_terminate_build 0

%description
OpenVPN/WireGuard UI with additional user-friendly features
Open-Source, GPLv3, Developed by AirVPN

%files
{@files}