#!/bin/bash

set -e

# Check args
if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

# Check env
if ! [ -x "$(command -v makepkg)" ]; then
  echo 'Error: makepkg is not installed.' >&2
  exit 1
fi

PROJECT=$1
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_arch
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_arch.tar.xz

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Cleanup
rm -rf $TARGETDIR

# Param 1- Type
# Param 2- Target Path
# Param 3- PKGBUILD template
function arch_env() {
    mkdir -p $2
    cd $2
    echo $2
    if [ "$1" = "self" ]; then
        echo "Build local";
    else
        if [ "$1" = "git" ]; then
            echo "Update official repo git edition"
            git clone https://aur.archlinux.org/eddie-ui-git.git
        elif [ "$1" = "release" ]; then
            echo "Update official repo release edition"
            git clone https://aur.archlinux.org/eddie-ui.git
        fi
        git config user.name "Eddie.website"
        git config user.email "maintainer@eddie.website"
    fi
    
    #cp $3 PKGBUILD
    #updpkgsums
    #makepkg -f
    #makepkg --printsrcinfo > .SRCINFO

    #rm -f *.deb
    #rm -f *.tar.xz
    #rm -rf src
    #rm -rf pkg

    #mv *.tar.xz output.tar.xz
    #cp eddie-ui-git-2.18.4-1-x86_64.pkg.tar.xz eddie-ui_2.18.4_linux_x64_arch.tar.xz
    #../linux_common_deploy.sh eddie-ui_2.18.4_linux_x64_arch.tar.xz
}

arch_env self "${TARGETDIR}" "${SCRIPTDIR}/PKGBUILD"

# Deploy to eddie.website
${SCRIPTDIR}/../linux_common/deploy.sh ${DEPLOYPATH}

# Update official repo
if test -f "${SCRIPTDIR}/../signing/gpg.passphrase"; then # Staff AirVPN

    arch_env git "${SCRIPTDIR}/../files/repo-arch/" "${SCRIPTDIR}/PKGBUILD"
    #arch_env release
fi

# Cleanup
echo Step: Final cleanup
rm -rf ${TARGETDIR}
echo Build linux_arch complete


