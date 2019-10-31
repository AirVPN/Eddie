#!/bin/sh

set -e

#if [ $1 -eq "" ]; then
#    echo First arg must be Config, 'Debug' or 'Release'
#    exit 1
#fi

BASEPATH=$(dirname $(realpath -s $0))
mkdir -p "$BASEPATH/bin"

# Dynamic edition
#g++ -o "$BASEPATH/bin/eddie-cli-elevated" "$BASEPATH/src/main.cpp" "$BASEPATH/src/impl.cpp" "$BASEPATH/../App.CLI.Common.Elevated.C/common.cpp" "$BASEPATH/../App.CLI.Common.Elevated.C/sha256.cpp" -Wall -std=c++11 -O3 -pthread -lpthread -D$1

# Static edition
# At 2019-09 for example, we compile from Debian8, and dynamic edition don't work with latest CentOS7.6.
# Contro: biggest exe file, and lintian need an override (bundled in .deb packages).
g++ -o "$BASEPATH/bin/eddie-cli-elevated" "$BASEPATH/src/main.cpp" "$BASEPATH/src/impl.cpp" "$BASEPATH/../App.CLI.Common.Elevated.C/iposix.cpp" "$BASEPATH/../App.CLI.Common.Elevated.C/ibase.cpp" "$BASEPATH/../App.CLI.Common.Elevated.C/sha256.cpp" -Wall -std=c++11 -O3 -static -pthread -Wl,--whole-archive -lpthread -Wl,--no-whole-archive -D$1

strip -S --strip-unneeded "$BASEPATH/bin/eddie-cli-elevated"
chmod a+x "$BASEPATH/bin/eddie-cli-elevated"
echo Done
exit 0
