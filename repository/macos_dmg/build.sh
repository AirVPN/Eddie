#!/bin/bash

set -e

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

if [ "$1" == "" ]; then
    echo First arg must be Project: cli,ui
    exit 1
fi

PROJECT=$1
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath "$0"))
ARCH=$($SCRIPTDIR/../macos_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../macos_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_macos_${ARCH}_disk.dmg
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_macos_${ARCH}_disk.dmg

mkdir -p ${SCRIPTDIR}/../files

if test -f "${DEPLOYPATH}"; then
    echo "Already builded: ${DEPLOYPATH}"
    exit 0;
fi

# Cleanup
rm -rf $TARGETDIR

# Package dependencies
echo Step: Package dependencies - Build Portable
"${SCRIPTDIR}/../macos_portable/build.sh" ${PROJECT}
mkdir -p ${TARGETDIR}
DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_macos_${ARCH}_portable.tar.gz  

# DMG

# Extract base
tar -jxvf "${SCRIPTDIR}/diskbase.dmg.tar.bz2" -C "${TARGETDIR}"
# Resize
hdiutil resize -size 200m "${TARGETDIR}/diskbase.dmg"
# Attach
hdiutil attach "${TARGETDIR}/diskbase.dmg" -mountpoint "${TARGETDIR}/DiskBuild"
# Fill
tar -jxvf "${DEPPACKAGEPATH}" -C "${TARGETDIR}/DiskBuild/"
# Detach
hdiutil detach "${TARGETDIR}/DiskBuild"
# Compress
hdiutil convert "${TARGETDIR}/diskbase.dmg" -format UDCO -imagekey zlib-level=9 -o "${DEPLOYPATH}"



# Deploy to eddie.website
${SCRIPTDIR}/../macos_common/deploy.sh ${DEPLOYPATH}

# Cleanup - with sudo because AppImage create files as root
rm -rf $TARGETDIR

