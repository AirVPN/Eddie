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

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_macos_${ARCH}_installer.pkg
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_macos_${ARCH}_installer.pkg

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

# PKG
mkdir -p "${TARGETDIR}/Applications"
tar -jxvf "${DEPPACKAGEPATH}" -C "${TARGETDIR}/Applications/"

if test -f "${SCRIPTDIR}/../signing/apple-dev-id.txt"; then # Staff AirVPN
    APPLEID=$(cat ${SCRIPTDIR}/../signing/apple-dev-id.txt)
    pkgbuild --identifier org.airvpn.eddie.ui --version ${VERSION} --root "${TARGETDIR}" --sign "${APPLEID}" --timestamp "${DEPLOYPATH}"
else
    pkgbuild --identifier org.airvpn.eddie.ui --version ${VERSION} --root "${TARGETDIR}" "${DEPLOYPATH}"
fi



# Deploy to eddie.website
${SCRIPTDIR}/../macos_common/deploy.sh ${DEPLOYPATH}

# Cleanup - with sudo because AppImage create files as root
rm -rf $TARGETDIR

