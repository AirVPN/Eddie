#!/bin/bash

set -e

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

SCRIPTDIR=$(dirname $(realpath "$0"))

if test -f "${SCRIPTDIR}/signing/eddie.website_deploy.key"; then # Staff AirVPN

    EXE="${SCRIPTDIR}/../src/App.CLI.Linux/bin/x64/Release/App.CLI.Linux.exe"

    mono ${EXE} -cli -help -help.format=html -path.resources='../../../../../resources/' >/tmp/manual.html
    scp -P 46333 -i "${SCRIPTDIR}/signing/eddie.website_deploy.key" "/tmp/manual.html"  deploy@eddie.website:/home/www/repository/eddie/manuals
    
    mono ${EXE} -cli -help -help.format=text -path.resources='../../../../../resources/' >/tmp/manual.txt
    scp -P 46333 -i "${SCRIPTDIR}/signing/eddie.website_deploy.key" "/tmp/manual.txt"  deploy@eddie.website:/home/www/repository/eddie/manuals
    
    mono ${EXE} -cli -help -help.format=man -path.resources='../../../../../resources/' >/tmp/manual.man
    scp -P 46333 -i "${SCRIPTDIR}/signing/eddie.website_deploy.key" "/tmp/manual.man"  deploy@eddie.website:/home/www/repository/eddie/manuals
    
else
    echo "This script is used ONLY by our Staff to generate (from software itself) and upload the latest man to our eddie.website."
fi
