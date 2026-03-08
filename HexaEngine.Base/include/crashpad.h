#ifndef HEXAENGINE_CRASHPAD_H
#define HEXAENGINE_CRASHPAD_H

#include "common.h"

typedef struct Crashpad_CrashInfo
{
    const char* dumpPath;
    const char* logPath;
    const char* appName;
    const char* appVersion;
    const char* additionalInfo;
} Crashpad_CrashInfo;

HEXAENGINE_API(void) Crashpad_Setup(const Crashpad_CrashInfo* crashInfo);

#endif