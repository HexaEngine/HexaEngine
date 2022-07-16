﻿namespace HexaEngine.Core.Unsafes
{
    using HexaEngine.Core.Graphics;
    using System;

    public unsafe class Vector<T> where T : IDeviceChild
    {
        private IDeviceChild[] children = Array.Empty<IDeviceChild>();
        private void** data;

        public T this[int index] { get => (T)children[index]; set => children[index] = value; }

        public uint Length => (uint)children.Length;

        public void** Data => data;

        public void Push(T srv)
        {
            Array.Resize(ref children, children.Length + 1);
            children[^1] = srv;
            data = Utilities.ToPointerArray(children);
        }

        public void Clear()
        {
            children = Array.Empty<IDeviceChild>();
            data = Utilities.ToPointerArray(children);
        }

        public void Pop()
        {
            Array.Resize(ref children, children.Length - 1);
            data = Utilities.ToPointerArray(children);
        }

        public void Resize(uint length)
        {
            Array.Resize(ref children, (int)length);
            data = Utilities.ToPointerArray(children);
        }
    }

    public unsafe struct Pointer<T> : IEquatable<Pointer<T>> where T : unmanaged
    {
        public T* Data;

        public Pointer(T* pointer)
        {
            Data = pointer;
        }

        public override bool Equals(object? obj)
        {
            return obj is Pointer<T> pointer && Equals(pointer);
        }

        public bool Equals(Pointer<T> other)
        {
            return Data == other.Data;
        }

        public static bool operator ==(Pointer<T> left, Pointer<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Pointer<T> left, Pointer<T> right)
        {
            return !(left == right);
        }

        public static implicit operator T*(Pointer<T> pointer) => pointer.Data;

        public static implicit operator Pointer<T>(T* pointer) => new(pointer);

        public static implicit operator IntPtr(Pointer<T> pointer) => (IntPtr)pointer.Data;

        public static implicit operator Pointer<T>(IntPtr pointer) => new((T*)pointer);

        public static implicit operator Pointer<T>(T[] values) => Utilities.AsPointer(values);

        public override int GetHashCode()
        {
            return Data->GetHashCode();
        }
    }

    public unsafe struct PointerPointer<T> where T : unmanaged
    {
        public T** Data;

        public PointerPointer(T** pointer)
        {
            Data = pointer;
        }

        public static implicit operator T**(PointerPointer<T> pointer) => pointer.Data;

        public static implicit operator PointerPointer<T>(T** pointer) => new(pointer);

        public static implicit operator IntPtr(PointerPointer<T> pointer) => (IntPtr)pointer.Data;

        public static implicit operator PointerPointer<T>(IntPtr pointer) => new((T**)pointer);

        public static implicit operator PointerPointer<T>(T*[] values) => Utilities.AsPointer(values);

        public static implicit operator PointerPointer<T>(Pointer<T>[] pointers) => Utilities.AsPointer(pointers);

        public static implicit operator PointerPointer<T>(T[][] values) => Utilities.AsPointer(values);
    }

    public unsafe struct VectorN<T> where T : unmanaged
    {
        public T* Data;

        public T this[int index] { get => Data[index]; set => Data[index] = value; }

        public uint Length;

        public void Push(T srv)
        {
            fixed (T* ptr = new T[Length + 1])
            {
                Buffer.MemoryCopy(Data, ptr, sizeof(T) * (Length + 1), sizeof(T) * Length);
                Data = ptr;
                Length++;
            }
            Data[Length - 1] = srv;
        }

        public void Clear()
        {
            Data = null;
            Length = 0;
        }

        public void Pop()
        {
            fixed (T* ptr = new T[Length - 1])
            {
                var size = sizeof(T) * (Length - 1);
                Buffer.MemoryCopy(Data, ptr, size, size);
                Data = ptr;
                Length--;
            }
        }

        public void Resize(uint length)
        {
            fixed (T* ptr = new T[length])
            {
                var size = sizeof(T) * length;
                Buffer.MemoryCopy(Data, ptr, size, size);
                Data = ptr;
                Length = length;
            }
        }
    }
}