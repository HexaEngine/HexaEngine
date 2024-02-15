﻿namespace HexaEngine.Editor.MaterialEditor.Generator.Structs
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using System.Globalization;

    public struct ShaderResourceView
    {
        public uint Slot;
        public string Name;
        public SType SrvType;
        public SType Type;
        public int Samples;

        public ShaderResourceView(string name, SType srvType, SType type, int samples = -1)
        {
            Name = name;
            SrvType = srvType;
            Type = type;
            Samples = samples;
        }

        public void Build(CodeWriter builder)
        {
            if (Samples > 0)
            {
                builder.WriteLine($"{SrvType.GetTypeName()}<{Type.GetTypeName()},{Samples.ToString(CultureInfo.InvariantCulture)}> {Name} : register(t{Slot.ToString(CultureInfo.InvariantCulture)});");
            }
            else
            {
                builder.WriteLine($"{SrvType.GetTypeName()}<{Type.GetTypeName()}> {Name} : register(t{Slot.ToString(CultureInfo.InvariantCulture)});");
            }
        }
    }
}