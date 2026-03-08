
cmake_minimum_required(VERSION 3.16)

project(HexaEngine.Base LANGUAGES C CXX)

set(CMAKE_C_STANDARD 99)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_C_EXTENSIONS OFF)

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

option(HEXAENGINE_BUILD_SHARED "Build HexaEngine.Base as a shared library" ON)

file(GLOB_RECURSE HEXAENGINE_BASE_HEADERS "${CMAKE_CURRENT_SOURCE_DIR}/include/*.h")
file(GLOB_RECURSE HEXAENGINE_BASE_SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")
file(GLOB_RECURSE HEXAENGINE_BASE_C_SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/src/*.c")
list(APPEND HEXAENGINE_BASE_SOURCES ${HEXAENGINE_BASE_C_SOURCES})

if(HEXAENGINE_BUILD_SHARED)
	add_library(HexaEngine.Base SHARED ${HEXAENGINE_BASE_SOURCES})
    target_compile_definitions(HexaEngine.Base PUBLIC HEXAENGINE_BUILD_SHARED=1)
else()
	add_library(HexaEngine.Base STATIC ${HEXAENGINE_BASE_SOURCES})
endif()

target_include_directories(HexaEngine.Base PUBLIC
	$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
	$<INSTALL_INTERFACE:include>
)

target_compile_features(HexaEngine.Base PUBLIC cxx_std_23 c_std_99)

if(MSVC)
	target_compile_options(HexaEngine.Base PRIVATE /W4 /permissive-)
else()
	target_compile_options(HexaEngine.Base PRIVATE -Wall -Wextra -Wpedantic)
endif()

set_target_properties(HexaEngine.Base PROPERTIES
	VERSION 0.1
	SOVERSION 0
	PUBLIC_HEADER "${HEXAENGINE_BASE_HEADERS}"
)

include(GNUInstallDirs)
install(TARGETS HexaEngine.Base
	EXPORT HexaEngineBaseTargets
	ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
	LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
	RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
	PUBLIC_HEADER DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}
)

install(EXPORT HexaEngineBaseTargets
	FILE HexaEngineBaseTargets.cmake
	NAMESPACE HexaEngine::
	DESTINATION ${CMAKE_INSTALL_LIBDIR}/cmake/HexaEngine.Base
)