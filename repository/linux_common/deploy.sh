#!/bin/bash

set -e

SCRIPTDIR=$(dirname $(realpath -s $0))

if test -f "${SCRIPTDIR}/../signing/eddie.website_deploy.key"; then # Staff AirVPN
	chmod 600 "${SCRIPTDIR}/../signing/eddie.website_deploy.key"
    echo Send to eddie.website server: $1
	scp -P 46333 -i "${SCRIPTDIR}/../signing/eddie.website_deploy.key" "$1"  deploy@eddie.website:/home/www/repository/eddie/internal
fi
