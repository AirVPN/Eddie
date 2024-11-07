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

FILES=""
FLAGS=""
DEFINES=""
CONFIG="$1"

echo "Building eddie-cli-elevated-service - Config: $CONFIG"

rm -rf "$BASEPATH/bin"
rm -rf "$BASEPATH/obj"
mkdir -p "$BASEPATH/bin"
mkdir -p "$BASEPATH/obj"

FILES="${FILES} $BASEPATH/src/main.cpp"
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

# g++ -o "$BASEPATH/bin/eddie-cli-elevated-service" ${FILES} -Wall -std=c++11 -O3 -static -D$1 ${DEFINES}
g++ -mmacosx-version-min=10.9 -arch ${ARCH} -o "$BASEPATH/bin/eddie-cli-elevated-service" ${FILES} -Wall -std=c++11 -O3 ${FLAGS} -D$1 ${DEFINES}

chmod a+x "$BASEPATH/bin/eddie-cli-elevated-service"

echo "Building eddie-cli-elevated-service - Done"

exit 0
