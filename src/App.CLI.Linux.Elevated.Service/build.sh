#!/bin/bash

set -e

if [ "$1" == "" ]; then
    echo First arg must be Config, 'Debug' or 'Release'
    exit 1
fi

BASEPATH=$(dirname $(realpath -s $0))

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

# g++ -o "$BASEPATH/bin/eddie-cli-elevated-service" ${FILES} -Wall -std=c++11 -O3 -static -D$1 ${DEFINES}
g++ -o "$BASEPATH/bin/eddie-cli-elevated-service" ${FILES} -Wall -std=c++11 -O3 ${FLAGS} -D$1 ${DEFINES}

strip -S --strip-unneeded "$BASEPATH/bin/eddie-cli-elevated-service" 
chmod a+x "$BASEPATH/bin/eddie-cli-elevated-service"

echo "Building eddie-cli-elevated-service - Done"

exit 0
