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

echo "Building eddie-tray - If compilation errors occur, remember libayatana-appindicator3-dev package is required."
export LDFLAGS="-Wl,-rpath=."
g++ "$BASEPATH/main.cpp" -fPIC -o "$BASEPATH/bin/eddie-tray" `pkg-config --cflags gtk+-3.0 ayatana-appindicator3-0.1` `pkg-config --libs gtk+-3.0 ayatana-appindicator3-0.1` 

strip -S --strip-unneeded "$BASEPATH/bin/eddie-tray" 
chmod a+x "$BASEPATH/bin/eddie-tray"
patchelf --set-rpath '$ORIGIN' "$BASEPATH/bin/eddie-tray"

echo "Building eddie-tray - Done"
exit 0
