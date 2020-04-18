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
g++ -mmacosx-version-min=10.9 -o "$BASEPATH/bin/eddie-cli-elevated" "$BASEPATH/src/main.cpp" "$BASEPATH/src/impl.cpp" "$BASEPATH/../App.CLI.Common.Elevated/iposix.cpp" "$BASEPATH/../App.CLI.Common.Elevated/ibase.cpp" "$BASEPATH/../App.CLI.Common.Elevated/sha256.cpp" -Wall -std=c++11 -O3 -pthread -lpthread -D$1

chmod a+x "$BASEPATH/bin/eddie-cli-elevated"
echo Done
exit 0
