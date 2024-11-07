#!/bin/bash

set -euo pipefail

if [ "${1-}" == "" ]; then
	echo First arg must be project: cli,ui
	exit 1
fi

if [ "${2-}" == "" ]; then
	echo Second arg must be framework: net4, net7
	exit 1
fi

PROJECT=$1
FRAMEWORK=$2
CONFIG=Release

SCRIPTDIR=$(dirname $(realpath -s $0))
ARCH=$($SCRIPTDIR/../linux_common/get-arch.sh)
RID=$($SCRIPTDIR/../linux_common/get-rid.sh)
VERSION=$($SCRIPTDIR/../linux_common/get-version.sh)

TARGETDIR="/tmp/eddie_deploy/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_portable"
DEPLOYPATH="${SCRIPTDIR}/../files/eddie-${PROJECT}_${VERSION}_linux_${ARCH}_portable.tar.gz"

# Check env

mkdir -p ${SCRIPTDIR}/../files

if test -f "${DEPLOYPATH}"; then
	echo "Already builded: ${DEPLOYPATH}"
	exit 0;
fi

# Clean, ensure build from zero
"${SCRIPTDIR}/../../src/linux_clean.sh"
rm -rf "${TARGETDIR}"

echo Step: Build portable

mkdir -p ${TARGETDIR}

TARGETBINDIR="${TARGETDIR}/eddie-${PROJECT}"

mkdir "${TARGETBINDIR}"

cp "${SCRIPTDIR}/portable.txt" "${TARGETBINDIR}"

if [ $PROJECT = "cli" ]; then

    cd "${SCRIPTDIR}/../../src/App.CLI.Linux/"
    dotnet publish App.CLI.Linux.net7.csproj --configuration Release --runtime ${RID} --self-contained true -p:PublishTrimmed=true -p:EnableCompressionInSingleFile=true
    cp "${SCRIPTDIR}/../../src/App.CLI.Linux/bin/${CONFIG}/net7.0/${RID}/publish"/* "${TARGETBINDIR}"
    cp "${SCRIPTDIR}/../../src/App.CLI.Linux/bin/${CONFIG}/net7.0/${RID}/eddie-cli-elevated" "${TARGETBINDIR}"
    cp "${SCRIPTDIR}/../../src/App.CLI.Linux/bin/${CONFIG}/net7.0/${RID}/eddie-cli-elevated-service" "${TARGETBINDIR}"
    cp "${SCRIPTDIR}/../../src/App.CLI.Linux/bin/${CONFIG}/net7.0/${RID}/libLib.Platform.Linux.Native.so" "${TARGETBINDIR}"

    # Strip commented, at 2023-11-29, corrupt.    
    # Even without "--strip-unneeded".
    # Even with -p:EnableCompressionInSingleFile=false
    # Need investigation, also because require lintian override on Debian, and !strip in PKGBUILD on Arch.
    # Ticket here: https://techcommunity.microsoft.com/t5/tools/linux-strip-corrupt-self-contained-publish-binary/m-p/4005771#M141    

    # strip -S --strip-unneeded "${TARGETBINDIR}/eddie-cli" 

    # Resources
    echo Step: Resources
    cp "${SCRIPTDIR}/../../deploy/linux_${ARCH}"/* "${TARGETBINDIR}"
    cp -r "${SCRIPTDIR}/../../resources" "${TARGETBINDIR}/res"
    if [[ ${VERSION} =~ ^2 ]]; then
        rm -rf "${TARGETBINDIR}/res/webui"
    fi

elif [ $PROJECT = "ui" ]; then 

    echo Step: Dependencies        
    "${SCRIPTDIR}/../linux_portable/build.sh" cli net7
    DEPPACKAGEPATH=${SCRIPTDIR}/../files/eddie-cli_${VERSION}_linux_${ARCH}_portable.tar.gz
    cp "${DEPPACKAGEPATH}" "${TARGETDIR}"
    cd "${TARGETDIR}"
    tar xvf *.tar.gz

    mv -f "${TARGETDIR}/eddie-cli"/* "${TARGETBINDIR}"
    
    if [ $FRAMEWORK = "net7" ]; then
        echo Step: Compile
        "${SCRIPTDIR}/../../src/App.UI.Linux/build.sh" ${CONFIG}    
        cp -r "${SCRIPTDIR}/../../src/App.UI.Linux/bin"/* ${TARGETBINDIR}        
    elif [ $FRAMEWORK = "net4" ]; then        
        echo Step: Compile
        TARGETFRAMEWORK="v4.8";
        RULESETPATH="${SCRIPTDIR}/../../src/ruleset/norules.ruleset"
        SOLUTIONPATH="${SCRIPTDIR}/../../src/App.Forms.Linux//App.Forms.Linux.sln"
        "${SCRIPTDIR}/../../src/linux_clean.sh"
        
        # msbuild is available when monodevelop is installed (reccomended)
        # xbuild is available when mono-complete is installed (deprecated)        
        msbuild /verbosity:minimal /property:CodeAnalysisRuleSet="${RULESETPATH}" /p:Configuration=${CONFIG} /p:Platform=x64 /p:TargetFrameworkVersion=${TARGETFRAMEWORK} /t:Rebuild "${SOLUTIONPATH}" /p:DefineConstants="EDDIEMONO4LINUX"        
        
        # msbuild/Mono under Linux don't honor the postbuild event, called manually
        "${SCRIPTDIR}/../../src/App.Forms.Linux/postbuild.sh" "${SCRIPTDIR}/../../src/App.Forms.Linux/bin/x64/${CONFIG}/" ${ARCH} ${CONFIG}
        
        cp "${SCRIPTDIR}/../../src/App.Forms.Linux.Tray/bin/"eddie-tray ${TARGETBINDIR}/eddie-tray
        cp "${SCRIPTDIR}/../../src/App.Forms.Linux/bin/x64/${CONFIG}"/App.Forms.Linux.exe ${TARGETBINDIR}/eddie-ui.exe
        cp "${SCRIPTDIR}/../../src/App.Forms.Linux/bin/x64/${CONFIG}"/Lib.Core.dll ${TARGETBINDIR}/
        cp "${SCRIPTDIR}/../../src/App.Forms.Linux/bin/x64/${CONFIG}"/Lib.Platform.Linux.dll ${TARGETBINDIR}/
        cp "${SCRIPTDIR}/../../src/App.Forms.Linux/bin/x64/${CONFIG}"/Lib.Forms.dll ${TARGETBINDIR}/
        cp "${SCRIPTDIR}/../../src/App.Forms.Linux/bin/x64/${CONFIG}"/Lib.Forms.Skin.dll ${TARGETBINDIR}/                        
        
        # mkbundle
        echo Step: mkbundle

        if [ $ARCH = "armv7l" ]; then
            MKBUNDLECROSSTARGET="mono-6.0.0-raspbian-9-arm"
        elif [ $ARCH = "aarch64" ]; then
            MKBUNDLECROSSTARGET="mono-6.6.0-debian-10-arm64"
        else
            MKBUNDLECROSSTARGET="mono-6.8.0-debian-10-${ARCH}"
        fi

        # Issue here, check with 'mkbundle --list-targets'
        #mkdir -p /home/pi/.mono/targets/${MKBUNDLECROSSTARGET}/lib/mono # Not sure if need
        if [[ ! -d ${HOME}/.mono/targets/${MKBUNDLECROSSTARGET} ]]; then
            echo Download mkbundle target. If break here, check with 'mkbundle --list-targets' and fix build.sh MKBUNDLECROSSTARGET
            mkbundle --fetch-target ${MKBUNDLECROSSTARGET}

            # Fix issue 'ERROR: Unable to load assembly `Mono.WebBrowser' referenced by'
            #cp /usr/lib/mono/4.5/Mono.WebBrowser.dll ${HOME}/.mono/targets/${MKBUNDLECROSSTARGET}/lib/mono/4.5/

            # Fix issue 'ERROR: Unable to load assembly `Mono.WebBrowser' referenced by'
            #cp /usr/lib/mono/4.5/Mono.Data.Tds.dll ${HOME}/.mono/targets/${MKBUNDLECROSSTARGET}/lib/mono/4.5/
        fi

        # Update config
        # Removed in 2.24.2, now use a dllmap.config
        #cp ${HOME}/.mono/targets/${MKBUNDLECROSSTARGET}/etc/mono/config ${SCRIPTDIR}/mkbundle.config
        #sed -i 's/\$mono_libdir\///g' ${SCRIPTDIR}/mkbundle.config

        # Remember: libMonoPosixHelper.so and libmono-native.so are required for System.IO.File operations
        # Without --config, throw issue about Mono.Unix.Native.Syscall        
        
        cd ${TARGETBINDIR}

        # Using '-L /usr/lib/mono/4.5' cause mscorlib mismatch
        #mkbundle eddie-ui.exe -o eddie-${PROJECT} --cross ${MKBUNDLECROSSTARGET} --i18n all -L /usr/local/lib/mono/4.5 --config ${SCRIPTDIR}/mkbundle.config --library ${SCRIPTDIR}/mkbundle/${ARCH}/libMonoPosixHelper.so --library ${SCRIPTDIR}/mkbundle/${ARCH}/libgdiplus.so.0 --library /usr/lib/libmono-native.so

        # 2023-03-04 notes
        # OpenSUSE / Fedora works, the 3 --library are bundled correctly.
        # Arch: untested
        # Raspbian: untested
        # Debian: throw error, search /usr/lib/../lib/libMonoPosixHelper.so
        # Gentoo: libmono-native.so issue (reported by @overmorrow)
        # - Unresolved.
        # - Solution: mono-runtime-common as dependencies, but 'portable' don't work standalone.  
        mkbundle eddie-ui.exe -o eddie-${PROJECT} --cross ${MKBUNDLECROSSTARGET} --i18n all --simple --no-machine-config --config ${SCRIPTDIR}/mkbundle/dllmap.config --deps --library ${SCRIPTDIR}/mkbundle/${ARCH}/libgdiplus.so.0 --library ${SCRIPTDIR}/mkbundle/${ARCH}/libmono-native.so --library ${SCRIPTDIR}/mkbundle/${ARCH}/libMonoPosixHelper.so

        rm -f ${TARGETBINDIR}/eddie-ui.exe # Linked with mkbundle
        rm -f ${TARGETBINDIR}/App.Forms.Linux.exe # Linked with mkbundle
        rm -f ${TARGETBINDIR}/Lib.Core.dll # Linked with mkbundle
        rm -f ${TARGETBINDIR}/Lib.Forms.dll # Linked with mkbundle
        rm -f ${TARGETBINDIR}/Lib.Forms.Skin.dll # Linked with mkbundle
        rm -f ${TARGETBINDIR}/Lib.Platform.Linux.dll # Linked with mkbundle

        # no, at 2023-11-29, corrupt, see above
        # strip -S --strip-unneeded "${TARGETBINDIR}/eddie-ui" 
    fi
fi

# Cleanup
echo Step: Cleanup
rm -f "${TARGETBINDIR}"/*.profile
rm -f "${TARGETBINDIR}"/*.pdb
rm -f "${TARGETBINDIR}"/*.config
rm -f "${TARGETBINDIR}"/Recovery.xml

if [ $PROJECT = "cli" ]; then
    rm -f "${TARGETBINDIR}"/libayatana*
    rm -f "${TARGETBINDIR}"/libdbus*
fi

# Owner and Permissions
echo Step: Owner and Permissions

chmod -R 755 "$TARGETBINDIR"
find ${TARGETBINDIR} -type f -exec chmod 644 {} +;
chmod 755 "${TARGETBINDIR}/eddie-cli"
if [ $PROJECT = "ui" ]; then
    chmod 755 "${TARGETBINDIR}/eddie-ui"
    chmod 755 "${TARGETBINDIR}/eddie-tray"
fi
chmod 755 "${TARGETBINDIR}/eddie-cli-elevated"
chmod 755 "${TARGETBINDIR}/eddie-cli-elevated-service"
chmod 755 "${TARGETBINDIR}/openvpn"
chmod 755 "${TARGETBINDIR}/hummingbird"
chmod 755 "${TARGETBINDIR}/stunnel"

# Build archive
echo Step: Build archive
cd "$TARGETDIR/"
tar cvfz "$DEPLOYPATH" "eddie-${PROJECT}"

# Deploy to eddie.website
"${SCRIPTDIR}/../linux_common/deploy.sh" "${DEPLOYPATH}"

# Cleanup
echo Step: Final cleanup
rm -rf "${TARGETDIR}"
