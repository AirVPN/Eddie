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

REPODIR=$(realpath -s $SCRIPTDIR/../../)

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
	mkdir -p $3
	cd $3

	cp $4/PKGBUILD PKGBUILD
	cp $4/eddie-ui.install eddie-ui.install

	sed -i "s|{@version}|${VERSION}|g" PKGBUILD    

	if [ "$1" = "self" ]; then
		echo "Build local";
		sed -i "s|{@pkgname}|eddie-ui|g" PKGBUILD    
		sed -i "s|{@source}|git+file:///$2/|g" PKGBUILD    
		updpkgsums
		makepkg -f
		makepkg --printsrcinfo > .SRCINFO
		echo mv eddie-ui-${VERSION}-1-x86_64.pkg.tar.xz eddie-ui_${VERSION}_linux_x64_arch.tar.xz        
		mv eddie-ui-${VERSION}-1-x86_64.pkg.tar.xz ${DEPLOYPATH}
		# Deploy to eddie.website
		${SCRIPTDIR}/../linux_common/deploy.sh ${DEPLOYPATH}
	else
		if [ "$1" = "git" ]; then
			echo "Update official repo git edition"
			sed -i "s|{@pkgname}|eddie-ui-git|g" PKGBUILD    
			sed -i "s|{@source}|git+https://github.com/AirVPN/Eddie.git|g" PKGBUILD    
			git clone https://aur.archlinux.org/eddie-ui-git.git
		elif [ "$1" = "stable" ]; then
			echo "Update official repo release edition"
			sed -i "s|{@pkgname}|eddie-ui|g" PKGBUILD    
			sed -i "s|{@source}|https://github.com/AirVPN/Eddie/archive/v${VERSION}.tar.gz|g" PKGBUILD    
			git clone https://aur.archlinux.org/eddie-ui.git
		fi
		git config user.name "Eddie.website"
		git config user.email "maintainer@eddie.website"
	fi   
}

arch_env self "${REPODIR}" "${TARGETDIR}" "${SCRIPTDIR}"

# Update official repo
if test -f "${SCRIPTDIR}/../signing/gpg.passphrase.pazzo"; then # Staff AirVPN
	arch_env git "${SCRIPTDIR}/../files/repo-arch/" "${SCRIPTDIR}"
	#arch_env release
fi

# Cleanup
echo Step: Final cleanup
#pazzo rm -rf ${TARGETDIR}
echo Build linux_arch complete


