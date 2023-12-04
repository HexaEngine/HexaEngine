using HexaEngine.Core;

namespace HexaEngine.Lights
{
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    public unsafe class SpatialAllocator
    {
        private Space* rootSpace;

        public SpatialAllocator(Vector2 size, int subDivisions)
        {
            Vector2 targetSize = size;
            for (int i = 0; i < subDivisions; i++)
            {
                targetSize /= 2;
            }

            rootSpace = AllocT<Space>();
            rootSpace->Size = size;
            rootSpace->Offset = default;
            rootSpace->InitSubSpaces(rootSpace, targetSize);
        }

        internal unsafe struct Space
        {
            public Vector2 Size;
            public Vector2 Offset;
            public Space* Parent;
            public Space* SubSpaces;
            public byte Flag;

            public bool IsUsed
            {
                readonly get => (Flag & 0x1) != 0;
                set
                {
                    if (value)
                    {
                        Flag |= 0x1;
                    }
                    else
                    {
                        Flag = (byte)(Flag & ~0x1);
                    }
                }
            }

            public bool IsPartiallyUsed
            {
                readonly get => (Flag & 0x2) != 0;
                set
                {
                    if (value)
                    {
                        Flag |= 0x2;
                    }
                    else
                    {
                        Flag = (byte)(Flag & ~0x2);
                    }
                }
            }

            public const int SubSpaceCount = 4;
            public const int SubSpaceWidth = 2;

            public void InitSubSpaces(Space* self, Vector2 endSize)
            {
                if (Size == endSize)
                    return;

                SubSpaces = AllocT<Space>(SubSpaceCount);
                ZeroMemoryT(SubSpaces, SubSpaceCount);

                Vector2 halfSize = Size / 2;
                for (int i = 0; i < SubSpaceCount; i++)
                {
                    int x = i / SubSpaceWidth;
                    int y = i % SubSpaceWidth;
                    Space* subSpace = &SubSpaces[i];
                    subSpace->Parent = self;
                    subSpace->Size = halfSize;
                    subSpace->Offset = Offset + new Vector2(x, y) * halfSize;
                    subSpace->InitSubSpaces(subSpace, endSize);
                }
            }

            public void Clear()
            {
                Flag = 0;
                if (SubSpaces != null)
                {
                    for (int i = 0; i < SubSpaceCount; i++)
                    {
                        Space* subSpace = &SubSpaces[i];
                        subSpace->Clear();
                    }
                }
            }

            public void Release()
            {
                if (SubSpaces != null)
                {
                    for (int i = 0; i < SubSpaceCount; i++)
                    {
                        Space* subSpace = &SubSpaces[i];
                        subSpace->Release();
                    }
                }

                Utils.Free(SubSpaces);
                SubSpaces = null;
            }

            public override readonly string ToString()
            {
                return $"Offset: {Offset}, Size: {Size}";
            }
        }

        public bool TryAlloc(Vector2 size, [NotNullWhen(true)] out SpatialAllocatorHandle? handle)
        {
            Space* space;
            bool result = TryAlloc(size, rootSpace, &space);
            if (!result)
            {
                handle = null;
                return false;
            }
            handle = new(this, space);
            return true;
        }

        internal bool TryAlloc(Vector2 size, Space* space, Space** outSpace)
        {
            *outSpace = null;
            if (size == space->Size)
            {
                // we cant use a used or partially used space.
                if (space->IsUsed || space->IsPartiallyUsed)
                {
                    return false;
                }

                // mark space as used.
                space->IsUsed = true;
                *outSpace = space;
                return true;
            }
            else
            {
                // we reached the end of the tree there is nothing left here.
                if (space->SubSpaces == null)
                {
                    return false;
                }

                for (int i = 0; i < Space.SubSpaceCount; i++)
                {
                    Space* subSpace = &space->SubSpaces[i];

                    if (subSpace->IsUsed)
                    {
                        continue;
                    }

                    // traverse down the tree.
                    if (TryAlloc(size, subSpace, outSpace))
                    {
                        // mark space as partially used.
                        space->IsPartiallyUsed = true;
                        return true;
                    }
                }

                return false;
            }
        }

        public SpatialAllocatorHandle? Alloc(Vector2 size)
        {
            Space* space = Alloc(size, rootSpace);
            if (space == null)
            {
                return null;
            }

            return new(this, space);
        }

        internal Space* AllocInternal(Vector2 size)
        {
            return Alloc(size, rootSpace);
        }

        private Space* Alloc(Vector2 size, Space* space)
        {
            if (size == space->Size)
            {
                // we can't use a used or partially used space.
                if (space->IsUsed || space->IsPartiallyUsed)
                {
                    return null;
                }

                // mark space as used.
                space->IsUsed = true;
                return space;
            }
            else
            {
                // we reached the end of the tree there is nothing left here.
                if (space->SubSpaces == null)
                {
                    return null;
                }

                for (int i = 0; i < Space.SubSpaceCount; i++)
                {
                    Space* subSpace = &space->SubSpaces[i];

                    if (subSpace->IsUsed)
                    {
                        continue;
                    }

                    // traverse down the tree.
                    var allocatedSpace = Alloc(size, subSpace);
                    if (allocatedSpace != null)
                    {
                        // mark space as partially used.
                        space->IsPartiallyUsed = true;
                        return allocatedSpace;
                    }
                }

                return null;
            }
        }

        internal void Free(Space* space)
        {
            if (space == null)
            {
                return;
            }

            if (!space->IsUsed)
            {
                return;
            }

            // mark space as unused.
            space->IsUsed = false;
            var parent = space->Parent;

            // clean up partially used.
            while (parent != null)
            {
                bool isUsed = false;
                for (int i = 0; i < Space.SubSpaceCount; i++)
                {
                    Space* subSpace = &parent->SubSpaces[i];

                    if (subSpace->IsUsed || subSpace->IsPartiallyUsed)
                    {
                        isUsed = true;
                        // we can skip the checks here because everything above will be partially used anyway.
                        break;
                    }
                }

                // we can skip the checks here because everything above will be partially used anyway.
                if (isUsed)
                {
                    break;
                }

                parent->IsPartiallyUsed = false;
                parent = parent->Parent;
            }
        }

        public void Clear()
        {
            rootSpace->Clear();
        }

        public void Dispose()
        {
            rootSpace->Release();
            Utils.Free(rootSpace);
            rootSpace = null;
        }
    }
}