#!/bin/bash

set -euo pipefail

#realpath() {
#    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
#}
#SCRIPTDIR=$(dirname $(realpath "$0"))
SCRIPTDIR="$(cd "$(dirname "$0")" && pwd -P)"

# Check args

if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

if [ "$2" == "" ]; then
	echo Second arg must be Arch: x64, arm64
	exit 1
fi

if [ "$3" == "" ]; then
	echo Third arg must be OS: 10.9,10.15
	exit 1
fi

if [ "$4" == "" ]; then
	echo Fourth arg must be line: l, u
	exit 1
fi

PROJECT=$1
ARCH=$2
VAROS=$3
LINE=$4
CONFIG=Release
VERSION=$($SCRIPTDIR/../macos_common/get-version.sh)
STAFF="no"
if test -f "${EDDIESIGNINGDIR}/apple-dev-id.txt"; then # Staff AirVPN
    STAFF="yes"
fi

ARCHOS=$($SCRIPTDIR/../macos_common/get-arch.sh)
if [ ${ARCH} != ${ARCHOS} ]; then
    echo "DMG build for '${ARCH}' arch build skipped on this OS, cross-compiling not supported."
    exit 0;
fi

if [ "${LINE}" != "l" ] && [ "${LINE}" != "u" ]; then
    echo Fourth arg must be line: l, u
    exit 1
fi

PACKAGEPROJECT="${PROJECT}"
if [ "${LINE}" = "u" ]; then
    PACKAGEPROJECT="${PROJECT}-u"
fi

TARGETDIR=/tmp/eddie_deploy/eddie-${PACKAGEPROJECT}_${VERSION}_${VAROS}_${ARCH}_disk_temp.dmg
FINALPATH=/tmp/eddie_deploy/eddie-${PACKAGEPROJECT}_${VERSION}_${VAROS}_${ARCH}_disk.dmg
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PACKAGEPROJECT}_${VERSION}_${VAROS}_${ARCH}_disk.dmg

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
"${SCRIPTDIR}/../macos_portable/build.sh" ${PROJECT} ${ARCH} ${VAROS} ${LINE}
mkdir -p ${TARGETDIR}
DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-${PACKAGEPROJECT}_${VERSION}_${VAROS}_${ARCH}_portable.zip

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

# Staff Deploy
if test -n "${EDDIESIGNINGDIR:-}"; then
    # Hardening - See comment in macos_portable/build.sh
    VARHARDENING="yes"
    if [ ${VAROS} = "macos-10.9" ]; then
        VARHARDENING="no"
    fi

    # Signing
    "${SCRIPTDIR}/../macos_common/sign.sh" "${FINALPATH}" yes ${VARHARDENING}

    # Notarization
    "${SCRIPTDIR}/../macos_common/notarize.sh" "${FINALPATH}"

    # Deploy
    "${SCRIPTDIR}/../macos_common/deploy.sh" "${FINALPATH}" "internal"

    # PGP
    "${SCRIPTDIR}/../macos_common/sign-openpgp.sh" "${FINALPATH}"
    test -f "${FINALPATH}.asc" && "${SCRIPTDIR}/../macos_common/deploy.sh" "${FINALPATH}.asc" "internal"
fi

# End
mv ${FINALPATH} ${DEPLOYPATH}
test -f "${FINALPATH}.asc" && mv "${FINALPATH}.asc" "${DEPLOYPATH}.asc"

# Cleanup
echo Step: Final cleanup
rm -rf $TARGETDIR

