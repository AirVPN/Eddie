#!/bin/bash

set -e

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

BASEPATH=$(dirname $(realpath "$0"))
mkdir -p "$BASEPATH/bin"

# Don't force any architecture (-arch), it's called by "VS for Mac" and must follow the same arch of parent. So will be x86_64 with net4, native with net7.

g++ -dynamiclib -framework CoreFoundation -framework Security -mmacosx-version-min=10.9 -shared -install_name @rpath/libLib.Platform.MacOS.Native.dylib -fPIC -o "$BASEPATH/bin/libLib.Platform.MacOS.Native.dylib" "$BASEPATH/src/lib.cpp" -Wall -std=c++11 -lcurl -O3 -D$1

chmod a-x "$BASEPATH/bin/libLib.Platform.MacOS.Native.dylib"

exit 0
