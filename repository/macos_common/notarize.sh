#!/bin/bash

# First argument must be file package

set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

APPLE_NOTARYTOOL_FILEPATH=$1

SCRIPTDIR=$(dirname $(realpath "$0"))

if test -f "${SCRIPTDIR}/../signing/apple-dev-id.txt"; then 
    
    # Remember: ensure .txt files are UTF8 without BOM
    APPLE_NOTARYTOOL_APPLEID=$(<${SCRIPTDIR}/../signing/apple-notarytool-appleid.txt)
    APPLE_NOTARYTOOL_TEAMID=$(<${SCRIPTDIR}/../signing/apple-notarytool-teamid.txt)
    APPLE_NOTARYTOOL_PASSWORD=$(<${SCRIPTDIR}/../signing/apple-notarytool-password.txt)    

    echo "Notarization..."
    
    echo ----
    echo 1- if fail with 'xcrun: error: unable to find utility "altool", not a developer tool or in PATH', run 'sudo xcode-select -r' to fix.
    echo 2- if fail with 'You must first sign the relevant contracts online. (1048)', goto https://developer.apple.com/account/ , login and accept the new TOS.
    echo ----

    xcrun notarytool submit "${APPLE_NOTARYTOOL_FILEPATH}" --apple-id $APPLE_NOTARYTOOL_APPLEID --team-id $APPLE_NOTARYTOOL_TEAMID --password $APPLE_NOTARYTOOL_PASSWORD --verbose --wait
        
    echo "Notarization stapler"
    # Note: don't work with .zip, ignore, for this "|| true"
    xcrun stapler staple "${APPLE_NOTARYTOOL_FILEPATH}" || true
    xcrun stapler validate "${APPLE_NOTARYTOOL_FILEPATH}" || true
else
    echo "Notarization of ${APPLE_NOTARYTOOL_FILEPATH} skipped, no keys found.";
fi

exit 0



