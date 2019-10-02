#!/bin/bash

set -e

CURRENTDIR=$(dirname $(realpath -s $0))

cat $CURRENTDIR/../../src/Lib.Common/Constants.cs | grep -oP "VersionDesc = \"\K[0-9\.]+"

exit 0

