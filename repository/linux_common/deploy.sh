#!/bin/bash

set -euo pipefail

SCRIPTDIR=$(dirname $(realpath -s $0))

if test -f "${EDDIESIGNINGDIR}/eddie.website_deploy.key"; then # Staff AirVPN
	chmod 600 "${EDDIESIGNINGDIR}/eddie.website_deploy.key"
    echo scp -P 46333 -i "${EDDIESIGNINGDIR}/eddie.website_deploy.key" "$1"  deploy@eddie.website:/opt/repository/eddie/internal
    echo Send to eddie.website server: $1
	scp -P 46333 -i "${EDDIESIGNINGDIR}/eddie.website_deploy.key" "$1"  deploy@eddie.website:/opt/repository/eddie/internal
fi
