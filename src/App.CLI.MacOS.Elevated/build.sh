#!/bin/sh

set -e

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

if [ "$1" == "" ]; then
    echo First arg must be Config, 'Debug' or 'Release'
    exit 1
fi

BASEPATH=$(dirname $(realpath "$0"))
CONFIG="$1"
FILES=""

echo "Build eddie-cli-elevated - Config: $CONFIG"

rm -rf "$BASEPATH/bin"
mkdir -p "$BASEPATH/bin"

FILES="${FILES} $BASEPATH/src/main.cpp"
FILES="${FILES} $BASEPATH/src/impl.cpp"
FILES="${FILES} $BASEPATH/../Lib.CLI.Elevated/src/iposix.cpp"
FILES="${FILES} $BASEPATH/../Lib.CLI.Elevated/src/ibase.cpp"
FILES="${FILES} $BASEPATH/../Lib.CLI.Elevated/src/ping.cpp"
FILES="${FILES} $BASEPATH/../../dependencies/sha256/sha256.cpp"

ARCH=$(uname -m)
if sysctl -a | grep ARM64 >/dev/null; then
    # Temp: This IF exists because when invoked by VisualStudio-For-Mac (at 2021-06-04 still x86_64), 'uname -m' return 'x86_64' even on M1 arch.
    # This exists in 
    # /src/App.CLI.MacOS.Elevated/build.sh 
    # /src/App.CLI.MacOS/postbuild.sh
    # /src/App.Cocoa.MacOS/postbuild.sh
    # TOFIX, remove when VisualStudio-For-Mac are not used anymore.
    ARCH=arm64
fi

g++ -mmacosx-version-min=10.9 -arch ${ARCH} -o "$BASEPATH/bin/eddie-cli-elevated" ${FILES} -Wall -std=c++11 -O3 -pthread -lpthread -D$1

chmod a+x "$BASEPATH/bin/eddie-cli-elevated"
echo Build eddie-cli-elevated - Done
exit 0
