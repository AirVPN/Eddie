#!/bin/bash

set -euo pipefail

# Check args
if [ "${1-}" == "" ]; then
	echo First arg must be Project: cli,ui
	exit 1
fi

./build.sh $1 git
