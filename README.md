### Eddie - OpenVPN GUI

OpenVPN GUI (open source under GPLv3 license) with additional features like:
- User-friendly UI
- Multiplatform support: Windows, OS X, macOS, GNU/Linux (with particular effort to [support a lot of distro](https://airvpn.org/forum/35-client-software-platforms-environments/)) with multiple architectures: x64, x86, armv7i (Raspberry)
- Censorship counter-measures, like tunnel over SSH, over SSL, direct Tor integration
- Network lock / leak prevention
- Advanced options for DNS, routing, events, leak etc.
- Support of VPN service providers (under development, currently it supports only AirVPN)
- Customizable actions triggered by events
- CLI edition

Eddie is developed by and is the official client of [AirVPN] (https://airvpn.org), a VPN service based on OpenVPN and operated by activists and hacktivists in defence of net neutrality, privacy and against censorship.

Currently release 2.x supports only AirVPN authentication and servers. Support of other VPN providers exists but is still under beta-stage.

### Links:

- [Download](https://airvpn.org/enter/)
- [Overview](https://airvpn.org/software/)
- [FAQ](https://airvpn.org/forum/34-client-software/)
- [Support forum](https://eddie.website/support/)
- [Changelogs](https://eddie.website/changelog/?software=client&format=html)

### Tech

Eddie is written in C# with .Net/Mono framework. macOS edition use Xamarin.Mac. It is bundled with precompiled binary versions of OpenVPN, stunnel, plink/ssh and other tools for many platforms and architectures. If you don't want them, simply remove them and Eddie will locate it in your file system, or provide custom paths.

To compile from sources, follow this [this guide](https://airvpn.org/topic/12760-how-can-i-compile-the-source-code/). 
