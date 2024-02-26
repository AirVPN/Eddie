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
	echo Second arg must be architecture or runtime
	exit 1
fi

if [ "$3" == "" ]; then
    echo Third arg must be config
    exit 1
fi


realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

BASEPATH=$(dirname $(realpath "$0"))
OUTPATH=$1
ARCH=$2
CONFIG=$3

# If arch is a RuntimeIdentifier
if [ "$ARCH" == "osx-arm64" ]; then
    ARCH="arm64"
fi
if [ "$ARCH" == "osx-x64" ]; then
    ARCH="x64"
fi

ARCH=$(uname -m)
if sysctl -a | grep ARM64 >/dev/null; then
    # Temp: This IF exists because when invoked by VisualStudio-For-Mac (at 2021-06-04 still x86_64), 'uname -m' return 'x86_64' even on M1 arch.
    # This exists in 
    # /src/App.CLI.MacOS.Elevated/build.sh 
    # /src/App.CLI.MacOS/postbuild.sh
    # /src/App.Cocoa.MacOS/postbuild.sh
    # TOFIX, remove when VisualStudio-For-Mac are not used anymore.
    ARCH=arm64
fi
if [ "$ARCH" == "x86_64" ]; then
    ARCH=x64
fi

echo MacOS PostBuild script
echo Basepath: $BASEPATH
echo TargetDir: $OUTPATH
echo Arch: $ARCH
echo Config: $CONFIG

echo Adapt Elevated
# Search 'expectedOpenvpnHash' in '/src/App.CLI.Common.Elevated/ibase.cpp' source for details
ELEVATEDCSOURCEPATH=${BASEPATH}/../Lib.CLI.Elevated/include/hashes.h

OPENVPNPATH="${BASEPATH}/../../deploy/macos_${ARCH}/openvpn"
OPENVPNHASH=$(openssl dgst -sha256 "${OPENVPNPATH}");
OPENVPNHASH=$(echo $OPENVPNHASH | cut -d "=" -f 2 | awk '{print $1}')
sed -E -i .bak "s/expectedOpenVpnHash = \"([0-9a-f]{64})\";/expectedOpenVpnHash = \"${OPENVPNHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

HUMMINGBIRDPATH="${BASEPATH}/../../deploy/macos_${ARCH}/hummingbird"
HUMMINGBIRDHASH=$(openssl dgst -sha256 "${HUMMINGBIRDPATH}");
HUMMINGBIRDHASH=$(echo $HUMMINGBIRDHASH | cut -d "=" -f 2 | awk '{print $1}')
sed -E -i .bak "s/expectedHummingbirdHash = \"([0-9a-f]{64})\";/expectedHummingbirdHash = \"${HUMMINGBIRDHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

WIREGUARDGOPATH="${BASEPATH}/../../deploy/macos_${ARCH}/wireguard-go"
WIREGUARDGOHASH=$(openssl dgst -sha256 "${WIREGUARDGOPATH}");
WIREGUARDGOHASH=$(echo $WIREGUARDGOHASH | cut -d "=" -f 2 | awk '{print $1}')
sed -E -i .bak "s/expectedWireGuardGoHash = \"([0-9a-f]{64})\";/expectedWireGuardGoHash = \"${WIREGUARDGOHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

WIREGUARDWGPATH="${BASEPATH}/../../deploy/macos_${ARCH}/wg"
WIREGUARDWGHASH=$(openssl dgst -sha256 "${WIREGUARDWGPATH}");
WIREGUARDWGHASH=$(echo $WIREGUARDWGHASH | cut -d "=" -f 2 | awk '{print $1}')
sed -E -i .bak "s/expectedWireGuardWgHash = \"([0-9a-f]{64})\";/expectedWireGuardWgHash = \"${WIREGUARDWGHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

echo Compile and Copy Elevated
chmod +x "$BASEPATH/../App.CLI.MacOS.Elevated/build.sh"
"$BASEPATH/../App.CLI.MacOS.Elevated/build.sh" "$CONFIG"
cp "$BASEPATH/../App.CLI.MacOS.Elevated/bin/eddie-cli-elevated" "$OUTPATH/"    

echo Compile and Copy Native
chmod +x "${BASEPATH}/../Lib.Platform.MacOS.Native/build.sh"
"${BASEPATH}/../Lib.Platform.MacOS.Native/build.sh" "$CONFIG"
cp "$BASEPATH/../Lib.Platform.MacOS.Native/bin/libLib.Platform.MacOS.Native.dylib" "$OUTPATH/"    

exit 0
