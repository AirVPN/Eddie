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
	echo Third arg must be architecture or runtime
	exit 1
fi

if [ "$4" == "" ]; then
    echo Fourty arg must be config - Debug/Relese
    exit 1
fi

# 2020-04-20: Forced 'static'. We build official packages from Debian8, and there is too much issues libcurl3 vs libcurl4 and packages dependencies on every linux distro supported.
# Note: we need to use a debian-lintian exception for 'static'.
# if [ "$5" == "" ]; then
#    echo Fifty arg must be mode - shared/static
#    exit 1
# fi

BASEPATH=$(dirname $(realpath -s $0))
OUTPATH=$1
PROJECT=$2
ARCH=$3
CONFIG=$4
MODE=shared

# If arch is a RuntimeIdentifier
if [ "$ARCH" == "linux-x64" ]; then
    ARCH="x64"
fi
if [ "$ARCH" == "linux-x86" ]; then
    ARCH="x86"
fi

echo BasePath: $BASEPATH
echo TargetDir: $OUTPATH
echo Project: $PROJECT
echo Arch: $ARCH
echo Config: $CONFIG
#echo Mode: $MODE # Not used anymore

#if [ $PROJECT = "ui" ]; then
#    # Copy Tray
#    cp "$BASEPATH/../deploy/linux_$ARCH/eddie-tray" "$OUTPATH"
#fi

# Adapt Elevated
# Search 'expectedOpenvpnHash' in '/src/App.CLI.Common.Elevated/ibase.cpp' source for details

ELEVATEDCSOURCEPATH=${BASEPATH}/App.CLI.Common.Elevated/hashes.h

OPENVPNPATH="${BASEPATH}/../deploy/linux_${ARCH}/openvpn"
OPENVPNHASH=$(sha256sum "${OPENVPNPATH}");
OPENVPNHASH=${OPENVPNHASH%% *}
sed -ri "s/expectedOpenVpnHash = \"([0-9a-f]{64})\";/expectedOpenVpnHash = \"${OPENVPNHASH}\";/g" ${ELEVATEDCSOURCEPATH}

HUMMINGBIRDPATH="${BASEPATH}/../deploy/linux_${ARCH}/hummingbird"
if test -f "${HUMMINGBIRDPATH}"; then    
    HUMMINGBIRDHASH=$(sha256sum "${HUMMINGBIRDPATH}");
    HUMMINGBIRDHASH=${HUMMINGBIRDHASH%% *}
    sed -ri "s/expectedHummingbirdHash = \"([0-9a-f]{64})\";/expectedHummingbirdHash = \"${HUMMINGBIRDHASH}\";/g" ${ELEVATEDCSOURCEPATH}
fi

# Compile and Copy Elevated
ELEVATED_SPECIAL="STANDARD"
if [ -f "/etc/arch-release" ]; then
	ELEVATED_SPECIAL="NOLZMA"
fi
chmod +x "$BASEPATH/App.CLI.Linux.Elevated/build.sh"
"$BASEPATH/App.CLI.Linux.Elevated/build.sh" "$CONFIG" "$ELEVATED_SPECIAL"
cp "$BASEPATH/App.CLI.Linux.Elevated/bin/eddie-cli-elevated" "$OUTPATH"

# Compile and Copy Native
chmod +x "${BASEPATH}/Lib.Platform.Linux.Native/build.sh"
"${BASEPATH}/Lib.Platform.Linux.Native/build.sh" "$CONFIG" "$MODE"
cp "$BASEPATH/Lib.Platform.Linux.Native/bin/libLib.Platform.Linux.Native.so" "$OUTPATH"

# Compile and Copy eddie-tray
if [ $PROJECT = "ui" ]; then
    chmod +x "${BASEPATH}/UI.GTK.Linux.Tray/build.sh"
    "${BASEPATH}/UI.GTK.Linux.Tray/build.sh" "$CONFIG"
    # If not build (it's optional), skip 
    if [ -f "$BASEPATH/UI.GTK.Linux.Tray/bin/eddie-tray" ]; then
        cp "$BASEPATH/UI.GTK.Linux.Tray/bin/eddie-tray" "$OUTPATH"
    fi
fi

