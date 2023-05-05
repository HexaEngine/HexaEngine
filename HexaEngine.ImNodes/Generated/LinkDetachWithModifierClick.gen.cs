using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using ImGuiNET;

namespace ImNodesNET
{
    public unsafe partial struct LinkDetachWithModifierClick
    {
        public byte* modifier;
    }
    public unsafe partial struct LinkDetachWithModifierClickPtr
    {
        public LinkDetachWithModifierClick* NativePtr { get; }
        public LinkDetachWithModifierClickPtr(LinkDetachWithModifierClick* nativePtr) => NativePtr = nativePtr;
        public LinkDetachWithModifierClickPtr(IntPtr nativePtr) => NativePtr = (LinkDetachWithModifierClick*)nativePtr;
        public static implicit operator LinkDetachWithModifierClickPtr(LinkDetachWithModifierClick* nativePtr) => new(nativePtr);
        public static implicit operator LinkDetachWithModifierClick* (LinkDetachWithModifierClickPtr wrappedPtr) => wrappedPtr.NativePtr;
        public static implicit operator LinkDetachWithModifierClickPtr(IntPtr nativePtr) => new(nativePtr);
        public IntPtr modifier { get => (IntPtr)NativePtr->modifier; set => NativePtr->modifier = (byte*)value; }
        public void Destroy()
        {
            ImNodesNative.LinkDetachWithModifierClick_destroy(NativePtr);
        }
    }
}
