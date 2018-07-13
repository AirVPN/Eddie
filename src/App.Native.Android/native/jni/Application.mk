ifeq ($(USE_SHORT_COMMANDS),1)
    APP_SHORT_COMMANDS := true
endif

#APP_ABI := arm64-v8a armeabi armeabi-v7a x86 x86_64
#APP_ABI := x86
#APP_ABI := armeabi-v7a
APP_ABI := arm64-v8a armeabi-v7a x86
APP_PLATFORM := android-22

#APP_STL:=stlport_static
APP_STL:=c++_static

#APP_OPTIM := release

#NDK_TOOLCHAIN_VERSION=4.9
APP_CPPFLAGS += -std=c++1y
APP_CFLAGS += -funwind-tables
