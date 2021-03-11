#!/bin/bash

set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

SCRIPTDIR=$(dirname $(realpath "$0"))
ARCH=$($SCRIPTDIR/../macos_common/get-arch.sh)

${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_${ARCH}/openvpn" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_${ARCH}/libcrypto.1.1.dylib" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_${ARCH}/liblzo2.2.dylib" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_${ARCH}/libpkcs11-helper.1.dylib" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_${ARCH}/libssl.1.1.dylib" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_${ARCH}/hummingbird" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_${ARCH}/stunnel" no yes

echo "Pre-sign done."
