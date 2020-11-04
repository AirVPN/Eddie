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
	echo Second arg must be Architecture: x64
	exit 1
fi

if [ "$3" == "" ]; then
	echo Third arg must be OS: 10.9,10.15
	exit 1
fi

PROJECT=$1
ARCH=$2
VAROS=$3
CONFIG=Release

VARSTAFF="no"
if test -f "${SCRIPTDIR}/../signing/apple-dev-id.txt"; then # Staff AirVPN
    VARSTAFF="yes"
fi

# ARCH=$($SCRIPTDIR/../macos_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../macos_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_installer_temp.pkg
FINALPATH=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_installer.pkg
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_installer.pkg

mkdir -p ${SCRIPTDIR}/../files

if test -f "${DEPLOYPATH}"; then
    echo "Already builded: ${DEPLOYPATH}"
    exit 0;
fi

# Cleanup
rm -rf "$TARGETDIR"

# Package dependencies
echo Step: Package dependencies - Build Portable
"${SCRIPTDIR}/../macos_portable/build.sh" ${PROJECT} ${ARCH} ${VAROS}
mkdir -p "${TARGETDIR}"
DEPPACKAGEPATH="${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_${VAROS}_${ARCH}_portable.zip"

mkdir -p "${TARGETDIR}"
#tar -jxvf "${DEPPACKAGEPATH}" -C "${TARGETDIR}/"
unzip "${DEPPACKAGEPATH}" -d "${TARGETDIR}/"

if [ ${VARSTAFF} = "yes" ]; then
    echo Step: Build with signature
    APPLEID=$(cat ${SCRIPTDIR}/../signing/apple-dev-id.txt)
    pkgbuild --component-plist ${SCRIPTDIR}/eddie-pkg.plist --identifier org.airvpn.eddie.${PROJECT} --version ${VERSION} --install-location /Applications --root "${TARGETDIR}" --sign "${APPLEID}" --timestamp "${FINALPATH}"
else
    echo Step: Build
    pkgbuild --component-plist ${SCRIPTDIR}/eddie-pkg.plist --identifier org.airvpn.eddie.${PROJECT} --version ${VERSION} --install-location /Applications --root "${TARGETDIR}" "${FINALPATH}"
fi

# Hardening - See comment in macos_portable/build.sh

VARHARDENING="yes"
if [ ${VAROS} = "macos-10.9" ]; then
    VARHARDENING="no"
fi

# Sign package

if [ ${VARSTAFF} = "yes" ]; then
    "${SCRIPTDIR}/../macos_common/sign.sh" "${FINALPATH}" yes ${VARHARDENING}
fi

# Notarization

if [ ${VARSTAFF} = "yes" ]; then
    if [ ${VARHARDENING} = "yes" ]; then    
        "${SCRIPTDIR}/../macos_common/notarize.sh" "${FINALPATH}" "${PROJECT}"
    fi
fi

# End

mv ${FINALPATH} ${DEPLOYPATH}

# Deploy to eddie.website

"${SCRIPTDIR}/../macos_common/deploy.sh" "${DEPLOYPATH}"

# Cleanup

echo Step: Final cleanup
rm -rf "$TARGETDIR"

