#!/bin/bash

set -e

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

#if [ $1 -eq "" ]; then
#    echo First arg must be Config, 'Debug' or 'Release'
#    exit 1
#fi

BASEPATH=$(dirname $(realpath "$0"))
mkdir -p "$BASEPATH/bin"

g++ -mmacosx-version-min=10.9 -shared -fPIC -o "$BASEPATH/bin/libLib.Platform.MacOS.Native.dylib" "$BASEPATH/src/api.cpp" -Wall -std=c++11 -lcurl -O3 -D$1

chmod a-x "$BASEPATH/bin/libLib.Platform.MacOS.Native.dylib"

exit 0
