#!/bin/bash

set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

CURRENTDIR=$(dirname $(realpath "$0"))

cat ${CURRENTDIR}/../../src/Lib.Core/Constants.cs | grep "VersionDesc = \"" | awk -F"\"" '{print $2}'

exit 0

