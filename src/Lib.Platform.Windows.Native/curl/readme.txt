Compile libcurl under VS2017

Go to
\src\Lib.Platform.Windows.Native\curl\winbuild\

Open VS2017 developer prompt, TWO times (x86 and x64)

nmake /f Makefile.vc mode=static machine=x64 RTLIBCFG=static

nmake /f Makefile.vc mode=static machine=x86 RTLIBCFG=static
(in both arch)

Copy curl\builds\libcurl-vc-x86-release-static-ipv6-sspi-winssl\include
in \src\Lib.Platform.Windows.Native\curl\
Copy curl\builds\libcurl-vc-x86-release-static-ipv6-sspi-winssl-obj-lib\libcurl_a.lib
in \src\Lib.Platform.Windows.Native\curl\lib\x86

Copy curl\builds\libcurl-vc-x64-release-static-ipv6-sspi-winssl-obj-lib\libcurl_a.lib
in \src\Lib.Platform.Windows.Native\curl\lib\x64