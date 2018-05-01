# Minimum version for CMake
cmake_minimum_required(VERSION 2.6)

# Import the pkg_search_module command
include(FindPkgConfig)

# Enable C++11
add_definitions("-std=c++11")

# Project's name
project(eddie_tray)

set(SOURCES
    ./src/main.cpp)

add_executable(eddie_tray ${SOURCES})

pkg_search_module(GTK2 REQUIRED gtk+-2.0)

#target_include_directories(
#    eddie_tray PUBLIC
#    /usr/include/libappindicator-0.1
#    ${GTK2_INCLUDE_DIRS}
#)
# Debian7 cmake support
include_directories(
    PUBLIC
    /usr/include/libappindicator-0.1
    ${GTK2_INCLUDE_DIRS}
)
target_link_libraries(eddie_tray -static-libgcc -static-libstdc++ pthread appindicator ${GTK2_LIBRARIES})

set_target_properties(eddie_tray PROPERTIES COMPILE_FLAGS "${CMAKE_BUILD_ARCHITECTURE}" LINK_FLAGS "${CMAKE_BUILD_ARCHITECTURE}")

