#ifeq ($(TARGET_ARCH),mips)
#	WITH_BREAKPAD=0
#else ifeq ($(TARGET_ARCH),mips64)
#	WITH_BREAKPAD=0
#else ifeq ($(USE_BREAKPAD),1)
#	WITH_BREAKPAD=1
#else
#	WITH_BREAKPAD=0
#endif

include breakpad/android/google_breakpad/Android.mk 
include lzo/Android.mk
include lz4/Android.mk
include openssl/Android.mk
#include mbedtls/Android.mk
include openvpn3/Android.mk

include eddie/Android.mk
