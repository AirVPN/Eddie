#!/bin/bash

# First argument must be file package
# Second argument must be project (cli/ui)

# set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

FILENOTARIZE=$1
PROJECT=$2

SCRIPTDIR=$(dirname $(realpath "$0"))

# Staff AirVPN only

if test -f "${SCRIPTDIR}/../signing/apple-dev-id.txt"; then 
    
    APPLE_NOTARIZE_USERNAME=$(cat ${SCRIPTDIR}/../signing/apple-notarize-username.txt)
    APPLE_NOTARIZE_PASSWORD=$(cat ${SCRIPTDIR}/../signing/apple-notarize-password.txt)
    
    APPLE_NOTARIZE_BUNDLE_ID="org.airvpn.eddie.${PROJECT}"
    APPLE_NOTARIZE_ASC_PROVIDER=$(cat ${SCRIPTDIR}/../signing/apple-notarize-asc-provider.txt)

    echo "Notarization Upload..."

    echo xcrun altool --notarize-app --primary-bundle-id "$APPLE_NOTARIZE_BUNDLE_ID" --username "$APPLE_NOTARIZE_USERNAME" --password "$APPLE_NOTARIZE_PASSWORD" --asc-provider "$APPLE_NOTARIZE_ASC_PROVIDER" --file "${FILENOTARIZE}"

    echo ----
    echo 1- if fail with 'xcrun: error: unable to find utility "altool", not a developer tool or in PATH', run 'sudo xcode-select -r' to fix.
    echo 2- if fail with 'You must first sign the relevant contracts online. (1048)', goto https://developer.apple.com/account/ , login and accept the new TOS.
    echo ----

    requestUUID=$(xcrun altool --notarize-app --primary-bundle-id "$APPLE_NOTARIZE_BUNDLE_ID" --username "$APPLE_NOTARIZE_USERNAME" --password "$APPLE_NOTARIZE_PASSWORD" --asc-provider "$APPLE_NOTARIZE_ASC_PROVIDER" --file "${FILENOTARIZE}" 2>&1 | awk '/RequestUUID/ { print $NF; }')
                               
    echo "Notarization RequestUUID: $requestUUID"
    
    if [[ $requestUUID == "" ]]; then 
        echo "could not upload for notarization"
        exit 1
    fi
    
    requestStatus="in progress"
    while [[ "$requestStatus" == "in progress" ]]; do
        echo "Notarization Waiting response (10s) ..."
        sleep 10
        requestStatus=$(xcrun altool --notarization-info "$requestUUID" \
                              --username "$APPLE_NOTARIZE_USERNAME" \
                              --password "$APPLE_NOTARIZE_PASSWORD" 2>&1 \
                 | awk -F ': ' '/Status:/ { print $2; }' )
        echo "Notarization Response: $requestStatus"
    done
    
    echo "Notarization final result:"
    xcrun altool --notarization-info "$requestUUID" \
                 --username "$APPLE_NOTARIZE_USERNAME" \
                 --password "$APPLE_NOTARIZE_PASSWORD"
    
    if [[ $requestStatus != "success" ]]; then
        echo "Notarization failed."
        exit 1
    fi

    echo "Notarization stapler"
    xcrun stapler staple $1
    xcrun stapler validate $1

    exit 0
fi

exit 1



