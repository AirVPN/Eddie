#!/bin/bash

set -euo pipefail

#rm -f files/*

#linux_mono/build.sh cli # [deprecated in 2.24.0] 
linux_portable/build.sh cli net8
linux_appimage/build.sh cli net8
linux_debian/build.sh cli net8
linux_rpm/build.sh cli net8 opensuse
linux_rpm/build.sh cli net8 fedora

#linux_mono/build.sh ui # [deprecated in 2.24.0] 
linux_portable/build.sh ui net4
linux_appimage/build.sh ui net4
linux_debian/build.sh ui net4
linux_rpm/build.sh ui net4 opensuse
linux_rpm/build.sh ui net4 fedora

echo "Done."
