#!/bin/bash

set -e

if [ "$1" == "" ]; then
    echo First arg must be Config, 'Debug' or 'Release'
    exit 1
fi

CONFIG=$1

BASEPATH=$(dirname $(realpath -s $0))
mkdir -p "$BASEPATH/bin"

MARCHOS=$(uname -m)
if [ $MARCHOS = "i686" ]; then
	MARCHOS="i386"
elif [ $MARCHOS = "x86_64" ]; then
	MARCHOS="x86_64"
elif [ $MARCHOS = "armv7l" ]; then # rPI 32
	MARCHOS="arm-linux-gnueabihf"
elif [ $MARCHOS = "aarch64" ]; then # rPI 64
	MARCHOS="arm-linux-gnueabihf" # Not sure
else
	echo $ARCHOS
fi

echo "Building libLib.Platform.Linux.Native.so - Config: $CONFIG"

# HTTP fetch is performed in-process through libcurl (eddie_curl, guarded by EDDIE_LIBCURL).
# libcurl is linked dynamically: a static link would pull in many .a dependencies and require a lintian override.
# Build host must provide the libcurl development headers (Debian: libcurl4-openssl-dev); runtime depends on libcurl4.
g++ -shared -fPIC -o "$BASEPATH/bin/libLib.Platform.Linux.Native.so" "$BASEPATH/src/lib.cpp" -Wall -std=c++11 -O3 -lcurl -DEDDIE_LIBCURL -D$CONFIG

strip -S --strip-unneeded "$BASEPATH/bin/libLib.Platform.Linux.Native.so"
chmod a-x "$BASEPATH/bin/libLib.Platform.Linux.Native.so"

echo "Building libLib.Platform.Linux.Native.so - Done"
exit 0
