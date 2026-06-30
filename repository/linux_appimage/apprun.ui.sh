#!/bin/sh

echo AppImage, extract
TMPDIR=$(mktemp -d)
trap 'rm -rf "$TMPDIR"' EXIT INT TERM
cp "$ARGV0" "$TMPDIR/"
cd "$TMPDIR"
"$ARGV0" --appimage-extract >/dev/null 2>&1
rm "./$(basename "$ARGV0")"
cd "squashfs-root/opt/eddie-ui/"
echo AppImage, run
./eddie-ui --path=home "$@"
echo AppImage, cleaning
