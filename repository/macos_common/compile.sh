#!/bin/bash

set -euo pipefail

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

# Check args
if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

if [ "$2" == "" ]; then
	echo Second arg must be Framework: net4,net6
	exit 1
fi

# Check env
if ! [ -x "$(command -v tar)" ]; then
  echo 'Error: tar is not installed.' >&2
  exit 1
fi

# Work
PROJECT=$1
FRAMEWORK=$2
CONFIG="Release"

SCRIPTDIR=$(dirname $(realpath "$0"))
ARCH=$($SCRIPTDIR/../macos_common/get-arch.sh)

if [ ${FRAMEWORK} = "net4" ]; then
    TARGETFRAMEWORK="v4.8";
    SOLUTIONPATH="";
    if [ ${PROJECT} = "cli" ]; then
        SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie.macos.cli.sln"
    elif [ ${PROJECT} = "ui" ]; then
        SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie2.macos.ui.sln"
    elif [ ${PROJECT} = "ui3" ]; then
        SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie3.macos.ui.sln"
    fi
    RULESETPATH="${SCRIPTDIR}/../../src/ruleset/norules.ruleset"

    ARCHCOMPILE=${ARCH}
    ARCHCOMPILE=x64


    # Unlike other platform, the pre-build / post-build script are already in .vcproj, managed by msbuild correctly

    #if [ $PROJECT = "cli" ]; then
    #    ${SCRIPTDIR}/../../src/eddie.macos.prebuild.sh ${SCRIPTDIR}/../../src/App.CLI.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/ ${PROJECT} ${ARCH} ${CONFIG} 
    #elif [ $PROJECT = "ui" ]; then
    #	 ${SCRIPTDIR}/../../src/eddie.macos.prebuild.sh ${SCRIPTDIR}/../../src/App.Cocoa.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/ ${PROJECT} ${ARCH} ${CONFIG}   
    #elif [ $PROJECT = "ui3" ]; then
    #    ${SCRIPTDIR}/../../src/eddie.macos.prebuild.sh ${SCRIPTDIR}/../../src/UI.Cocoa.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/ ${PROJECT} ${ARCH} ${CONFIG}  
    #fi

    /Library/Frameworks/Mono.framework/Versions/Current/Commands/msbuild /verbosity:minimal /property:CodeAnalysisRuleSet="${RULESETPATH}" /p:Configuration=${CONFIG} /p:Platform=${ARCHCOMPILE} /p:TargetFrameworkVersion=${TARGETFRAMEWORK} /t:Rebuild "${SOLUTIONPATH}" /p:DefineConstants="EDDIENET4"
elif [ ${FRAMEWORK} = "net6" ]; then
    SOLUTIONPATH="";
    if [ $PROJECT = "cli" ]; then
        SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie.macos.cli.net6.sln"
    elif [ $PROJECT = "ui" ]; then
        SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie2.macos.ui.net6.sln"
    elif [ $PROJECT = "ui3" ]; then
        SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie3.macos.ui.net6.sln"
    fi

    # tested on M1 for now
    dotnet publish ${SOLUTIONPATH} --configuration ${CONFIG} --runtime osx.11.0-arm64 --self-contained true -p:DefineConstants="EDDIENET6"
fi


