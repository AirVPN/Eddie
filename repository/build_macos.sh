#!/bin/bash

set -e


# Currently 2021-02-03 "VS for Mac" works under Rosetta, and for example App.CLI.MacOS.Elevated will be build (because called from VS prebuild) in x86_64 even on Apple M1 (arm64).
# This avoid the issue. Can be cleaned when "VS for Mac" will be released for arm64.
# The below file it's checked in /src/App.CLI.MacOS.Elevated/build.sh
# The below file it's checked in /src/eddie.macos.prebuild.sh to allow pick the right deploy folder for whitelist hashing
uname -m >/tmp/eddie_deploy_arch_native.txt

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

# Need arch diff
VARARCH=$(cat /tmp/eddie_deploy_arch_native.txt)
if [ ${VARARCH} = "x86_64" ]; then
    macos_portable/build.sh ui macos-10.9
    macos_pkg/build.sh ui macos-10.9
    macos_dmg/build.sh ui macos-10.9
fi

macos_portable/build.sh ui macos-10.15
macos_pkg/build.sh ui macos-10.15
macos_dmg/build.sh ui macos-10.15

echo "Done."
