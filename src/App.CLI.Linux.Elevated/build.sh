#!/bin/bash

set -e

#if [ $1 -eq "" ]; then
#    echo First arg must be Config, 'Debug' or 'Release'
#    exit 1
#fi

SPECIAL="$2"

BASEPATH=$(dirname $(realpath -s $0))
mkdir -p "$BASEPATH/bin"
mkdir -p "$BASEPATH/obj"

FILES=""
FLAGS=""
DEFINES=""

#if [ ${SPECIAL} == "NOLZMA" ]; then
#	echo Link without LZMA	
#    DEFINES="${DEFINES} -DEDDIE_NOLZMA"
#else 
#	echo Link with LZMA
#	FLAGS="${FLAGS} -llzma"
#	FILES="${FILES} $BASEPATH/src/loadmod.c"
#fi

# WireGuard functions
gcc -c "$BASEPATH/src/wireguard.c" -o "$BASEPATH/obj/wireguard.o"

# Dynamic edition
#g++ -o "$BASEPATH/bin/eddie-cli-elevated" "$BASEPATH/src/main.cpp" "$BASEPATH/src/impl.cpp" "$BASEPATH/../App.CLI.Common.Elevated/common.cpp" "$BASEPATH/../App.CLI.Common.Elevated/sha256.cpp" -Wall -std=c++11 -O3 -pthread -lpthread -D$1

# Static edition
# At 2019-09 for example, we compile from Debian8, and dynamic edition don't work with latest CentOS7.6.
# Contro: biggest exe file, and lintian need an override (bundled in .deb packages).
# Remember: switch position of pthread can compile but cause a "Segmentation fault".
g++ -o "$BASEPATH/bin/eddie-cli-elevated" "$BASEPATH/src/main.cpp" "$BASEPATH/src/impl.cpp" "$BASEPATH/../App.CLI.Common.Elevated/iposix.cpp" "$BASEPATH/../App.CLI.Common.Elevated/ibase.cpp" "$BASEPATH/../App.CLI.Common.Elevated/ping.cpp" "$BASEPATH/../App.CLI.Common.Elevated/sha256.cpp" "$BASEPATH/obj/wireguard.o" ${FILES} -Wall -std=c++11 -O3 -static -pthread -Wl,--whole-archive -lpthread ${FLAGS} -Wl,--no-whole-archive -D$1 ${DEFINES}
#g++ -o "$BASEPATH/bin/eddie-cli-elevated" "$BASEPATH/src/main.cpp" "$BASEPATH/src/impl.cpp" "$BASEPATH/../App.CLI.Common.Elevated/iposix.cpp" "$BASEPATH/../App.CLI.Common.Elevated/ibase.cpp" "$BASEPATH/../App.CLI.Common.Elevated/sha256.c" ${FILES} -Wall -std=c++11 -O3 -static -pthread -Wl,--whole-archive -lpthread ${FLAGS} -Wl,--no-whole-archive -D$1 ${DEFINES}

strip -S --strip-unneeded "$BASEPATH/bin/eddie-cli-elevated"
chmod a+x "$BASEPATH/bin/eddie-cli-elevated"
echo Done
exit 0
