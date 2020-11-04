#!/bin/bash

set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

SCRIPTDIR=$(dirname $(realpath "$0"))

${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_x64/openvpn" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_x64/libcrypto.1.1.dylib" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_x64/liblzo2.2.dylib" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_x64/libpkcs11-helper.1.dylib" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_x64/libssl.1.1.dylib" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_x64/hummingbird" no yes
${SCRIPTDIR}/sign.sh "${SCRIPTDIR}/../../deploy/macos_x64/stunnel" no yes

echo "Pre-sign done."
