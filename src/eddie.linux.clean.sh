#!/bin/bash

set -e

echo This exists because mixing already-builded net4.8 and net6 cause issues, so we launch this between switch.

echo For example errors like https://stackoverflow.com/questions/67072200/how-to-fix-add-a-reference-to-netframework-version-v4-7-1-in-the-targetfram
echo when switching between .Net4.8 and .Net6

rm -rf Lib.Core/bin      
rm -rf Lib.Core/obj
rm -rf Lib.Platform.Linux/bin
rm -rf Lib.Platform.Linux/obj
rm -rf Lib.Forms/bin
rm -rf Lib.Forms/obj
rm -rf Lib.Forms.Skin/bin
rm -rf Lib.Forms.Skin/obj
rm -rf App.CLI.Linux/bin
rm -rf App.CLI.Linux/obj
rm -rf App.Forms.Linux/bin
rm -rf App.Forms.Linux/obj

echo Done.
