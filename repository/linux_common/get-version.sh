#!/bin/bash

set -e

CURRENTDIR=$(dirname $(realpath -s $0))

cat $CURRENTDIR/../../src/Lib.Core/Constants.cs | grep -oP "VersionDesc = \"\K[0-9\.]+"

exit 0

