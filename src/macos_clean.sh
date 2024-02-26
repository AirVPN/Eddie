#!/bin/bash

set -euo pipefail

# This exists because mixing already-builded net4.8 and net7 cause issues, so we launch this between switch.

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}
SCRIPTDIR=$(dirname $(realpath "$0"))

echo Clean Start

rm -rf "${SCRIPTDIR}/Lib.Core/bin"
rm -rf "${SCRIPTDIR}/Lib.Core/obj"

rm -rf "${SCRIPTDIR}/Lib.Platform.MacOS/bin"
rm -rf "${SCRIPTDIR}/Lib.Platform.MacOS/obj"

rm -rf "${SCRIPTDIR}/App.CLI.MacOS/bin"
rm -rf "${SCRIPTDIR}/App.CLI.MacOS/obj"

# ---------------

rm -rf "${SCRIPTDIR}/App.Cocoa.MacOS/bin"
rm -rf "${SCRIPTDIR}/App.Cocoa.MacOS/obj"

echo Clean Done

