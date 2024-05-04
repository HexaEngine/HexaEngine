namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.UI;
    using HexaEngine.Editor;
    using HexaEngine.Editor.Attributes;
    using Microsoft.Diagnostics.Symbols;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Etlx;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Parsers.Clr;
    using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
    using Microsoft.Diagnostics.Tracing.Session;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    public static class ProfilingPermission
    {
        private const uint TRACELOG_GUID_ENABLE = 0x0080;
        private const int S_OK = 0;  // ERROR_SUCCESS in C++
        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        // read https://docs.microsoft.com/en-us/windows/win32/etw/configuring-and-starting-a-systemtraceprovider-session
        // for more details
        public static void EnableProfilerUser(string accountName)
        {
            // Kernel provider from https://github.com/microsoft/perfview/blob/master/src/TraceEvent/Parsers/KernelTraceEventParser.cs#L43
            Guid kernelProviderGuid = new("{9e814aad-3204-11d2-9a82-006008a86939}");
            byte[] sid = LookupSidByName(accountName);

            // from https://docs.microsoft.com/en-us/windows/win32/etw/configuring-and-starting-a-systemtraceprovider-session
            uint operation = (uint)EventSecurityOperation.EventSecurityAddDACL;
            uint rights = TRACELOG_GUID_ENABLE;
            bool allowOrDeny = "Allow" != null;
            uint result = EventAccessControl(
                ref kernelProviderGuid,
                operation,
                sid,
                rights,
                allowOrDeny
            );

            if (result != S_OK)
            {
                var lastErrorMessage = new Win32Exception((int)result).Message;
                throw new InvalidOperationException($"Failed to add ACL ({result.ToString()}) : {lastErrorMessage}");
            }
        }

        private static byte[] LookupSidByName(string accountName)
        {
            byte[] sid = null;
            uint cbSid = 0;
            StringBuilder referencedDomainName = new StringBuilder();
            uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
            SID_NAME_USE sidUse;

            int err = S_OK;
            if (!LookupAccountName(null, accountName, sid, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out sidUse))
            {
                err = Marshal.GetLastWin32Error();
                if (err == ERROR_INSUFFICIENT_BUFFER)
                {
                    sid = new byte[cbSid];
                    referencedDomainName.EnsureCapacity((int)cchReferencedDomainName);
                    err = S_OK;
                    if (!LookupAccountName(null, accountName, sid, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out sidUse))
                    {
                        err = Marshal.GetLastWin32Error();
                    }
                }
            }

            if (err != S_OK)
            {
                var lastErrorMessage = new Win32Exception(err).Message;
                throw new InvalidOperationException($"LookupAccountName fails ({err.ToString()}) : {lastErrorMessage}");
            }

            // display the SID associated to the given user
            nint ptrSid;
            if (!ConvertSidToStringSid(sid, out ptrSid))
            {
                err = Marshal.GetLastWin32Error();
                var lastErrorMessage = new Win32Exception(err).Message;
                Console.WriteLine($"No SID string associated to user {accountName} ({err.ToString()}) : {lastErrorMessage}");
            }
            else
            {
                string sidString = Marshal.PtrToStringAuto(ptrSid);
                LocalFree(ptrSid);
                Console.WriteLine($"Account ({referencedDomainName}){accountName} mapped to {sidString}");
            }

            return sid;
        }

        [DllImport("Sechost.dll", SetLastError = true)]
        private static extern uint EventAccessControl(
            ref Guid providerGuid,
            uint operation,
            [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            uint right,
            bool allowOrDeny // true means ALLOW
            );

        [DllImport("kernel32.dll")]
        private static extern nint LocalFree(nint hMem);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupAccountName(
            string systemName,
            string accountName,
            [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            ref uint cbSid,
            StringBuilder referencedDomainName,
            ref uint cchReferencedDomainName,
            out SID_NAME_USE nameUse);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ConvertSidToStringSid(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pSID,
            out nint ptrSid); // can't be an out string because we need to explicitly call LocalFree on it;

        // the marshaller would call CoTaskMemFree in case of a string

        // from http://pinvoke.net/default.aspx/advapi32/LookupAccountName.html
        private enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        // from evntcons.h
        private enum EventSecurityOperation
        {
            EventSecuritySetDACL = 0,
            EventSecuritySetSACL,
            EventSecurityAddDACL,
            EventSecurityAddSACL,
            EventSecurityMax
        } // EVENTSECURITYOPERATION
    }

    public class MergedSymbolicStacks
    {
        private int _countAsNode;
        private int _countAsLeaf;
        private List<MergedSymbolicStacks> _stacks;
        private Guid guid = Guid.NewGuid();

        public MergedSymbolicStacks() : this(0, string.Empty)
        {
            // this will be the root of all stacks
        }

        private MergedSymbolicStacks(ulong frame, string symbol)
        {
            Frame = frame;
            Symbol = symbol;
            _countAsNode = 0;
            _countAsLeaf = 0;
            _stacks = [];
            DisplayName = $"{symbol}##{guid}";
        }

        public ulong Frame { get; private set; }

        public string Symbol { get; private set; }

        public int CountAsNode => _countAsNode;

        public int CountAsLeaf => _countAsLeaf;

        public List<MergedSymbolicStacks> Stacks { get => _stacks; set => _stacks = value; }

        public string DisplayName { get; private set; }

        public void AddStack(SymbolicFrame[] frames, int index = 0)
        {
            _countAsNode++;

            var firstFrame = frames[index];

            // search if the frame to add has already been seen
            var callstack = Stacks.FirstOrDefault(s => string.CompareOrdinal(s.Symbol, firstFrame.Symbol) == 0);

            // if not, we are starting a new branch
            if (callstack == null)
            {
                callstack = new MergedSymbolicStacks(frames[index].Address, frames[index].Symbol);
                Stacks.Add(callstack);
            }

            // it was the last frame of the stack
            if (index == frames.Length - 1)
            {
                callstack._countAsLeaf++;
                return;
            }

            callstack.AddStack(frames, index + 1);
        }

        public void Reset()
        {
            _countAsNode = 0;
            _countAsLeaf = 0;
            _stacks.Clear();
        }
    }

    [EditorWindowCategory("Experimental")]
    public class DeepProfilerWindow : EditorWindow
    {
        private TraceEventSession? _session;
        private Task? _profilingTask;

        private readonly MergedSymbolicStacks _stacks = new();
        private int _stackCount;
        private readonly Dictionary<TraceModuleFile, bool> _missingSymbols = [];

        private bool _isProfiling = false;

        private static readonly int Pid = Environment.ProcessId;
        private TextWriter log;
        private SymbolReader reader;

        public DeepProfilerWindow()
        {
#if DEBUG
            //IsShown = true;
#endif
        }

        protected override string Name { get; } = $"{UwU.ChartPie} Deep Profiler";

        public bool StartProfiling()
        {
            log = File.CreateText("profile.log");
            reader = new(log);

            // ProfilingPermission.EnableProfilerUser("juna");

            string sessionName = "Cpu_Profiling_Session+" + Guid.NewGuid().ToString();
            _session = new TraceEventSession(sessionName, TraceEventSessionOptions.Create);
            if (!EnableProviders(_session))
            {
                _session.Dispose();
                _session = null;
                return false;
            }

            var Pid = Environment.ProcessId;
            Guid perfInfoTaskGuid = new(0xce1dbfb4, 0x137e, 0x4da6, 0x87, 0xb0, 0x3f, 0x59, 0xaa, 0x10, 0x2c, 0xbc);
            int profileOpcode = 46;

            _profilingTask = Task.Factory.StartNew(() =>
            {
                using TraceLogEventSource source = TraceLog.CreateFromTraceEventSession(_session);

                //source.Clr.All += Clr_All;
                source.Dynamic.All += Dynamic_All;
                //source.Kernel.PerfInfoSample += Kernel_PerfInfoSample;
                source.Process();
                source.Dynamic.All -= Dynamic_All;
                // source.Clr.All -= Clr_All;
            });

            return true;
        }

        private void Dynamic_All(TraceEvent data)
        {
            if (data.ProcessID != Pid || data.EventName == null)
            {
                return;
            }

            if (data is not ClrStackWalkTraceData walkTraceData || walkTraceData.Source is not TraceLog traceLog)
            {
                ImGuiConsole.WriteLine($"TID: {data.ThreadID}, {data.EventName}");
                return;
            }
        }

        private Guid perfInfoTaskGuid = new(0xce1dbfb4, 0x137e, 0x4da6, 0x87, 0xb0, 0x3f, 0x59, 0xaa, 0x10, 0x2c, 0xbc);
        private int profileOpcode = 46;

        private void Kernel_PerfInfoSample(SampledProfileTraceData data)
        {
            if (data.ProcessID != Pid)
            {
                return;
            }

            if (data.TaskGuid != perfInfoTaskGuid)
            {
                return;
            }

            if ((uint)data.Opcode != profileOpcode)
            {
                return;
            }

            var callstack = data.CallStack();
            if (callstack == null)
            {
                return;
            }

            MergeCallStack(callstack, reader);
        }

        public override void Close()
        {
            _session?.Dispose();
            reader.Dispose();
            log.Close();
        }

        private void Clr_All(TraceEvent data)
        {
            if (data.ProcessID != Pid || data.EventName == null)
            {
                return;
            }

            if (data is not ClrStackWalkTraceData walkTraceData || walkTraceData.Source is not TraceLog traceLog)
            {
                ImGuiConsole.WriteLine($"TID: {data.ThreadID}, {data.EventName}");
                return;
            }
        }

        public void StopProfiling()
        {
            _session?.Stop();
            _session = null;
            _profilingTask?.Wait();
            _profilingTask = null;
            _stackCount = 0;
            _stacks.Reset();
            _missingSymbols.Clear();
        }

        private static bool EnableProviders(TraceEventSession session)
        {
            session.BufferSizeMB = 256;

            // Note: the kernel provider MUST be the first provider to be enabled
            // If the kernel provider is not enabled, the callstacks for CLR events are still received
            // but the symbols are not found (except for the application itself)
            // TraceEvent implementation details triggered when a module (image) is loaded
            /*
            session.EnableKernelProvider(
                KernelTraceEventParser.Keywords.ImageLoad |
                KernelTraceEventParser.Keywords.Process |
                KernelTraceEventParser.Keywords.Profile,
                stackCapture: KernelTraceEventParser.Keywords.Profile
            );
            */

            // this call always returns false  :^(
            bool success = session.EnableProvider(
                     ClrTraceEventParser.ProviderGuid,
                     TraceEventLevel.Verbose,
                     (ulong)(
        // events related to JITed methods
        ClrTraceEventParser.Keywords.GC |
        ClrTraceEventParser.Keywords.Jit |                      // Turning on JIT events is necessary to resolve JIT compiled code
        ClrTraceEventParser.Keywords.JittedMethodILToNativeMap |// This is needed if you want line number information in the stacks
        ClrTraceEventParser.Keywords.Loader |                   // You must include loader events as well to resolve JIT compiled code.
        ClrTraceEventParser.Keywords.Default | ClrTraceEventParser.Keywords.Debugger | ClrTraceEventParser.Keywords.PerfTrack

                     )
                 );

            // this provider will send events of already JITed methods
            success = session.EnableProvider(
                ClrRundownTraceEventParser.ProviderGuid,
                TraceEventLevel.Verbose,
                (ulong)(
                ClrTraceEventParser.Keywords.Jit |              // We need JIT events to be rundown to resolve method names
                ClrTraceEventParser.Keywords.JittedMethodILToNativeMap | // This is needed if you want line number information in the stacks
                ClrTraceEventParser.Keywords.Loader |           // As well as the module load events.
                ClrTraceEventParser.Keywords.StartEnumeration |   // This indicates to do the rundown now (at enable time)
                ClrTraceEventParser.Keywords.PerfTrack | ClrTraceEventParser.Keywords.Debugger | ClrTraceEventParser.Keywords.Default
                ));

            return true;
        }

        protected void MergeCallStack(TraceCallStack callStack, SymbolReader reader)
        {
            var currentFrame = callStack.Depth;
            var frames = new SymbolicFrame[callStack.Depth];

            // the first element of callstack is the last frame: we need to iterate on each frame
            // up to the first one before adding them into the MergedSymbolicStack
            while (callStack != null)
            {
                var codeAddress = callStack.CodeAddress;
                if (codeAddress.Method == null)
                {
                    var moduleFile = codeAddress.ModuleFile;
                    if (moduleFile != null)
                    {
                        // TODO: this seems to trigger extremely slow retrieval of symbols
                        //       through HTTP requests: see how to delay it AFTER the user
                        //       stops the profiling
                        if (!_missingSymbols.TryGetValue(moduleFile, out var _))
                        {
                            codeAddress.CodeAddresses.LookupSymbolsForModule(reader, moduleFile);
                            if (codeAddress.Method == null)
                            {
                                _missingSymbols[moduleFile] = true;
                            }
                        }
                    }
                }

                frames[--currentFrame] = new SymbolicFrame(
                    codeAddress.Address,
                    string.IsNullOrEmpty(codeAddress.FullMethodName) ? $"{codeAddress.Address:x}" : codeAddress.FullMethodName
                    );

                callStack = callStack.Caller;
            }

            _stackCount++;
            _stacks.AddStack(frames);
        }

        public override void DrawContent(IGraphicsContext context)
        {
            if (ImGui.Checkbox("Profiling", ref _isProfiling))
            {
                if (_isProfiling)
                {
                    StartProfiling();
                }
                else
                {
                    StopProfiling();
                }
            }

            if (_isProfiling)
            {
                RenderStack(_stacks, true, 0);
            }
        }

        private static void RenderStack(MergedSymbolicStacks stack, bool isRoot, int increment)
        {
            var currentFrame = stack.Frame;

            bool isOpen;

            // special root case
            if (isRoot)
            {
                isOpen = ImGui.TreeNode(stack.DisplayName);
                TooltipHelper.Tooltip($"{stack.CountAsNode}");
            }
            else
            {
                isOpen = ImGui.TreeNode(stack.DisplayName);
                TooltipHelper.Tooltip($"{stack.CountAsLeaf + stack.CountAsNode}");
            }

            var childrenCount = stack.Stacks.Count;
            if (childrenCount == 0)
            {
                ImGui.Separator();
                if (isOpen)
                {
                    ImGui.TreePop();
                }

                return;
            }
            foreach (var nextStackFrame in stack.Stacks.OrderByDescending(s => s.CountAsNode + s.CountAsLeaf))
            {
                // increment when more than 1 children
                var childIncrement = childrenCount == 1 ? increment : increment + 1;
                RenderStack(nextStackFrame, false, childIncrement);
                if (increment != childIncrement)
                {
                    ImGui.SeparatorText($"{nextStackFrame.CountAsNode + nextStackFrame.CountAsLeaf}");
                    ImGui.SeparatorText($"~~~~ ");
                }
            }

            if (isOpen)
            {
                ImGui.TreePop();
            }
        }
    }
}