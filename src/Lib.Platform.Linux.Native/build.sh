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


echo "Building UI linux native"
echo "Config: $CONFIG"

# Dynamic edition

# Static edition
# At 2019-09 for example, we compile from Debian8, and dynamic edition don't work with latest CentOS7.6.
# Contro: biggest exe file, and lintian need an override (bundled in .deb packages).

#g++ -shared -fPIC -o "$BASEPATH/bin/libLib.Platform.Linux.Native.so" "$BASEPATH/src/api.cpp" -Wall -std=c++11 -O3 -static -pthread -Wl,--whole-archive -lpthread -Wl,--no-whole-archive -D$1

# Version without libcurl (C#::Platform::FetchUrlInternal=false)
g++ -shared -fPIC -o "$BASEPATH/bin/libLib.Platform.Linux.Native.so" "$BASEPATH/lib.cpp" -Wall -std=c++11 -O3 -D$CONFIG

# Version libcurl - unresolved deploy compatibility issues libcurl3 vs libcurl4, CURL_OPENSSL_3 issue etc. And remember need AppImage/Portable static edition.
# When compiled in our Debian8 (that have libcurl4-openssl-dev), result binary throw in recent linux with libcurl4 "version CURL_OPENSSL_3 not found". Dependencies with libcurl3 is excluded, will uninstall other software.
# Linking libcurl statically maybe a solution, but complex (a lots of .a dependencies) and require lintian override (and generally not recommended).
# Until solution, Eddie Linux still use curl binary with shell.
#g++ -shared -fPIC -o "$BASEPATH/bin/libLib.Platform.Linux.Native.so" "$BASEPATH/src/api.cpp" -Wall -std=c++11 -O3 -lcurl -DEDDIE_LIBCURL -D$CONFIG

strip -S --strip-unneeded "$BASEPATH/bin/libLib.Platform.Linux.Native.so"
chmod a-x "$BASEPATH/bin/libLib.Platform.Linux.Native.so"

exit 0
