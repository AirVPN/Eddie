#!/bin/bash

set -e

# Check args
if [ "$1" == "" ]; then
	echo First arg must be Output directory
	exit 1
fi

if [ ! -d "$1" ]; then
	echo Output directory not exists
	exit 1
fi

if [ "$2" == "" ]; then
    echo Second arg must be project - cli/ui/ui3
    exit 1
fi

if [ "$3" == "" ]; then
	echo Third arg must be architecture
	exit 1
fi

if [ "$4" == "" ]; then
    echo Fourty arg must be config
    exit 1
fi

BASEPATH=$(dirname $(realpath -s $0))
OUTPATH=$1
PROJECT=$2
ARCH=$3
CONFIG=$4

echo BasePath: $BASEPATH
echo TargetDir: $OUTPATH
echo Project: $PROJECT
echo Arch: $ARCH
echo Config: $CONFIG

if [ $PROJECT = "ui" ]; then
    # Copy Tray
    cp "$BASEPATH/../deploy/linux_$ARCH/eddie-tray" "$OUTPATH"
fi

# Adapt Elevated
# Search 'expectedOpenvpnHash' in '/src/App.CLI.Common.Elevated.C/ibase.cpp' source for details

OPENVPNPATH="${BASEPATH}/../deploy/linux_${ARCH}/openvpn"
ELEVATEDCSOURCEPATH=${BASEPATH}/App.CLI.Common.Elevated.C/ibase.cpp

COMPUTEHASH=$(sha256sum "${OPENVPNPATH}");
COMPUTEHASH=${COMPUTEHASH%% *}

sed -ri "s/expectedOpenvpnHash = \"([0-9a-f]{64})\";/expectedOpenvpnHash = \"${COMPUTEHASH}\";/g" ${ELEVATEDCSOURCEPATH}

# Compile and Copy Elevated
"$BASEPATH/App.CLI.Linux.Elevated/build.sh" "$CONFIG"
cp "$BASEPATH/App.CLI.Linux.Elevated/bin/eddie-cli-elevated" "$OUTPATH"

# Compile and Copy Native
"${BASEPATH}/Lib.Platform.Linux.Native/build.sh" "$CONFIG"
#cd "${BASEPATH}/Lib.Platform.Linux.Native"
#if [ $ARCH = "armhf" ]; then
#	./build-armhf.sh	
#else
#	./build-multilib.sh
#fi
cp "$BASEPATH/Lib.Platform.Linux.Native/bin/libLib.Platform.Linux.Native.so" "$OUTPATH"

