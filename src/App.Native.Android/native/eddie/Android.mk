LOCAL_PATH:= $(call my-dir)/

include $(CLEAR_VARS)
LOCAL_C_INCLUDES := openvpn3 openvpn3/client asio/include boost_1_67_0/include breakpad/src
LOCAL_CFLAGS= -funwind-tables -DUSE_OPENSSL -DOPENSSL_NO_ENGINE -DUSE_ASIO
#LOCAL_LDLIBS := -L$(LOCAL_PATH)/../libs/x86 
LOCAL_LDLIBS := -llog
LOCAL_SHARED_LIBRARIES := ovpn3
LOCAL_STATIC_LIBRARIES += breakpad_client
#LOCAL_CFLAGS =  -DTARGET_ARCH_ABI=\"${TARGET_ARCH_ABI}\"
LOCAL_SRC_FILES:=\
	api.cpp\
	breakpad.cpp\
	client.cpp\
	constants.cpp\
	exception.cpp\
	utils.cpp

LOCAL_MODULE = eddie
include $(BUILD_SHARED_LIBRARY)
