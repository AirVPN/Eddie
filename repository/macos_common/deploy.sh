#!/bin/bash

set -euo pipefail

#realpath() {
#    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
#}
#SCRIPTDIR=$(dirname $(realpath "$0"))
SCRIPTDIR="$(cd "$(dirname "$0")" && pwd -P)"
FILEPACKAGE=$1

if test -f "${EDDIESIGNINGDIR}/eddie.website_deploy.key"; then # Staff AirVPN

    REMOTEDIR=$2
    if [[ ${REMOTEDIR} == "internal" ]]; then 
        REMOTEDIR=/opt/repository/eddie/internal
    fi

	chmod 600 "${EDDIESIGNINGDIR}/eddie.website_deploy.key"
    echo Send to eddie.website server: ${FILEPACKAGE}
	scp -P 46333 -i "${EDDIESIGNINGDIR}/eddie.website_deploy.key" "${FILEPACKAGE}"  deploy@eddie.website:${REMOTEDIR}
else
    echo "Deploy of ${FILEPACKAGE} skipped, no keys found.";
fi
