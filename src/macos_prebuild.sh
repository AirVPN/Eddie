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
    echo Fourth arg must be config
    exit 1
fi


realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

BASEPATH=$(dirname $(realpath "$0"))
OUTPATH=$1
PROJECT=$2
ARCH=$3
CONFIG=$4

ARCH=$(uname -m)
if sysctl -a | grep ARM64 >/dev/null; then
    # Temp: This IF exists because when invoked by VisualStudio-For-Mac (at 2021-06-04 still x86_64), 'uname -m' return 'x86_64' even on M1 arch.
    # This exists in /src/App.CLI.MacOS.Elevated/build.sh and in /src/eddie.macos.prebuild.sh
    ARCH=arm64
fi
if [ "$ARCH" == "x86_64" ]; then
    ARCH=x64
fi

echo Basepath: $BASEPATH
echo TargetDir: $OUTPATH
echo Project: $PROJECT
echo Arch: $ARCH
echo Config: $CONFIG

VERSION=$(${BASEPATH}/../repository/macos_common/get-version.sh)

mkdir -p "$OUTPATH/Eddie.app/Contents/MacOS"

# Info.plist
if [ ${PROJECT} = "ui" ]; then
    cp "$BASEPATH/App.Cocoa.MacOS/Info-ui.plist" "$BASEPATH/App.Cocoa.MacOS/Info.plist"
    sed -E -i .bak "s/{@version}/${VERSION}/g" "$BASEPATH/App.Cocoa.MacOS/Info.plist"
    cp "$BASEPATH/App.Cocoa.MacOS/Info.plist" "$OUTPATH/Eddie.app/Contents/Info.plist"
elif [ ${PROJECT} = "ui3" ]; then
    cp "$BASEPATH/UI.Cocoa.MacOS/Info-ui.plist" "$BASEPATH/UI.Cocoa.MacOS/Info.plist"
    sed -E -i .bak "s/{@version}/${VERSION}/g" "$BASEPATH/UI.Cocoa.MacOS/Info.plist"
    cp "$BASEPATH/UI.Cocoa.MacOS/Info.plist" "$OUTPATH/Eddie.app/Contents/Info.plist"
elif [ ${PROJECT} = "cli" ]; then
    cp "$BASEPATH/App.CLI.MacOS/Info-cli.plist" "$BASEPATH/App.CLI.MacOS/Info.plist"
    sed -E -i .bak "s/{@version}/${VERSION}/g" "$BASEPATH/App.CLI.MacOS/Info.plist"
    cp "$BASEPATH/App.CLI.MacOS/Info.plist" "$OUTPATH/Eddie.app/Contents/Info.plist"
fi

# Adapt Elevated
# Search 'expectedOpenvpnHash' in '/src/App.CLI.Common.Elevated/ibase.cpp' source for details

ELEVATEDCSOURCEPATH=${BASEPATH}/App.CLI.Common.Elevated/hashes.h

OPENVPNPATH="${BASEPATH}/../deploy/macos_${ARCH}/openvpn"
OPENVPNHASH=$(openssl dgst -sha256 "${OPENVPNPATH}");
OPENVPNHASH=$(echo $OPENVPNHASH | cut -d "=" -f 2 | awk '{print $1}')
sed -E -i .bak "s/expectedOpenVpnHash = \"([0-9a-f]{64})\";/expectedOpenVpnHash = \"${OPENVPNHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

HUMMINGBIRDPATH="${BASEPATH}/../deploy/macos_${ARCH}/hummingbird"
HUMMINGBIRDHASH=$(openssl dgst -sha256 "${HUMMINGBIRDPATH}");
HUMMINGBIRDHASH=$(echo $HUMMINGBIRDHASH | cut -d "=" -f 2 | awk '{print $1}')
sed -E -i .bak "s/expectedHummingbirdHash = \"([0-9a-f]{64})\";/expectedHummingbirdHash = \"${HUMMINGBIRDHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

WIREGUARDGOPATH="${BASEPATH}/../deploy/macos_${ARCH}/wireguard-go"
WIREGUARDGOHASH=$(openssl dgst -sha256 "${WIREGUARDGOPATH}");
WIREGUARDGOHASH=$(echo $WIREGUARDGOHASH | cut -d "=" -f 2 | awk '{print $1}')
sed -E -i .bak "s/expectedWireGuardGoHash = \"([0-9a-f]{64})\";/expectedWireGuardGoHash = \"${WIREGUARDGOHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

WIREGUARDWGPATH="${BASEPATH}/../deploy/macos_${ARCH}/wg"
WIREGUARDWGHASH=$(openssl dgst -sha256 "${WIREGUARDWGPATH}");
WIREGUARDWGHASH=$(echo $WIREGUARDWGHASH | cut -d "=" -f 2 | awk '{print $1}')
sed -E -i .bak "s/expectedWireGuardWgHash = \"([0-9a-f]{64})\";/expectedWireGuardWgHash = \"${WIREGUARDWGHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

# Compile and Copy Elevated
chmod +x "$BASEPATH/App.CLI.MacOS.Elevated/build.sh"
"$BASEPATH/App.CLI.MacOS.Elevated/build.sh" "$CONFIG"

if [ $PROJECT = "ui" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MacOS/"
    cp "$BASEPATH/App.CLI.MacOS.Elevated/bin/eddie-cli-elevated" "$OUTPATH/Eddie.app/Contents/MacOS/"
elif [ $PROJECT = "ui3" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MacOS/"
    cp "$BASEPATH/App.CLI.MacOS.Elevated/bin/eddie-cli-elevated" "$OUTPATH/Eddie.app/Contents/MacOS/"
elif [ $PROJECT = "cli" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MacOS/"
    cp "$BASEPATH/App.CLI.MacOS.Elevated/bin/eddie-cli-elevated" "$OUTPATH/Eddie.app/Contents/MacOS/"
else
    echo Unexpected
fi

# Signing

${BASEPATH}/../repository/macos_common/sign.sh "$OUTPATH/Eddie.app/Contents/MacOS/eddie-cli-elevated" no yes

# Compile and Copy Native

chmod +x "${BASEPATH}/Lib.Platform.MacOS.Native/build.sh"
"${BASEPATH}/Lib.Platform.MacOS.Native/build.sh" "$CONFIG"

if [ $PROJECT = "ui" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MonoBundle/"
    cp "$BASEPATH/Lib.Platform.MacOS.Native/bin/libLib.Platform.MacOS.Native.dylib" "$OUTPATH/Eddie.app/Contents/MonoBundle/"
elif [ $PROJECT = "ui3" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MonoBundle/"
    cp "$BASEPATH/Lib.Platform.MacOS.Native/bin/libLib.Platform.MacOS.Native.dylib" "$OUTPATH/Eddie.app/Contents/MonoBundle/"
elif [ $PROJECT = "cli" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MonoBundle/"
    cp "$BASEPATH/Lib.Platform.MacOS.Native/bin/libLib.Platform.MacOS.Native.dylib" "$OUTPATH/Eddie.app/Contents/MonoBundle/"
fi

# Signing
if [ $CONFIG = "Release" ]; then
    ${BASEPATH}/../repository/macos_common/sign.sh "$OUTPATH/Eddie.app/Contents/MonoBundle/libLib.Platform.MacOS.Native.dylib" no yes
fi

exit 0
