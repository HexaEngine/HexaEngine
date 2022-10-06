using System;
using System.Diagnostics;

namespace SPIRVCross
{
    /// <summary>
    /// A dispatchable handle.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct SpvcContext : IEquatable<SpvcContext>
    {
        public SpvcContext(nint handle)
        { Handle = handle; }

        public nint Handle { get; }
        public bool IsNull => Handle == 0;
        public static SpvcContext Null => new(0);

        public static implicit operator SpvcContext(nint handle) => new(handle);

        public static bool operator ==(SpvcContext left, SpvcContext right) => left.Handle == right.Handle;

        public static bool operator !=(SpvcContext left, SpvcContext right) => left.Handle != right.Handle;

        public static bool operator ==(SpvcContext left, nint right) => left.Handle == right;

        public static bool operator !=(SpvcContext left, nint right) => left.Handle != right;

        public bool Equals(SpvcContext other) => Handle == other.Handle;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpvcContext handle && Equals(handle);

        /// <inheritdoc/>
        public override int GetHashCode() => Handle.GetHashCode();

        private string DebuggerDisplay => string.Format("spvc_context [0x{0}]", Handle.ToString("X"));
    }

    /// <summary>
    /// A dispatchable handle.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct SpvcParsedIr : IEquatable<SpvcParsedIr>
    {
        public SpvcParsedIr(nint handle)
        { Handle = handle; }

        public nint Handle { get; }
        public bool IsNull => Handle == 0;
        public static SpvcParsedIr Null => new(0);

        public static implicit operator SpvcParsedIr(nint handle) => new(handle);

        public static bool operator ==(SpvcParsedIr left, SpvcParsedIr right) => left.Handle == right.Handle;

        public static bool operator !=(SpvcParsedIr left, SpvcParsedIr right) => left.Handle != right.Handle;

        public static bool operator ==(SpvcParsedIr left, nint right) => left.Handle == right;

        public static bool operator !=(SpvcParsedIr left, nint right) => left.Handle != right;

        public bool Equals(SpvcParsedIr other) => Handle == other.Handle;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpvcParsedIr handle && Equals(handle);

        /// <inheritdoc/>
        public override int GetHashCode() => Handle.GetHashCode();

        private string DebuggerDisplay => string.Format("spvc_parsed_ir [0x{0}]", Handle.ToString("X"));
    }

    /// <summary>
    /// A dispatchable handle.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct SpvcCompiler : IEquatable<SpvcCompiler>
    {
        public SpvcCompiler(nint handle)
        { Handle = handle; }

        public nint Handle { get; }
        public bool IsNull => Handle == 0;
        public static SpvcCompiler Null => new(0);

        public static implicit operator SpvcCompiler(nint handle) => new(handle);

        public static bool operator ==(SpvcCompiler left, SpvcCompiler right) => left.Handle == right.Handle;

        public static bool operator !=(SpvcCompiler left, SpvcCompiler right) => left.Handle != right.Handle;

        public static bool operator ==(SpvcCompiler left, nint right) => left.Handle == right;

        public static bool operator !=(SpvcCompiler left, nint right) => left.Handle != right;

        public bool Equals(SpvcCompiler other) => Handle == other.Handle;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpvcCompiler handle && Equals(handle);

        /// <inheritdoc/>
        public override int GetHashCode() => Handle.GetHashCode();

        private string DebuggerDisplay => string.Format("spvc_compiler [0x{0}]", Handle.ToString("X"));
    }

    /// <summary>
    /// A dispatchable handle.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct SpvcCompilerOptions : IEquatable<SpvcCompilerOptions>
    {
        public SpvcCompilerOptions(nint handle)
        { Handle = handle; }

        public nint Handle { get; }
        public bool IsNull => Handle == 0;
        public static SpvcCompilerOptions Null => new(0);

        public static implicit operator SpvcCompilerOptions(nint handle) => new(handle);

        public static bool operator ==(SpvcCompilerOptions left, SpvcCompilerOptions right) => left.Handle == right.Handle;

        public static bool operator !=(SpvcCompilerOptions left, SpvcCompilerOptions right) => left.Handle != right.Handle;

        public static bool operator ==(SpvcCompilerOptions left, nint right) => left.Handle == right;

        public static bool operator !=(SpvcCompilerOptions left, nint right) => left.Handle != right;

        public bool Equals(SpvcCompilerOptions other) => Handle == other.Handle;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpvcCompilerOptions handle && Equals(handle);

        /// <inheritdoc/>
        public override int GetHashCode() => Handle.GetHashCode();

        private string DebuggerDisplay => string.Format("spvc_compiler_options [0x{0}]", Handle.ToString("X"));
    }

    /// <summary>
    /// A dispatchable handle.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct SpvcResources : IEquatable<SpvcResources>
    {
        public SpvcResources(nint handle)
        { Handle = handle; }

        public nint Handle { get; }
        public bool IsNull => Handle == 0;
        public static SpvcResources Null => new(0);

        public static implicit operator SpvcResources(nint handle) => new(handle);

        public static bool operator ==(SpvcResources left, SpvcResources right) => left.Handle == right.Handle;

        public static bool operator !=(SpvcResources left, SpvcResources right) => left.Handle != right.Handle;

        public static bool operator ==(SpvcResources left, nint right) => left.Handle == right;

        public static bool operator !=(SpvcResources left, nint right) => left.Handle != right;

        public bool Equals(SpvcResources other) => Handle == other.Handle;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpvcResources handle && Equals(handle);

        /// <inheritdoc/>
        public override int GetHashCode() => Handle.GetHashCode();

        private string DebuggerDisplay => string.Format("spvc_resources [0x{0}]", Handle.ToString("X"));
    }

    /// <summary>
    /// A dispatchable handle.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct SpvcType : IEquatable<SpvcType>
    {
        public SpvcType(nint handle)
        { Handle = handle; }

        public nint Handle { get; }
        public bool IsNull => Handle == 0;
        public static SpvcType Null => new(0);

        public static implicit operator SpvcType(nint handle) => new(handle);

        public static bool operator ==(SpvcType left, SpvcType right) => left.Handle == right.Handle;

        public static bool operator !=(SpvcType left, SpvcType right) => left.Handle != right.Handle;

        public static bool operator ==(SpvcType left, nint right) => left.Handle == right;

        public static bool operator !=(SpvcType left, nint right) => left.Handle != right;

        public bool Equals(SpvcType other) => Handle == other.Handle;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpvcType handle && Equals(handle);

        /// <inheritdoc/>
        public override int GetHashCode() => Handle.GetHashCode();

        private string DebuggerDisplay => string.Format("spvc_type [0x{0}]", Handle.ToString("X"));
    }

    /// <summary>
    /// A dispatchable handle.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct SpvcConstant : IEquatable<SpvcConstant>
    {
        public SpvcConstant(nint handle)
        { Handle = handle; }

        public nint Handle { get; }
        public bool IsNull => Handle == 0;
        public static SpvcConstant Null => new(0);

        public static implicit operator SpvcConstant(nint handle) => new(handle);

        public static bool operator ==(SpvcConstant left, SpvcConstant right) => left.Handle == right.Handle;

        public static bool operator !=(SpvcConstant left, SpvcConstant right) => left.Handle != right.Handle;

        public static bool operator ==(SpvcConstant left, nint right) => left.Handle == right;

        public static bool operator !=(SpvcConstant left, nint right) => left.Handle != right;

        public bool Equals(SpvcConstant other) => Handle == other.Handle;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpvcConstant handle && Equals(handle);

        /// <inheritdoc/>
        public override int GetHashCode() => Handle.GetHashCode();

        private string DebuggerDisplay => string.Format("spvc_constant [0x{0}]", Handle.ToString("X"));
    }

    /// <summary>
    /// A dispatchable handle.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct SpvcSet : IEquatable<SpvcSet>
    {
        public SpvcSet(nint handle)
        { Handle = handle; }

        public nint Handle { get; }
        public bool IsNull => Handle == 0;
        public static SpvcSet Null => new(0);

        public static implicit operator SpvcSet(nint handle) => new(handle);

        public static bool operator ==(SpvcSet left, SpvcSet right) => left.Handle == right.Handle;

        public static bool operator !=(SpvcSet left, SpvcSet right) => left.Handle != right.Handle;

        public static bool operator ==(SpvcSet left, nint right) => left.Handle == right;

        public static bool operator !=(SpvcSet left, nint right) => left.Handle != right;

        public bool Equals(SpvcSet other) => Handle == other.Handle;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpvcSet handle && Equals(handle);

        /// <inheritdoc/>
        public override int GetHashCode() => Handle.GetHashCode();

        private string DebuggerDisplay => string.Format("spvc_set [0x{0}]", Handle.ToString("X"));
    }

    /// <summary>
    /// A dispatchable handle.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly partial struct SpvcErrorCallback : IEquatable<SpvcErrorCallback>
    {
        public SpvcErrorCallback(nint handle)
        { Handle = handle; }

        public nint Handle { get; }
        public bool IsNull => Handle == 0;
        public static SpvcErrorCallback Null => new(0);

        public static implicit operator SpvcErrorCallback(nint handle) => new(handle);

        public static bool operator ==(SpvcErrorCallback left, SpvcErrorCallback right) => left.Handle == right.Handle;

        public static bool operator !=(SpvcErrorCallback left, SpvcErrorCallback right) => left.Handle != right.Handle;

        public static bool operator ==(SpvcErrorCallback left, nint right) => left.Handle == right;

        public static bool operator !=(SpvcErrorCallback left, nint right) => left.Handle != right;

        public bool Equals(SpvcErrorCallback other) => Handle == other.Handle;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is SpvcErrorCallback handle && Equals(handle);

        /// <inheritdoc/>
        public override int GetHashCode() => Handle.GetHashCode();

        private string DebuggerDisplay => string.Format("spvc_error_callback [0x{0}]", Handle.ToString("X"));
    }
}