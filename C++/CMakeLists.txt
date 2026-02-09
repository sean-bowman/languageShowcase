cmake_minimum_required(VERSION 3.16)
project(hohmann_calculator VERSION 0.1.0 LANGUAGES CXX)

# Require C++17
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Build type
if(NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE Release)
endif()

# Compiler warnings
if(MSVC)
    add_compile_options(/W4)
else()
    add_compile_options(-Wall -Wextra -Wpedantic)
endif()

# Include directories
include_directories(${CMAKE_SOURCE_DIR}/include)

# Library sources
set(LIB_SOURCES
    src/celestial_body.cpp
    src/orbit.cpp
    src/hohmann_transfer.cpp
)

# Create library
add_library(hohmann_lib ${LIB_SOURCES})
target_include_directories(hohmann_lib PUBLIC ${CMAKE_SOURCE_DIR}/include)

# Main executable
add_executable(hohmann src/main.cpp)
target_link_libraries(hohmann hohmann_lib)

# Examples
add_executable(leo_to_geo examples/leo_to_geo.cpp)
target_link_libraries(leo_to_geo hohmann_lib)

add_executable(earth_mars examples/earth_mars.cpp)
target_link_libraries(earth_mars hohmann_lib)

# Install
install(TARGETS hohmann DESTINATION bin)
