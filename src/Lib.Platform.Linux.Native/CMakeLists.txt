# Minimum version for CMake
cmake_minimum_required(VERSION 2.6)

# Project's name
project(Lib.Platform.Linux.Native)

# Grab all files inside ./src folder
file(GLOB SOURCES "src/*.cpp")

#set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/../../../bin/x86/${CMAKE_BUILD_TYPE}/)

# Create a shared library
add_library(Lib.Platform.Linux.Native SHARED ${SOURCES})

# Remove the "lib" prefix from the shared library filename
#set_target_properties(Lib.Platform.Linux.Native PROPERTIES PREFIX "" COMPILE_FLAGS "${CMAKE_BUILD_ARCHITECTURE}" LINK_FLAGS "${CMAKE_BUILD_ARCHITECTURE}")

#set_target_properties(Lib.Platform.Linux.Native PROPERTIES COMPILE_FLAGS "${CMAKE_BUILD_ARCHITECTURE}" LINK_FLAGS "${CMAKE_BUILD_ARCHITECTURE}")
set_target_properties(Lib.Platform.Linux.Native PROPERTIES COMPILE_FLAGS "${CMAKE_BUILD_ARCHITECTURE}" LINK_FLAGS "${CMAKE_BUILD_ARCHITECTURE} -static-libgcc -static-libstdc++")
