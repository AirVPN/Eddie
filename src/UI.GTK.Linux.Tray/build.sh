#!/bin/bash

# 2022-05-12: builded with dynamic link, require some libs (mainly libayatana-appindicator and GTK2).
# some of them are copied in /deploy/ for Portable/AppImage
# libdbusmenu-glib.so.4
# libdbusmenu-gtk.so.4
# libayatana-indicator.so.7
# libayatana-appindicator.so.1
# run
# patchelf --set-rpath '$ORIGIN' on each file

set -e

if [ "$1" == "" ]; then
    echo First arg must be Config, 'Debug' or 'Release'
    exit 1
fi

BASEPATH=$(dirname $(realpath -s $0))
mkdir -p "$BASEPATH/bin"
mkdir -p "$BASEPATH/obj"

FILES=""
FLAGS=""
DEFINES=""
CONFIG="$1"

FILES="${FILES} $BASEPATH/src/main.cpp"

echo "Building eddie-tray - Config: $CONFIG"

#if [ -f "/etc/arch-release" ]; then
#	if pacman -Qs "libappindicator-gtk2" > /dev/null ; then
#		echo "Building eddie-tray - Linux Arch detected, libappindicator-gtk2 present.";
#	else
#		echo "Building eddie-tray - Linux Arch detected, libappindicator-gtk2 NOT present, skip build.";
#		exit 0
#	fi
#fi

# gtk3
# g++ "$BASEPATH/main.cpp" -o "$BASEPATH/bin/eddie-tray" `pkg-config --cflags gtk+-3.0 ayatana-appindicator3-0.1` `pkg-config --libs gtk+-3.0 ayatana-appindicator3-0.1` 

# gtk2
echo "Building eddie-tray - If compilation errors occur, remember libgtk2.0-dev and libayatana-appindicator-dev package are required."
export LDFLAGS="-Wl,-rpath=."
if [ -f "/etc/arch-release" ]; then
	#2022-05-13: in Arch/Manjaro, the package is libappindicator-0.1
	g++ "$BASEPATH/main.cpp" -fPIC -Dappindicatorpackage_libappindicator -o "$BASEPATH/bin/eddie-tray" `pkg-config --cflags gtk+-2.0 appindicator-0.1` `pkg-config --libs gtk+-2.0 appindicator-0.1` 
else
	#2022-05-13: recently in major distro, the package is ayatana-appindicator-0.1
	g++ "$BASEPATH/main.cpp" -fPIC -Dappindicatorpackage_libayatanaappindicator -o "$BASEPATH/bin/eddie-tray" `pkg-config --cflags gtk+-2.0 ayatana-appindicator-0.1` `pkg-config --libs gtk+-2.0 ayatana-appindicator-0.1` 
fi

strip -S --strip-unneeded "$BASEPATH/bin/eddie-tray" 
chmod a+x "$BASEPATH/bin/eddie-tray"
patchelf --set-rpath '$ORIGIN' "$BASEPATH/bin/eddie-tray"

echo "Building eddie-tray - Done"
exit 0
