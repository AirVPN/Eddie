#!/bin/bash

set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}
SCRIPTDIR=$(dirname $(realpath "$0"))

if [ "$1" == "" ]; then
    echo First arg must be Project: cli,ui
    exit 1
fi

if [ "$2" == "" ]; then
	echo Second arg must be OS: 10.9,10.15
	exit 1
fi

PROJECT=$1
VAROS=$2
CONFIG=Release

VARSTAFF="no"
if test -f "${SCRIPTDIR}/../signing/apple-dev-id.txt"; then # Staff AirVPN
    VARSTAFF="yes"
fi

ARCH=$($SCRIPTDIR/../macos_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../macos_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_disk_temp.dmg
FINALPATH=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_disk.dmg
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_disk.dmg

mkdir -p ${SCRIPTDIR}/../files

if test -f "${DEPLOYPATH}"; then
    echo "Already builded: ${DEPLOYPATH}"
    exit 0;
fi

# Cleanup
rm -rf $TARGETDIR
rm -rf $FINALPATH

# Package dependencies
echo Step: Package dependencies - Build Portable
"${SCRIPTDIR}/../macos_portable/build.sh" ${PROJECT} ${ARCH} ${VAROS}
mkdir -p ${TARGETDIR}
DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_portable.zip
# DMG

# Extract base

tar -jxvf "${SCRIPTDIR}/diskbase.dmg.tar.bz2" -C "${TARGETDIR}"

# Resize

hdiutil resize -size 200m "${TARGETDIR}/diskbase.dmg"

# Attach

hdiutil attach "${TARGETDIR}/diskbase.dmg" -mountpoint "${TARGETDIR}/DiskBuild"

# Fill

unzip "${DEPPACKAGEPATH}" -d "${TARGETDIR}/DiskBuild/"

# Detach

hdiutil detach "${TARGETDIR}/DiskBuild"

# Compress

hdiutil convert "${TARGETDIR}/diskbase.dmg" -format UDCO -imagekey zlib-level=9 -o "${FINALPATH}"

# Hardening - See comment in macos_portable/build.sh

VARHARDENING="yes"
if [ ${VAROS} = "macos-10.9" ]; then
    VARHARDENING="no"
fi

# Sign package

if [ ${VARSTAFF} = "yes" ]; then
    "${SCRIPTDIR}/../macos_common/sign.sh" "${FINALPATH}" yes ${VARHARDENING}
fi

# Notarization - Not need
# if [ ${VARSTAFF} = "yes" ]; then
#     "${SCRIPTDIR}/../macos_common/notarize.sh" "${FINALPATH}" "org.airvpn.eddie.${PROJECT}"
# fi

# Deploy to eddie.website

${SCRIPTDIR}/../macos_common/deploy.sh ${FINALPATH} "internal"

# End

mv ${FINALPATH} ${DEPLOYPATH}

# Cleanup

echo Step: Final cleanup
rm -rf $TARGETDIR

