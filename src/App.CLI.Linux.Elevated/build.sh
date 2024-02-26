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
SHARED="yes" # See pthread_static_issue/build.sh

echo "Building eddie-cli-elevated - Config: $CONFIG, Shared: $SHARED"

rm -rf "$BASEPATH/bin"
rm -rf "$BASEPATH/obj"
mkdir -p "$BASEPATH/bin"
mkdir -p "$BASEPATH/obj"

FILES="${FILES} $BASEPATH/src/main.cpp"
FILES="${FILES} $BASEPATH/src/impl.cpp"
FILES="${FILES} $BASEPATH/../Lib.CLI.Elevated/src/iposix.cpp"
FILES="${FILES} $BASEPATH/../Lib.CLI.Elevated/src/ibase.cpp"
FILES="${FILES} $BASEPATH/../Lib.CLI.Elevated/src/ping.cpp"
FILES="${FILES} $BASEPATH/../../dependencies/sha256/sha256.cpp"
FILES="${FILES} $BASEPATH/obj/wireguard.o"

#SPECIAL="$2"
#if [ ${SPECIAL} == "NOLZMA" ]; then
#	echo Link without LZMA	
#    DEFINES="${DEFINES} -DEDDIE_NOLZMA"
#else 
#	echo Link with LZMA
#	FLAGS="${FLAGS} -llzma"
#	FILES="${FILES} $BASEPATH/src/loadmod.c"
#fi

#if [ -f "/etc/arch-release" ]; then
#	SHARED="no"
#fi

# WireGuard functions
gcc -c "$BASEPATH/src/wireguard.c" -o "$BASEPATH/obj/wireguard.o"

if [ $SHARED = "yes" ]; then
	g++ -o "$BASEPATH/bin/eddie-cli-elevated" ${FILES} -Wall -std=c++11 -O3 -pthread -lpthread ${FLAGS} -D$1 ${DEFINES}
else
	# throw segmentation fault in some distro, see pthread_static_issue/build.sh
	g++ -o "$BASEPATH/bin/eddie-cli-elevated" ${FILES} -Wall -std=c++11 -O3 -static -pthread -Wl,--whole-archive -lpthread ${FLAGS} -Wl,--no-whole-archive -D$1 ${DEFINES}
fi

strip -S --strip-unneeded "$BASEPATH/bin/eddie-cli-elevated" 
chmod a+x "$BASEPATH/bin/eddie-cli-elevated"

echo "Building eddie-cli-elevated - Done"

exit 0
