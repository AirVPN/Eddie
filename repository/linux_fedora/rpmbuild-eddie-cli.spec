Name: eddie-cli
Version: {@version}
Release: 0

License: see /usr/share/doc/eddie-cli/copyright
Summary: Eddie - VPN Tunnel - CLI
Packager: Eddie.website <maintainer@eddie.website>
URL: https://eddie.website
Group: net
Requires: {@requires}
AutoReqProv: no

%define _rpmdir ../
%define _rpmfilename %%{NAME}-%%{VERSION}-%%{RELEASE}.%%{ARCH}.rpm
%define _unpackaged_files_terminate_build 0

%description
OpenVPN/WireGuard CLI with additional user-friendly features
Open-Source, GPLv3, Developed by AirVPN

%files
{@files}