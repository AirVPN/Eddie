#!/bin/sh

# <2.24.3 version
#SELF=$(readlink -f "$0")
#HERE=${SELF%/*}
#cd ${HERE}/opt/eddie-ui/
#./eddie-ui --path=home "$@"

# >=2.24.3 version, see repository/linux_appimage/readme.txt
echo AppImage, extract
TMPDIR=$(mktemp -d)
cp $ARGV0 "$TMPDIR/"
cd $TMPDIR
$ARGV0 --appimage-extract >/dev/null 2>&1
rm $ARGV0
cd "squashfs-root/opt/eddie-ui/"
echo AppImage, run
./eddie-ui --path=home "$@"
echo AppImage, cleaning
rm -rf "$TMPDIR"
