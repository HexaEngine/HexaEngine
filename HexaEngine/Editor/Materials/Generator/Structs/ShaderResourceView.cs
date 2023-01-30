﻿namespace HexaEngine.Editor.Materials.Generator.Structs
{
    using System.Globalization;
    using System.Text;

    public struct ShaderResourceView
    {
        public int Slot;
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

        public void Build(StringBuilder builder)
        {
            if (Samples > 0)
            {
                builder.AppendLine($"{SrvType.GetTypeName()}<{Type.GetTypeName()},{Samples.ToString(CultureInfo.InvariantCulture)}> {Name} : register(t{Slot.ToString(CultureInfo.InvariantCulture)});");
            }
            else
            {
                builder.AppendLine($"{SrvType.GetTypeName()}<{Type.GetTypeName()}> {Name} : register(t{Slot.ToString(CultureInfo.InvariantCulture)});");
            }
        }
    }
}