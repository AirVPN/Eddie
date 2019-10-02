#!/bin/bash

set -e

rm -f files/*

# Note: first, ensure deploy files signature. Normally are done by building script, this is an exception.
# Otherwise, openvpn will be signed after the compilation of Elevated, that will contain a mismatch sha256.
macos_common/sign.sh ../deploy/macos_x64/openvpn

macos_mono/build.sh cli
# macos_portable/build.sh cli
# macos_pkg/build.sh ui
# macos_dmg/build.sh ui

# macos_mono/build.sh ui
macos_portable/build.sh ui
macos_pkg/build.sh ui
macos_dmg/build.sh ui

echo "Done."
