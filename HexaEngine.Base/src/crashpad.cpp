#include "crashpad.h"
#include <stdio.h>
#include <fstream>
#include <string>
#include <format>
#include <vector>
#include <sstream>
#include <iostream>

#if defined(HEXAENGINE_PLATFORM_WINDOWS)
#include <windows.h>
#include <dbghelp.h>
#pragma comment(lib, "dbghelp.lib")
#endif

struct CrashpadInternalInfo
{
    std::string dumpPath;
    std::string logPath;
    std::string appName;
    std::string appVersion;
    std::string additionalInfo;
};

CrashpadInternalInfo crashpadInfo;

class LogWriter
{
    std::ofstream logFile;
    size_t indentLevel = 0;
    bool shouldIndentNext = true;
public:
    LogWriter(const std::string& filePath)
    {
        logFile.open(filePath, std::ios::out | std::ios::app);
        if (!logFile.is_open())
        {
            printf("Failed to open log file: %s\n", filePath.c_str());
        }
    }

    ~LogWriter()
    {
        if (logFile.is_open())
        {
            logFile.close();
        }
    }

    void Indent() { indentLevel++; }
    void Unindent() { if (indentLevel > 0) indentLevel--; }

    void Write(const std::string& message)
    {
        if (shouldIndentNext)
        {
            logFile << std::string(indentLevel, '\t');
            shouldIndentNext = false;
        }
        if (logFile.is_open())
        {
            logFile << message;
        }
        if (message.ends_with('\n'))
        {
            shouldIndentNext = true;
        }
    }

    template<typename... Args>
    void WriteFormat(const std::string& formatStr, Args&&... args)
    {
        Write(std::format(formatStr, std::forward<Args>(args)...));
    }

    void WriteLine(const std::string& message = "")
    {
        if (logFile.is_open())
        {
            Write(message);
            Write("\n");
        }
    }

    template<typename... Args>
    void WriteLineFormat(const std::string& formatStr, Args&&... args)
    {
        WriteLine(std::format(formatStr, std::forward<Args>(args)...));
    }
};

struct ReportInfo
{
    uint64_t exceptionCode;
    uintptr_t exceptionAddress;
    uint64_t exceptionFlags;
    std::string message;
    std::string cpuContext;
    std::string callStack;
};

static void WriteReport(LogWriter& logWriter, const ReportInfo& reportInfo)
{
    logWriter.WriteLineFormat("{}: v{}", crashpadInfo.appName, crashpadInfo.appVersion);
    logWriter.WriteLineFormat("Dump Path: {}", crashpadInfo.dumpPath);
    logWriter.WriteLineFormat("Log Path: {}", crashpadInfo.logPath);
    if (crashpadInfo.additionalInfo.empty() == false)
    {
        logWriter.WriteLine(crashpadInfo.additionalInfo);
    }
    logWriter.WriteLine();
    logWriter.WriteLine();

    logWriter.WriteLineFormat("Unhandled exception {} {} at {}", reportInfo.exceptionCode, reportInfo.exceptionFlags, reportInfo.exceptionAddress);
    logWriter.Indent();
    logWriter.WriteLine(reportInfo.message);
    logWriter.Unindent();
    logWriter.WriteLine();

    logWriter.WriteLine();
    logWriter.WriteLine();
    logWriter.Indent();
    logWriter.WriteLine(reportInfo.cpuContext);
    logWriter.WriteLine(reportInfo.callStack);
    logWriter.Unindent();
    logWriter.WriteLine();
    logWriter.WriteLine();
}

#if defined(HEXAENGINE_PLATFORM_WINDOWS)

std::vector<void*> CaptureCallStack(unsigned int maxFrames = 62)
{
    void* stack[62];
    USHORT frames = CaptureStackBackTrace(0, maxFrames, stack, nullptr);
    return std::vector<void*>(stack, stack + frames);
}

std::string PrintStackTrace(LogWriter& logWriter, const std::vector<void*>& stack)
{
    std::ostringstream os;
    HANDLE process = GetCurrentProcess();
    SymInitialize(process, nullptr, TRUE);

    DWORD64 displacement = 0;
    char buffer[sizeof(SYMBOL_INFO) + 1024] = {};
    PSYMBOL_INFO symbol = reinterpret_cast<PSYMBOL_INFO>(buffer);
    symbol->SizeOfStruct = sizeof(SYMBOL_INFO);
    symbol->MaxNameLen = 1024;
    for (void* addr : stack)
    {
        displacement = 0;
        if (SymFromAddr(process, (DWORD64)addr, &displacement, symbol))
        {
            os << "\t" << symbol->Name << " + 0x" << std::hex << displacement << std::dec << "\n";
        }
        else
        {
            os << "\t" << "Unknown function at " << addr << "\n";
        }
    }
    return os.str();
}

inline std::string SehCodeToString(DWORD code)
{
    switch (code)
    {
        case EXCEPTION_ACCESS_VIOLATION:        return "Access violation";
        case EXCEPTION_ARRAY_BOUNDS_EXCEEDED:   return "Array bounds exceeded";
        case EXCEPTION_BREAKPOINT:              return "Breakpoint";
        case EXCEPTION_DATATYPE_MISALIGNMENT:   return "Datatype misalignment";
        case EXCEPTION_FLT_DENORMAL_OPERAND:    return "Float denormal operand";
        case EXCEPTION_FLT_DIVIDE_BY_ZERO:      return "Float divide by zero";
        case EXCEPTION_FLT_INEXACT_RESULT:      return "Float inexact result";
        case EXCEPTION_FLT_INVALID_OPERATION:   return "Float invalid operation";
        case EXCEPTION_FLT_OVERFLOW:            return "Float overflow";
        case EXCEPTION_FLT_STACK_CHECK:         return "Float stack check";
        case EXCEPTION_FLT_UNDERFLOW:           return "Float underflow";
        case EXCEPTION_ILLEGAL_INSTRUCTION:     return "Illegal instruction";
        case EXCEPTION_IN_PAGE_ERROR:           return "In-page error";
        case EXCEPTION_INT_DIVIDE_BY_ZERO:      return "Integer divide by zero";
        case EXCEPTION_INT_OVERFLOW:            return "Integer overflow";
        case EXCEPTION_INVALID_DISPOSITION:     return "Invalid disposition";
        case EXCEPTION_NONCONTINUABLE_EXCEPTION:return "Non-continuable exception";
        case EXCEPTION_PRIV_INSTRUCTION:        return "Privileged instruction";
        case EXCEPTION_STACK_OVERFLOW:          return "Stack overflow";
        default:
            return "Unknown SEH exception (0x" + std::to_string(code) + ")";
    }
}

LONG WINAPI WinUnhandledExceptionHandler(EXCEPTION_POINTERS* ExceptionInfo) 
{
    auto rec = ExceptionInfo->ExceptionRecord;
    LogWriter logWriter(crashpadInfo.logPath);

    ReportInfo reportInfo;
    reportInfo.exceptionCode = rec->ExceptionCode;
    reportInfo.exceptionAddress = (uintptr_t)rec->ExceptionAddress;
    reportInfo.exceptionFlags = rec->ExceptionFlags;
    reportInfo.message = SehCodeToString(rec->ExceptionCode);
    
    WriteReport(logWriter, reportInfo);
    logWriter.WriteLineFormat("Unhandled exception {} {} at {}", rec->ExceptionCode, rec->ExceptionFlags, rec->ExceptionAddress);
    logWriter.Indent();
    logWriter.WriteLine(SehCodeToString(rec->ExceptionCode));
    logWriter.Unindent();
    logWriter.WriteLine();

    logWriter.WriteLine();
    logWriter.WriteLine();
    auto stack = CaptureCallStack();
    PrintStackTrace(logWriter, stack);
    logWriter.WriteLine();
    logWriter.WriteLine();

    return EXCEPTION_EXECUTE_HANDLER;
}
#endif

HEXAENGINE_API_INTERNAL(void) Crashpad_Setup(const Crashpad_CrashInfo* crashInfo)
{
    crashpadInfo.dumpPath = crashInfo->dumpPath;
    crashpadInfo.logPath = crashInfo->logPath;
    crashpadInfo.appName = crashInfo->appName;
    crashpadInfo.appVersion = crashInfo->appVersion;
    crashpadInfo.additionalInfo = crashInfo->additionalInfo;
#if defined(HEXAENGINE_PLATFORM_WINDOWS)
    SetUnhandledExceptionFilter(WinUnhandledExceptionHandler);
#endif
}