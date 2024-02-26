#!/bin/bash

set -euo pipefail

chmod +x src/linux_clean.sh
chmod +x src/Lib.Platform.Linux.Native/build.sh
chmod +x src/App.CLI.Linux.Elevated/build.sh
chmod +x src/App.CLI.Linux/postbuild.sh
#chmod +x src/App.UI.Linux/build.sh
chmod +x src/App.Forms.Linux/postbuild.sh
chmod +x src/App.Forms.Linux.Tray/build.sh
chmod +x repository/build_all_linux.sh
chmod +x repository/linux_common/get-version.sh
chmod +x repository/linux_common/get-rid.sh
chmod +x repository/linux_common/get-arch.sh
chmod +x repository/linux_common/deploy.sh
chmod +x repository/linux_portable/build.sh
chmod +x repository/linux_appimage/build.sh
chmod +x repository/linux_debian/build.sh
chmod +x repository/linux_rpm/build.sh
chmod +x repository/linux_arch/build.sh
chmod +x repository/linux_arch/build_aur_stable.sh
chmod +x repository/linux_arch/build_aur_git.sh
chmod +x repository/linux_arch/build_local.sh

chmod +x src/macos_clean.sh
chmod +x src/Lib.Platform.MacOS.Native/build.sh
chmod +x src/App.CLI.MacOS/postbuild.sh
chmod +x src/App.CLI.MacOS.Elevated/build.sh
#chmod +x src/App.UI.MacOS/build.sh
chmod +x src/App.Cocoa.MacOS/postbuild.sh
chmod +x repository/build_all_macos.sh
chmod +x repository/macos_common/get-version.sh
chmod +x repository/macos_common/presign.sh
chmod +x repository/macos_common/get-rid.sh
chmod +x repository/macos_common/notarize.sh
chmod +x repository/macos_common/get-arch.sh
chmod +x repository/macos_common/deploy.sh
chmod +x repository/macos_common/sign.sh
chmod +x repository/macos_portable/build.sh
chmod +x repository/macos_pkg/build.sh
chmod +x repository/macos_dmg/build.sh

chmod +x repository/man_upload.sh

echo Done