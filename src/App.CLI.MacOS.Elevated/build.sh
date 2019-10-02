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

# Dynamic edition
g++ -o "$BASEPATH/bin/eddie-cli-elevated" "$BASEPATH/src/main.cpp" "$BASEPATH/src/impl.cpp" "$BASEPATH/../App.CLI.Common.Elevated.C/common.cpp" "$BASEPATH/../App.CLI.Common.Elevated.C/sha256.cpp" -Wall -std=c++11 -O3 -pthread -lpthread -D$1

# Static edition - Used in Linux
#g++ -o "$BASEPATH/bin/eddie-cli-elevated" "$BASEPATH/src/main.cpp" "$BASEPATH/src/impl.cpp" "$BASEPATH/../App.CLI.Common.Elevated.C/common.cpp" "$BASEPATH/../App.CLI.Common.Elevated.C/sha256.cpp" -Wall -std=c++11 -O3 -static -pthread -Wl,--whole-archive -lpthread -Wl,--no-whole-archive -D$1

chmod a+x "$BASEPATH/bin/eddie-cli-elevated"
echo Done
exit 0
