#!/bin/bash

set -euo pipefail

#rm -f files/*

#linux_mono/build.sh cli # [deprecated in 2.24.0] 
linux_portable/build.sh cli l
linux_appimage/build.sh cli l
linux_debian/build.sh cli l
linux_rpm/build.sh cli l opensuse
linux_rpm/build.sh cli l fedora

#linux_mono/build.sh ui # [deprecated in 2.24.0] 
linux_portable/build.sh ui l
linux_appimage/build.sh ui l
linux_debian/build.sh ui l
linux_rpm/build.sh ui l opensuse
linux_rpm/build.sh ui l fedora

echo "Done."
