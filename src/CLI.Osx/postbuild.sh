#!/bin/bash

echo Eddie - Embed Mono Framework

# Tested with Xamarin Studio 6.1.2 build 44, Mono 4.6.2, macOS Sierra 10.12.1

# Ensure it can find pkg-config:
export PKG_CONFIG_PATH=$PKG_CONFIG_PATH:/usr/lib/pkgconfig:/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig

# Force 32bit build and manually set some clang linker properties:
export AS="as -arch i386"
export CC="cc -arch i386 -lobjc -liconv -framework CoreFoundation -I /Library/Frameworks/Mono.framework/Versions/Current/include/mono-2.0/"

# Force 64bit build and manually set some clang linker properties:
# export AS="as -arch x86_64"
# export CC="cc -arch x86_64 -lobjc -liconv -framework CoreFoundation -I /Library/Frameworks/Mono.framework/Versions/Current/include/mono-2.0/"

# Other ensure it can find pig-config
export PATH=/Library/Frameworks/Mono.framework/Versions/Current/bin/:$PATH


# WARNING: Currently 2017-03-10 , cannot be signed for this bug: https://bugzilla.xamarin.com/show_bug.cgi?id=52443
mkbundle --sdk /Library/Frameworks/Mono.framework/Versions/Current CLI.Osx.exe Lib.Common.dll Lib.Core.dll Platforms.Osx.dll -z --static --deps -o eddie-cli

