namespace HexaEngine.Core.Graphics.Shaders.Reflection
{
    public unsafe struct TypeInfo
    {
        public uint Id;
        public StdString TypeName;
        public StdString StructMemberName;
        public TypeFlags TypeFlags;
        public DecorationFlags DecorationFlags;
        public TypeTraits Traits;
        public TypeInfo* StructTypeDescription;
        public uint MemberCount;
        public TypeInfo* Members;

        public void Release()
        {
            TypeName.Release();
            StructMemberName.Release();

            if (StructTypeDescription != null)
            {
                StructTypeDescription->Release();

                Free(StructTypeDescription);
                StructTypeDescription = null;
            }

            if (Members != null)
            {
                for (int i = 0; i < MemberCount; i++)
                {
                    Members[i].Release();
                }

                Free(Members);
                Members = null;
                MemberCount = 0;
            }
        }
    }
}