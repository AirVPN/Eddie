#!/bin/bash

set -e

# This exists because mixing already-builded net4.8 and net7 cause issues, so we launch this between switch.
# For example errors like https://stackoverflow.com/questions/67072200/how-to-fix-add-a-reference-to-netframework-version-v4-7-1-in-the-targetfram
# when switching between .Net4.8 and .Net7

SCRIPTDIR=$(dirname $(realpath -s $0))

echo Clean - Start

rm -rf "${SCRIPTDIR}/Lib.Core/bin"
rm -rf "${SCRIPTDIR}/Lib.Core/obj"

rm -rf "${SCRIPTDIR}/Lib.Platform.Linux/bin"
rm -rf "${SCRIPTDIR}/Lib.Platform.Linux/obj"

rm -rf "${SCRIPTDIR}/App.CLI.Linux/bin"
rm -rf "${SCRIPTDIR}/App.CLI.Linux/obj"

# ------------

rm -rf "${SCRIPTDIR}/Lib.Forms/bin"
rm -rf "${SCRIPTDIR}/Lib.Forms/obj"

rm -rf "${SCRIPTDIR}/Lib.Forms.Skin/bin"
rm -rf "${SCRIPTDIR}/Lib.Forms.Skin/obj"

rm -rf "${SCRIPTDIR}/App.Forms.Linux/bin"
rm -rf "${SCRIPTDIR}/App.Forms.Linux/obj"

echo Clean - Done
