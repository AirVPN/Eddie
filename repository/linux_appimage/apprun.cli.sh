#!/bin/sh

# <2.24.3 version
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
${HERE}/opt/eddie-cli/eddie-cli --path=home "$@"

# >=2.24.3 version, see repository/linux_appimage/readme.txt
echo AppImage, extract
TMPDIR=$(mktemp -d)
cp $ARGV0 "$TMPDIR/"
cd $TMPDIR
$ARGV0 --appimage-extract >/dev/null 2>&1
rm $ARGV0
cd "squashfs-root/opt/eddie-cli/"
echo AppImage, run
./eddie-cli --path=home "$@"
echo AppImage, cleaning
rm -rf "$TMPDIR"
