#!/bin/bash

set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}
SCRIPTDIR=$(dirname $(realpath "$0"))

if [ "$1" == "" ]; then
	echo First arg must be Path: yes,no
	exit 1
fi

if [ "$2" == "" ]; then
	echo Second arg must be Force: yes,no
	exit 1
fi

if [ "$3" == "" ]; then
	echo Third arg must be Hardening: yes,no
	exit 1
fi

VARPATH=$1
VARFORCE=$2
VARHARDENING=$3

#VARFORCE="yes"
#VARHARDENING="no"

VARSTAFF="no"
if test -f "${SCRIPTDIR}/../signing/apple-dev-id.txt"; then # Staff AirVPN
    VARSTAFF="yes"
fi

if [ ${VARSTAFF} = "yes" ]; then
    echo Signing, checking: ${VARPATH}    
    DOSIGN=${VARFORCE}

    if [ ${DOSIGN} = "no" ]; then
        set +e
        codesign --verify -v "${VARPATH}"
        if [ $? -eq 0 ]; then            
            # Files with 'Signature=adhoc' pass verify, ensure there is an Authority
            VARAUTHORITY=$(codesign -dv --verbose=4 "${VARPATH}" 2>&1 | grep Authority)
            if [ -z "$VARAUTHORITY" ]; then
                echo "Detected signed without Authority, resign"
                DOSIGN="yes"
            else
                echo "Signing, already: ${VARPATH}"
            fi
        else
            DOSIGN="yes"
        fi
    fi

    if [ ${DOSIGN} = "yes" ]; then
        echo "Signing, need: ${VARPATH}"
        
        export  LDFLAGS="-mmacosx-version-min=10.9"   
        export   CFLAGS="-mmacosx-version-min=10.9"   
        export CXXFLAGS="-mmacosx-version-min=10.9"

        APPLEID=$(cat ${SCRIPTDIR}/../signing/apple-dev-id.txt)

        #codesign -d --deep -v --force --sign "${APPLEID}" "${VARPATH}"

        #codesign -d --deep -v --force --entitlements "${SCRIPTDIR}/Entitlements.plist" --options=runtime --sign "${APPLEID}" "${VARPATH}"

        # See comment in macos_portable/build.sh

        if [ ${VARHARDENING} = "yes" ]; then
            echo test
            echo codesign -d -v --force --entitlements "${SCRIPTDIR}/Entitlements.plist" --options=runtime --sign "${APPLEID}" "${VARPATH}"    
            codesign -d -v --force --entitlements "${SCRIPTDIR}/Entitlements.plist" --options=runtime --sign "${APPLEID}" "${VARPATH}"    
        else
            codesign -d -v --force --sign "${APPLEID}" "${VARPATH}"
        fi
        
        #codesign -d -v --force --sign "${APPLEID}" "${VARPATH}"
        
        codesign --verify -v "${VARPATH}"
    fi
else
    echo "Signature of ${VARPATH} skipped, no keys.";
fi