#!/bin/bash

set -e

echo This exists because mixing already-builded net4.8 and net7 cause issues, so we launch this between switch.


rm -rf App.CLI/bin
rm -rf App.CLI/obj
rm -rf Lib.Core/bin
rm -rf Lib.Core/obj
rm -rf Lib.Platform.MacOS/bin
rm -rf Lib.Platform.MacOS/obj
rm -rf UI.Cocoa.MacOS/bin
rm -rf UI.Cocoa.MacOS/obj

echo Done.

