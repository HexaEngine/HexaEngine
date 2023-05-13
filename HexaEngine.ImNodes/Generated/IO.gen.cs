using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using ImGuiNET;

namespace ImNodesNET
{
    public unsafe partial struct IO
    {
        public EmulateThreeButtonMouse emulate_three_button_mouse;
        public LinkDetachWithModifierClick link_detach_with_modifier_click;
    }
    public unsafe partial struct IOPtr
    {
        public IO* NativePtr { get; }
        public IOPtr(IO* nativePtr) => NativePtr = nativePtr;
        public IOPtr(IntPtr nativePtr) => NativePtr = (IO*)nativePtr;
        public static implicit operator IOPtr(IO* nativePtr) => new(nativePtr);
        public static implicit operator IO* (IOPtr wrappedPtr) => wrappedPtr.NativePtr;
        public static implicit operator IOPtr(IntPtr nativePtr) => new(nativePtr);
        public ref EmulateThreeButtonMouse emulate_three_button_mouse => ref Unsafe.AsRef<EmulateThreeButtonMouse>(&NativePtr->emulate_three_button_mouse);
        public ref LinkDetachWithModifierClick link_detach_with_modifier_click => ref Unsafe.AsRef<LinkDetachWithModifierClick>(&NativePtr->link_detach_with_modifier_click);
        public void Destroy()
        {
            ImNodesNative.IO_destroy(NativePtr);
        }
    }
}