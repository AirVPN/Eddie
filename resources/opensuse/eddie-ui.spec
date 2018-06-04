Name: eddie-ui
Version: {@version}
Release: 0

License: see /usr/share/doc/eddie-ui/copyright
Summary: Eddie - OpenVPN UI
Packager: Eddie.website <maintainer@eddie.website>
URL: https://eddie.website
Group: net
Requires: mono-core mono-winforms sudo openvpn curl libgdiplus-devel libnotify libappindicator1 stunnel
AutoReqProv: no
Obsoletes: airvpn < 2.14.4

%define _rpmdir ../
%define _rpmfilename %%{NAME}-%%{VERSION}-%%{RELEASE}.%%{ARCH}.rpm
%define _unpackaged_files_terminate_build 0

%description
OpenVPN UI with additional user-friendly features
Open-Source, GPLv3, Developed by AirVPN

%files
"/usr/share/applications/eddie-ui.desktop"
%dir "/usr/share/doc/eddie-ui/"
"/usr/share/doc/eddie-ui/copyright"
"/usr/share/pixmaps/eddie-ui.png"
"/usr/share/polkit-1/actions/org.airvpn.eddie.ui.policy"
"/usr/share/man/man8/eddie-ui.8.gz"
"/usr/share/eddie-ui/cacert.pem"
"/usr/share/eddie-ui/icon.png"
"/usr/share/eddie-ui/icon_gray.png"
"/usr/share/eddie-ui/tray.png"
"/usr/share/eddie-ui/tray_gray.png"
"/usr/bin/eddie-ui"
%dir "/usr/{@lib}/eddie-ui/"
"/usr/{@lib}/eddie-ui/Lib.Core.dll"
"/usr/{@lib}/eddie-ui/Lib.Common.dll"
"/usr/{@lib}/eddie-ui/Lib.Forms.dll"
"/usr/{@lib}/eddie-ui/Lib.Platform.Linux.dll"
"/usr/{@lib}/eddie-ui/libLib.Platform.Linux.Native.so"
"/usr/{@lib}/eddie-ui/update-resolv-conf"
"/usr/{@lib}/eddie-ui/Eddie-UI.exe"
"/usr/{@lib}/eddie-ui/eddie-tray"
