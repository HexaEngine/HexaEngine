#ifndef HEXAENGINE_COMMON_H
#define HEXAENGINE_COMMON_H

#if defined(_WIN32) || defined(_WIN64)
#define HEXAENGINE_PLATFORM_WINDOWS
#elif defined(__linux__)
#define HEXAENGINE_PLATFORM_LINUX
#elif defined(__APPLE__)
#define HEXAENGINE_PLATFORM_MACOS
#else
#error "Unsupported platform"
#endif

#include <stdio.h>
#include <stdint.h>
/* Calling convention */

#if defined(_WIN32) || defined(_WIN64)
#define HEXAENGINE_CALL __cdecl
#elif defined(__GNUC__) || defined(__clang__)
#define HEXAENGINE_CALL __attribute__((__cdecl__))
#else
#define HEXAENGINE_CALL
#endif

/* API export/import */
#if defined(_WIN32) || defined(_WIN64)
#ifdef HEXAENGINE_BUILD_SHARED
#define HEXAENGINE_EXPORT __declspec(dllexport)
#else
#define HEXAENGINE_EXPORT __declspec(dllimport)
#endif
#elif defined(__GNUC__) || defined(__clang__)
#ifdef HEXAENGINE_BUILD_SHARED
#define HEXAENGINE_EXPORT __attribute__((visibility("default")))
#else
#define HEXAENGINE_EXPORT
#endif
#else
#define HEXAENGINE_EXPORT
#endif

#if defined __cplusplus
#define HEXAENGINE_EXTERN extern "C"
#else
#include <stdarg.h>
#include <stdbool.h>
#define HEXAENGINE_EXTERN extern
#endif

#define HEXAENGINE_API(type) HEXAENGINE_EXTERN HEXAENGINE_EXPORT type HEXAENGINE_CALL
#define HEXAENGINE_API_INTERNAL(type) HEXAENGINE_EXTERN type HEXAENGINE_CALL

#endif