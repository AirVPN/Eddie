#!/bin/bash

set -euo pipefail

# Check args
if [ "${1-}" == "" ]; then
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
VERSIONSTABLE=$(curl --silent "https://api.github.com/repos/AirVPN/Eddie/releases/latest" | grep -Po '"tag_name": "\K.*?(?=")')

TARGETDIR=$HOME/Documents/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_arch
DEPLOYPATH=${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_arch.tar.zst

REPODIR=$(realpath -s $SCRIPTDIR/../../)

# Param 1- Type
# Param 2- Target Path
# Param 3- PKGBUILD template
function arch_env() {
	mkdir -p $3
	cd $3

	cp $4/PKGBUILD PKGBUILD
	cp $4/eddie-${PROJECT}.install eddie-${PROJECT}.install

	if [ "$1" = "self" ]; then
		echo "Build local";
		sed -i "s|{@project}|${PROJECT}|g" PKGBUILD    
		sed -i "s|{@version}|${VERSION}|g" PKGBUILD
		sed -i "s|{@pkgname}|eddie-${PROJECT}|g" PKGBUILD    
		sed -i "s|{@pkgdesc}|Eddie - VPN tunnel|g" PKGBUILD    
		if [ "${PROJECT}" = "cli" ]; then
			sed -i "s|{@pkgdesc}|Eddie - VPN tunnel - CLI - prebuilt|g" PKGBUILD    
			sed -i "s|{@pkgdepends}|(curl openvpn sudo)|g" PKGBUILD
			sed -i "s|{@pkgmakedepends}|(cmake dotnet-sdk)|g" PKGBUILD
		elif [ "${PROJECT}" = "ui" ]; then
			sed -i "s|{@pkgdesc}|Eddie - VPN tunnel - UI - prebuilt|g" PKGBUILD    
			sed -i "s|{@pkgdepends}|(curl openvpn sudo polkit desktop-file-utils libnotify libayatana-appindicator patchelf)|g" PKGBUILD
			sed -i "s|{@pkgmakedepends}|(cmake dotnet-sdk mono-msbuild mono)|g" PKGBUILD
		fi
		sed -i "s|{@source}|git+file:///$2/|g" PKGBUILD    
		sed -i "s|cd \"Eddie-\$pkgver\"|cd \"eddie-air\"|g" PKGBUILD

		updpkgsums
		makepkg -f
		makepkg --printsrcinfo > .SRCINFO
		echo $PWD
		mv eddie-${PROJECT}-${VERSION}-1-x86_64.pkg.tar.zst ${DEPLOYPATH}
		# Deploy to eddie.website
		${SCRIPTDIR}/../linux_common/deploy.sh ${DEPLOYPATH}
	else
		if [ "$1" = "git" ]; then
			echo "Update official repo git edition"
			sed -i "s|{@project}|${PROJECT}|g" PKGBUILD    
			sed -i "s|{@version}|${VERSION}|g" PKGBUILD    
			sed -i "s|{@pkgname}|eddie-${PROJECT}-git|g" PKGBUILD    
			if [ "${PROJECT}" = "cli" ]; then
				sed -i "s|{@pkgdesc}|Eddie - VPN tunnel - CLI|g" PKGBUILD    
				sed -i "s|{@pkgdepends}|(curl openvpn sudo)|g" PKGBUILD
				sed -i "s|{@pkgmakedepends}|(cmake dotnet-sdk)|g" PKGBUILD
			else
				sed -i "s|{@pkgdesc}|Eddie - VPN tunnel - UI|g" PKGBUILD    
				sed -i "s|{@pkgdepends}|(mono curl openvpn sudo polkit desktop-file-utils libnotify libayatana-appindicator patchelf)|g" PKGBUILD
				sed -i "s|{@pkgmakedepends}|(cmake dotnet-sdk mono-msbuild mono)|g" PKGBUILD
			fi
			sed -i "s|{@source}|git+https://github.com/AirVPN/Eddie.git|g" PKGBUILD    
			sed -i "s|cd \"Eddie-\$pkgver\"|cd \"Eddie\"|g" PKGBUILD
			if test -f "${SCRIPTDIR}/../signing/aur.key.password.txt"; then # Staff AirVPN
    			echo if requested, enter $(cat "${SCRIPTDIR}/../signing/aur.key.password.txt") as passphrase
			fi
			git -c core.sshCommand="ssh -i ${SCRIPTDIR}/../signing/aur.key" clone ssh://aur@aur.archlinux.org/eddie-${PROJECT}-git.git
			cd eddie-${PROJECT}-git
			cp ../PKGBUILD .
			cp ../eddie-${PROJECT}.install .
			updpkgsums
			makepkg --printsrcinfo > .SRCINFO			
		elif [ "$1" = "stable" ]; then
			echo "Update official repo release edition ${VERSIONSTABLE}"
			sed -i "s|{@project}|${PROJECT}|g" PKGBUILD    
			sed -i "s|{@version}|${VERSIONSTABLE}|g" PKGBUILD    
			sed -i "s|{@pkgname}|eddie-${PROJECT}|g" PKGBUILD    
			if [ "${PROJECT}" = "cli" ]; then
				sed -i "s|{@pkgdesc}|Eddie - VPN tunnel - CLI|g" PKGBUILD    
				sed -i "s|{@pkgdepends}|(curl openvpn sudo)|g" PKGBUILD
				sed -i "s|{@pkgmakedepends}|(cmake dotnet-sdk)|g" PKGBUILD
			else
				sed -i "s|{@pkgdesc}|Eddie - VPN tunnel - UI|g" PKGBUILD
				sed -i "s|{@pkgdepends}|(mono curl openvpn sudo polkit desktop-file-utils libnotify libayatana-appindicator patchelf)|g" PKGBUILD
				sed -i "s|{@pkgmakedepends}|(cmake dotnet-sdk mono-msbuild mono)|g" PKGBUILD
			fi
			sed -i "s|{@source}|https://github.com/AirVPN/Eddie/archive/${VERSIONSTABLE}.tar.gz|g" PKGBUILD    
			if test -f "${SCRIPTDIR}/../signing/aur.key.password.txt"; then # Staff AirVPN
    			echo if requested, enter $(cat "${SCRIPTDIR}/../signing/aur.key.password.txt") as passphrase
			fi
			git -c core.sshCommand="ssh -i ${SCRIPTDIR}/../signing/aur.key" clone ssh://aur@aur.archlinux.org/eddie-${PROJECT}.git
			cd eddie-${PROJECT}
			cp ../PKGBUILD .
			cp ../eddie-${PROJECT}.install .
			updpkgsums
			makepkg --printsrcinfo > .SRCINFO			
		fi
		git config user.name "Eddie.website"
		git config user.email "maintainer@eddie.website"
		git add .SRCINFO
		git add eddie-${PROJECT}.install
		git add PKGBUILD
		git commit -m "${VERSION}"
		git -c core.sshCommand="ssh -i ${SCRIPTDIR}/../signing/aur.key" push
	fi   
}

mkdir -p ${SCRIPTDIR}/../files

# Cleanup
rm -rf $TARGETDIR

if [ "$MODE" == "local" ]; then
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

