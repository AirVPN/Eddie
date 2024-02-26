#!/bin/sh
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
${HERE}/opt/eddie-cli/eddie-cli --path=home "$@"
