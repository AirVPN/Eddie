#!/bin/bash

set -e

#if [ $1 -eq "" ]; then
#    echo First arg must be Config, 'Debug' or 'Release'
#    exit 1
#fi

BASEPATH=$(dirname $(realpath -s $0))
mkdir -p "$BASEPATH/bin"

# Dynamic edition

# Static edition
# At 2019-09 for example, we compile from Debian8, and dynamic edition don't work with latest CentOS7.6.
# Contro: biggest exe file, and lintian need an override (bundled in .deb packages).

#g++ -shared -fPIC -o "$BASEPATH/bin/libLib.Platform.Linux.Native.so" "$BASEPATH/src/api.cpp" -Wall -std=c++11 -O3 -static -pthread -Wl,--whole-archive -lpthread -Wl,--no-whole-archive -D$1
g++ -shared -fPIC -o "$BASEPATH/bin/libLib.Platform.Linux.Native.so" "$BASEPATH/src/api.cpp" -Wall -lcurl -std=c++11 -O3 -D$1

strip -S --strip-unneeded "$BASEPATH/bin/libLib.Platform.Linux.Native.so"
chmod a-x "$BASEPATH/bin/libLib.Platform.Linux.Native.so"

exit 0
