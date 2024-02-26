#!/bin/sh
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
cd ${HERE}/opt/eddie-ui/
./eddie-ui --path=home "$@"
