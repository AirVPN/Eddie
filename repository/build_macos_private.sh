#!/bin/bash

set -e

#macos_portable/build.sh ui x64 macos-10.9 net4
#macos_pkg/build.sh ui x64 macos-10.9 net4
#macos_dmg/build.sh ui x64 macos-10.9 net4
#macos_portable/build.sh ui x64 macos-10.15 net4
#macos_pkg/build.sh ui x64 macos-10.15 net4
#macos_dmg/build.sh ui x64 macos-10.15 net4

macos_portable/build.sh cli arm64 macos-10.15 net6
#macos_pkg/build.sh ui arm64 macos-10.15 net4
#macos_dmg/build.sh ui arm64 macos-10.15 net4

echo "Done."
