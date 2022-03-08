#!/bin/bash

set -e

# Project to debug different linux compilation flags.
# Run this ./build.sh and after ./mytest, ensure do not crash with a "segmentation fault".

# 2022-03-07
# Eddie 2.21.5 will be compiled without any -static.

# 2022-03-06
# There are issue related to pthread and static compilation, for example
# https://gcc.gnu.org/bugzilla/show_bug.cgi?id=95989

# We build portable & AppImage build, and to avoid issues over different distro, we need static compilation.
# For example, CentOS7.6 don't work without static.
# For example, Arch Manjaro 21.2.4 don't work with static (custom case in App.CLI.Elevated/build.sh)

# General contro about static version: big file
# General contro about static version: DEB Lintian throw a warning
# Note: Arch is expected to be build from AUR, so shared is OK

# Current, static version (Eddie <2.21.5)
# Works on every tested linux, NOT in recent Arch (2022-03-07)
#g++ -o "mytest" "test.cpp" -Wall -std=c++11 -O3 -static -pthread -Wl,--whole-archive -lpthread -Wl,--no-whole-archive

# Plain static version, crash
g++ -o "mytest" "test.cpp" ${FILES} -Wall -std=c++11 -O3 -static -pthread

chmod a+x "mytest"
echo Done
exit 0
