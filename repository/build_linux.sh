#!/bin/bash

set -euo pipefail

#rm -f files/*

linux_mono/build.sh cli
linux_portable/build.sh cli
linux_appimage/build.sh cli
linux_debian/build.sh cli
linux_opensuse/build.sh cli
linux_fedora/build.sh cli

linux_mono/build.sh ui
linux_portable/build.sh ui
linux_appimage/build.sh ui
linux_debian/build.sh ui
linux_opensuse/build.sh ui
linux_fedora/build.sh ui

echo "Done."
