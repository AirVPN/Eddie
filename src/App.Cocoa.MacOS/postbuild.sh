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

# Note: folder structure are builded by "VS for Mac" AFTER the postbuild.sh call, for this we prebuild here one of the dirs
mkdir -p "$OUTPATH/Eddie-UI.app/Contents/MacOS/"
mkdir -p "$OUTPATH/Eddie-UI.app/Contents/MonoBundle/"

echo Compile and Copy Elevated
chmod +x "$BASEPATH/../App.CLI.MacOS.Elevated/build.sh"
"$BASEPATH/../App.CLI.MacOS.Elevated/build.sh" "$CONFIG"
cp "$BASEPATH/../App.CLI.MacOS.Elevated/bin/eddie-cli-elevated" "$OUTPATH/Eddie-UI.app/Contents/MacOS/"    
chmod +x "$BASEPATH/../App.CLI.MacOS.Elevated.Service/build.sh"
"$BASEPATH/../App.CLI.MacOS.Elevated.Service/build.sh" "$CONFIG"
cp "$BASEPATH/../App.CLI.MacOS.Elevated.Service/bin/eddie-cli-elevated-service" "$OUTPATH/Eddie-UI.app/Contents/MacOS/"    

echo Compile and Copy Native
chmod +x "${BASEPATH}/../Lib.Platform.MacOS.Native/build.sh"
"${BASEPATH}/../Lib.Platform.MacOS.Native/build.sh" "$CONFIG"
cp "$BASEPATH/../Lib.Platform.MacOS.Native/bin/libLib.Platform.MacOS.Native.dylib" "$OUTPATH/Eddie-UI.app/Contents/MonoBundle/"    

echo Copy Info.plist
cp "$BASEPATH/Info.plist" "$OUTPATH/Eddie-UI.app/Contents/"

exit 0
