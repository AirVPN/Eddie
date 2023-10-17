Compile libcurl under VS2022

Latest: 8.2.1 2023/08/14

Download and extract curl-*.zip

Open X64 Native Tools Command Prompt for VS 2022
Goto winbuild subdir
nmake /f Makefile.vc mode=static machine=x64 RTLIBCFG=static

Open X86 Native Tools Command Prompt for VS 2022
Goto winbuild subdir
nmake /f Makefile.vc mode=static machine=x86 RTLIBCFG=static




Copy curl\builds\libcurl-vc-x86-release-static-ipv6-sspi-winssl\include
in \src\Lib.Platform.Windows.Native\curl\
Copy curl\builds\libcurl-vc-x86-release-static-ipv6-sspi-winssl-obj-lib\libcurl_a.lib
in \src\Lib.Platform.Windows.Native\curl\lib\x86

Copy curl\builds\libcurl-vc-x64-release-static-ipv6-sspi-winssl-obj-lib\libcurl_a.lib
in \src\Lib.Platform.Windows.Native\curl\lib\x64