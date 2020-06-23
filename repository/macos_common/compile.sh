#!/bin/bash

set -e

realpath() {
    [[ $1 = /* ]] && echo "$1" || echo "$PWD/${1#./}"
}

# Check args
if [ "$1" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

# Check env
if ! [ -x "$(command -v xbuild)" ]; then
  echo 'Error: tar is not installed.' >&2
  exit 1
fi

# Work
PROJECT=$1
CONFIG="Release"

SCRIPTDIR=$(dirname $(realpath "$0"))
ARCH=$($SCRIPTDIR/../macos_common/get-arch.sh)
TARGETFRAMEWORK="v4.8";
SOLUTIONPATH="";
if [ $PROJECT = "cli" ]; then
    SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie.macos.cli.sln"
elif [ $PROJECT = "ui" ]; then
    SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie2.macos.ui.sln"
elif [ $PROJECT = "ui3" ]; then
    SOLUTIONPATH="${SCRIPTDIR}/../../src/eddie3.macos.ui.sln"
fi
RULESETPATH="${SCRIPTDIR}/../../src/ruleset/norules.ruleset"

ARCHCOMPILE=${ARCH}


# Unlike other platform, the pre-build / post-build script are already in .vcproj, managed by msbuild correctly

#if [ $PROJECT = "cli" ]; then
#    ${SCRIPTDIR}/../../src/eddie.macos.prebuild.sh ${SCRIPTDIR}/../../src/App.CLI.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/ ${PROJECT} ${ARCH} ${CONFIG} 
#elif [ $PROJECT = "ui" ]; then
#	 ${SCRIPTDIR}/../../src/eddie.macos.prebuild.sh ${SCRIPTDIR}/../../src/App.Cocoa.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/ ${PROJECT} ${ARCH} ${CONFIG}   
#elif [ $PROJECT = "ui3" ]; then
#    ${SCRIPTDIR}/../../src/eddie.macos.prebuild.sh ${SCRIPTDIR}/../../src/UI.Cocoa.MacOS/bin/${ARCHCOMPILE}/${CONFIG}/ ${PROJECT} ${ARCH} ${CONFIG}  
#fi

msbuild /verbosity:minimal /property:CodeAnalysisRuleSet="${RULESETPATH}" /p:Configuration=${CONFIG} /p:Platform=${ARCHCOMPILE} /p:TargetFrameworkVersion=${TARGETFRAMEWORK} /t:Rebuild "${SOLUTIONPATH}" /p:DefineConstants="EDDIENET4"

# msbuild /verbosity:detailed /property:CodeAnalysisRuleSet="${RULESETPATH}" /p:Configuration=${CONFIG} /p:Platform=${ARCHCOMPILE} /p:TargetFrameworkVersion=${TARGETFRAMEWORK} /t:Rebuild "${SOLUTIONPATH}" /p:DefineConstants="EDDIENET4"


