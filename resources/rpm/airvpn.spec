Name: airvpn
Version: {@version}
Release: 0

License: see /usr/share/doc/airvpn/copyright
Summary: AirVPN - VPN service based on OpenVPN
Packager: AirVPN Team <info@airvpn.org>
URL: https://airvpn.org
Group: net
Requires: mono-core mono-winforms openvpn curl libgdiplus-devel
AutoReqProv: no

%define _rpmdir ../
%define _rpmfilename %%{NAME}-%%{VERSION}-%%{RELEASE}.%%{ARCH}.rpm
%define _unpackaged_files_terminate_build 0

%description
VPN based on OpenVPN and operated by activists and hacktivists 
in defence of net neutrality, privacy and against censorship.

%files
"/usr/share/applications/AirVPN.desktop"
%dir "/usr/share/doc/airvpn/"
"/usr/share/doc/airvpn/copyright"
"/usr/share/pixmaps/AirVPN.png"
"/usr/share/man/man8/airvpn.8.gz"
"/usr/share/AirVPN/cacert.pem"
"/usr/bin/airvpn"
%dir "/usr/{@lib}/AirVPN/"
"/usr/{@lib}/AirVPN/Lib.Core.dll"
"/usr/{@lib}/AirVPN/Lib.Common.dll"
"/usr/{@lib}/AirVPN/Lib.Forms.dll"
"/usr/{@lib}/AirVPN/Platforms.Linux.dll"
"/usr/{@lib}/AirVPN/update-resolv-conf"
"/usr/{@lib}/AirVPN/AirVPN.exe"
"/usr/{@lib}/AirVPN/CLI.exe"
"/usr/{@lib}/AirVPN/stunnel"
