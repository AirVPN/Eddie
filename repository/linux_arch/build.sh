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

MODE=$2

PROJECT=$1
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)
VERSIONSTABLE="2.18.9"

TARGETDIR=/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_arch
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_arch.tar.xz

REPODIR=$(realpath -s $SCRIPTDIR/../../)

# Param 1- Type
# Param 2- Target Path
# Param 3- PKGBUILD template
function arch_env() {
	mkdir -p $3
	cd $3

	cp $4/PKGBUILD PKGBUILD
	cp $4/eddie-ui.install eddie-ui.install

	if [ "$1" = "self" ]; then
		echo "Build local";
		sed -i "s|{@version}|${VERSION}|g" PKGBUILD    
		sed -i "s|{@pkgname}|eddie-ui|g" PKGBUILD    
		sed -i "s|{@pkgdesc}|Eddie - VPN tunnel|g" PKGBUILD    

		sed -i "s|{@source}|git+file:///$2/|g" PKGBUILD    
        sed -i "s|cd \"Eddie-\$pkgver\"|cd \"eddie-air\"|g" PKGBUILD

		updpkgsums
		makepkg -f
		makepkg --printsrcinfo > .SRCINFO
		mv eddie-ui-${VERSION}-1-x86_64.pkg.tar.xz ${DEPLOYPATH}
		# Deploy to eddie.website
		${SCRIPTDIR}/../linux_common/deploy.sh ${DEPLOYPATH}
	else
		if [ "$1" = "git" ]; then
			echo "Update official repo git edition"
			sed -i "s|{@version}|${VERSION}|g" PKGBUILD    
			sed -i "s|{@pkgname}|eddie-ui-git|g" PKGBUILD    
			sed -i "s|{@pkgdesc}|Eddie - VPN tunnel - beta version|g" PKGBUILD    
			sed -i "s|{@source}|git+https://github.com/AirVPN/Eddie.git|g" PKGBUILD    
			sed -i "s|cd \"Eddie-\$pkgver\"|cd \"Eddie\"|g" PKGBUILD
			echo Enter AUR passphrase if requested
			git clone ssh://aur@aur.archlinux.org/eddie-ui-git.git
			cd eddie-ui-git
			cp ../PKGBUILD .
			cp ../eddie-ui.install .
			updpkgsums
			makepkg --printsrcinfo > .SRCINFO			
		elif [ "$1" = "stable" ]; then
			echo "Update official repo release edition ${VERSIONSTABLE}"
			sed -i "s|{@version}|${VERSIONSTABLE}|g" PKGBUILD    
			sed -i "s|{@pkgname}|eddie-ui|g" PKGBUILD    
			sed -i "s|{@pkgdesc}|Eddie - VPN tunnel|g" PKGBUILD    
			sed -i "s|{@source}|https://github.com/AirVPN/Eddie/archive/${VERSIONSTABLE}.tar.gz|g" PKGBUILD    
			echo Enter AUR passphrase if requested
			git clone ssh://aur@aur.archlinux.org/eddie-ui.git
			cd eddie-ui
			cp ../PKGBUILD .
			cp ../eddie-ui.install .
			updpkgsums
			makepkg --printsrcinfo > .SRCINFO			
		fi
		git config user.name "Eddie.website"
		git config user.email "maintainer@eddie.website"
		git add .SRCINFO
		git add eddie-ui.install
		git add PKGBUILD
		git commit -m "${VERSION}"
		git push
	fi   
}



# Cleanup
rm -rf $TARGETDIR

if [ "$MODE" == "" ]; then
	if test -f "${DEPLOYPATH}"; then
		echo "Already builded: ${DEPLOYPATH}"
		exit 0;
	fi

	arch_env self "${REPODIR}" "${TARGETDIR}" "${SCRIPTDIR}"

elif [ "$MODE" == "git" ]; then
	if test -f "${SCRIPTDIR}/../signing/eddie.gpg-signing.passphrase"; then # Staff AirVPN
		arch_env git "${REPODIR}" "${TARGETDIR}" "${SCRIPTDIR}"
	fi
elif [ "$MODE" == "stable" ]; then
	if test -f "${SCRIPTDIR}/../signing/eddie.gpg-signing.passphrase"; then # Staff AirVPN
		arch_env stable "${REPODIR}" "${TARGETDIR}" "${SCRIPTDIR}"
	fi
fi


# Cleanup
echo Step: Final cleanup
rm -rf ${TARGETDIR}
echo Build linux_arch complete

