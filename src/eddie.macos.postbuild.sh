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

echo Basepath: $BASEPATH
echo TargetDir: $OUTPATH
echo Project: $PROJECT
echo Arch: $ARCH
echo Config: $CONFIG

VERSION=$(${BASEPATH}/../repository/macos_common/get-version.sh)

mkdir -p "$OUTPATH"

# Copy Native
if [ $PROJECT = "ui" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MonoBundle/"
    cp "$BASEPATH/../deploy/macos_x64/libLib.Platform.macOS.Native.dylib" "$OUTPATH/Eddie.app/Contents/MonoBundle/"
elif [ $PROJECT = "ui3" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MonoBundle/"
    cp "$BASEPATH/../deploy/macos_x64/libLib.Platform.macOS.Native.dylib" "$OUTPATH/Eddie.app/Contents/MonoBundle/"
elif [ $PROJECT = "cli" ]; then
    cp "$BASEPATH/../deploy/macos_x64/libLib.Platform.macOS.Native.dylib" "$OUTPATH/"
fi

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
fi



# Adapt Elevated
# Search 'expectedOpenvpnHash' in '/src/App.CLI.Common.Elevated.C/ibase.cpp' source for details

ELEVATEDCSOURCEPATH=${BASEPATH}/App.CLI.Common.Elevated.C/ibase.cpp

OPENVPNPATH="${BASEPATH}/../deploy/macos_${ARCH}/openvpn"
OPENVPNHASH=$(openssl dgst -sha256 "${OPENVPNPATH}");
OPENVPNHASH=$(echo $OPENVPNHASH | cut -d "=" -f 2 | awk '{print $1}') # Remember: test with openvpn path with whitespace
sed -E -i .bak "s/expectedOpenvpnHash = \"([0-9a-f]{64})\";/expectedOpenvpnHash = \"${OPENVPNHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

HUMMINGBIRDPATH="${BASEPATH}/../deploy/macos_${ARCH}/hummingbird"
HUMMINGBIRDHASH=$(openssl dgst -sha256 "${HUMMINGBIRDPATH}");
HUMMINGBIRDHASH=$(echo $HUMMINGBIRDHASH | cut -d "=" -f 2 | awk '{print $1}') # Remember: test with hummingbird path with whitespace
sed -E -i .bak "s/expectedHummingbirdHash = \"([0-9a-f]{64})\";/expectedHummingbirdHash = \"${HUMMINGBIRDHASH}\";/g" "${ELEVATEDCSOURCEPATH}"

# Compile and Copy Elevated
"$BASEPATH/App.CLI.MacOS.Elevated/build.sh" "$CONFIG"

if [ $PROJECT = "ui" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MacOS/"
    cp "$BASEPATH/App.CLI.MacOS.Elevated/bin/eddie-cli-elevated" "$OUTPATH/Eddie.app/Contents/MacOS/"
elif [ $PROJECT = "ui3" ]; then
    mkdir -p "$OUTPATH/Eddie.app/Contents/MacOS/"
    cp "$BASEPATH/App.CLI.MacOS.Elevated/bin/eddie-cli-elevated" "$OUTPATH/Eddie.app/Contents/MacOS/"
elif [ $PROJECT = "cli" ]; then
    cp "$BASEPATH/App.CLI.MacOS.Elevated/bin/eddie-cli-elevated" "$OUTPATH/"
else
    echo Unexpected
fi

