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
mkdir -p "$BASEPATH/bin"

VARARCH=$(uname -m)
if sysctl -a | grep ARM64 >/dev/null; then
    # Temp: This IF exists because when invoked by VisualStudio-For-Mac (at 2021-06-04 still x86_64), 'uname -m' return 'x86_64' even on M1 arch.
    # This exists in /src/App.CLI.MacOS.Elevated/build.sh and in /src/eddie.macos.prebuild.sh
    VARARCH=arm64
fi

g++ -mmacosx-version-min=10.9 -arch ${VARARCH} -o "$BASEPATH/bin/eddie-cli-elevated" "$BASEPATH/src/main.cpp" "$BASEPATH/src/impl.cpp" "$BASEPATH/../App.CLI.Common.Elevated/iposix.cpp" "$BASEPATH/../App.CLI.Common.Elevated/ibase.cpp" "$BASEPATH/../App.CLI.Common.Elevated/ping.cpp" "$BASEPATH/../App.CLI.Common.Elevated/sha256.cpp" -Wall -std=c++11 -O3 -pthread -lpthread -D$1

chmod a+x "$BASEPATH/bin/eddie-cli-elevated"
echo Done
exit 0
