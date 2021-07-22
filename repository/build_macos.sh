#!/bin/bash

set -e

# rm -f files/*

# Note: first, ensure deploy files signature. Normally are done by building script, this is an exception.
# Otherwise, openvpn will be signed after the compilation of Elevated, that will contain a mismatch sha256.
macos_common/presign.sh

# CLI edition exists, but Xamarin build an .app bundle and
# i don't yet discover how to write a CLI app in .app bundle (avoid doubleclick useless, etc).
# Right now, UI can create a shortcut (Preferences -> UI),
# and it's rare that someone need a CLI-only app (a macOS without desktop environment?)
# macos_portable/build.sh cli
# macos_pkg/build.sh cli # issues
# macos_dmg/build.sh cli

# macos_mono/build.sh ui # not used

# Differences between 10.9 and 10.15 are ONLY related to this issue: https://forums.xamarin.com/discussion/183073/notarization-issue/p1?new=1
# 10.9 don't have --options=runtime or entitlements for this.
# From our tests, 2020-06-23:
# Catalina works with 10.15, don't work with 10.9 for missing notarization
# Mojave works witn 10.9, don't work with 10.15 (throw a SystemNetworkInformation exception, due libc.dylib link issue)
# High Sierra works with 10.15 AND 10.9.

ARCH=$(uname -m)

if [ ${ARCH} = "x86_64" ]; then
    macos_portable/build.sh ui macos-10.9
    macos_pkg/build.sh ui macos-10.9
    macos_dmg/build.sh ui macos-10.9
fi

macos_portable/build.sh ui macos-10.15
macos_pkg/build.sh ui macos-10.15
macos_dmg/build.sh ui macos-10.15

echo "Done."
