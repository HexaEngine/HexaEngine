using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SPIRVCross
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public readonly partial struct SpvId : IEquatable<SpvId>
    {
        public SpvId(uint value) => this.Value = value;

        public readonly uint Value;

        public bool Equals(SpvId other) => Value.Equals(other.Value);

        public override bool Equals(object? obj) => obj is SpvId other && Equals(other);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value.ToString();

        public static implicit operator uint(SpvId from) => from.Value;

        public static implicit operator SpvId(uint from) => new(from);

        public static bool operator ==(SpvId left, SpvId right) => left.Equals(right);

        public static bool operator !=(SpvId left, SpvId right) => !left.Equals(right);
    }
}