#!/bin/bash

set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

SCRIPTDIR=$(dirname $(realpath "$0"))
FILEPACKAGE=$1

if test -f "${SCRIPTDIR}/../signing/eddie.website_deploy.key"; then # Staff AirVPN

    REMOTEDIR=$2
    if [[ ${REMOTEDIR} == "internal" ]]; then 
        REMOTEDIR=/home/www/repository/eddie/internal
    fi

	chmod 600 "${SCRIPTDIR}/../signing/eddie.website_deploy.key"
    echo Send to eddie.website server: ${FILEPACKAGE}
	scp -P 46333 -i "${SCRIPTDIR}/../signing/eddie.website_deploy.key" "${FILEPACKAGE}"  deploy@eddie.website:${REMOTEDIR}
else
    echo "Deploy of ${FILEPACKAGE} skipped, no keys found.";
fi
