#!/bin/bash

set -e

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

SCRIPTDIR=$(dirname $(realpath "$0"))

if test -f "${SCRIPTDIR}/../signing/apple-dev-id.txt"; then # Staff AirVPN
    echo Signing, checking: $1
    APPLEID=$(cat ${SCRIPTDIR}/../signing/apple-dev-id.txt)
    set +e
    codesign --verify -v "$1"
    if [ $? -eq 0 ]; then
        echo "Signing, already: $1"
    else
        echo "Signing, need: $1"
        set -e
        codesign -d --deep -v --force --sign "${APPLEID}" "$1"
        codesign --verify -v "$1"
    fi
fi