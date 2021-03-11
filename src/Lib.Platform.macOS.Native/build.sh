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

#g++ -mmacosx-version-min=10.9 -shared -install_name @rpath/libLib.Platform.MacOS.Native.dylib -fPIC -o "$BASEPATH/bin/libLib.Platform.MacOS.Native.dylib" "$BASEPATH/src/api.cpp" -Wall -std=c++11 -lcurl -O3 -D$1
#M1: need arch as param, after use -arch arm64   OR -arch x86_64

# Don't force any architecture (-arch), it's called by "VS for Mac" and must follow the same arch of parent.
# So, under Apple M1, at 2020-02-02, it's compiled with x86_64.
g++ -mmacosx-version-min=10.9 -shared -install_name @rpath/libLib.Platform.MacOS.Native.dylib -fPIC -o "$BASEPATH/bin/libLib.Platform.MacOS.Native.dylib" "$BASEPATH/src/api.cpp" -Wall -std=c++11 -lcurl -O3 -D$1
#g++ -mmacosx-version-min=10.9 -arch x86_64 -shared -install_name @rpath/libLib.Platform.MacOS.Native.dylib -fPIC -o "$BASEPATH/bin/libLib.Platform.MacOS.Native.dylib" "$BASEPATH/src/api.cpp" -Wall -std=c++11 -lcurl -O3 -D$1

chmod a-x "$BASEPATH/bin/libLib.Platform.MacOS.Native.dylib"

exit 0
