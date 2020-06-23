#!/bin/bash

set -e

ARCHOS=$(uname -m)
if [ $ARCHOS = "i686" ]; then
	echo "x86"
elif [ $ARCHOS = "x86_64" ]; then
	echo "x64"
else
	echo $ARCHOS
fi

exit 0

