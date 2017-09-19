# Minimum version for CMake
cmake_minimum_required(VERSION 2.6)

# Enable C++11
add_definitions("-std=c++11")

# Project's name
project(Lib.Platform.MacOS.Native)

# Grab all files inside ./src folder
file(GLOB SOURCES "src/*.cpp")

#set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/../../../bin/x86/${CMAKE_BUILD_TYPE}/)

# Create a shared library
add_library(Lib.Platform.MacOS.Native SHARED ${SOURCES})

# Static link C/C++ runtime (could be invoked in LINK_FLAGS too)
#target_link_libraries(Lib.Platform.MacOS.Native -static-libgcc)

# Remove the "lib" prefix from the shared library filename
#set_target_properties(Lib.Platform.MacOS.Native PROPERTIES PREFIX "" COMPILE_FLAGS "${CMAKE_BUILD_ARCHITECTURE}" LINK_FLAGS "${CMAKE_BUILD_ARCHITECTURE}")

set_target_properties(Lib.Platform.MacOS.Native PROPERTIES COMPILE_FLAGS "${CMAKE_BUILD_ARCHITECTURE}" LINK_FLAGS "${CMAKE_BUILD_ARCHITECTURE}")
