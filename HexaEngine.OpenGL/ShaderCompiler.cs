namespace HexaEngine.OpenGL
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Shaders;
    using HexaEngine.Core.IO;
    using Silk.NET.OpenGL;
    using System.Buffers.Binary;
    using System.Diagnostics;
    using System.IO;
    using static System.Runtime.InteropServices.JavaScript.JSType;
    using Shader = Core.Graphics.Shader;

    public unsafe class ShaderCompiler
    {
        private readonly GL gl;

        public ShaderCompiler(GL gl)
        {
            this.gl = gl;
        }

        public bool Compile(string source, ShaderMacro[] macros, string entryPoint, string sourceName, ShaderType type, out uint shader, out string? error)
        {
            Debug.WriteLine($"Compiling: {sourceName}");
            error = null;
            shader = 0;
            var lang = CrossCompiler.GetSourceLanguage(sourceName);
            if (lang != SourceLanguage.GLSL)
            {
                if (!CrossCompiler.CompileSPIRVFromSource(source, sourceName, entryPoint, macros, ShaderKind.ComputeShader, lang, out var il, out error))
                {
                    error = $"Error: {sourceName}";
                    return false;
                }
                if (!CrossCompiler.CompileSPIRVTo(il, SourceLanguage.GLSL, out source))
                {
                    error = $"Error: {sourceName}";
                    return false;
                }
            }

            shader = gl.CreateShader(type);
            gl.ShaderSource(shader, source);
            gl.CompileShader(shader);

            //Checking the shader for compilation errors.
            string infoLog = gl.GetShaderInfoLog(shader);
            if (!string.IsNullOrEmpty(infoLog))
            {
                error = $"Error: {sourceName}";
                gl.DeleteShader(shader); // Don't leak the shader.
                return false;
            }

            Debug.WriteLine($"Done: {sourceName}");

            return true;
        }

        public bool CompileProgram(ComputePipelineDesc desc, ShaderMacro[] macros, out uint program, out string? error)
        {
            program = 0;
            if (!Compile(FileSystem.ReadAllText(Paths.CurrentShaderPath + desc.Path), macros, desc.Entry, desc.Path, ShaderType.ComputeShader, out var shader, out error))
            {
                return false;
            }

            program = gl.CreateProgram();
            gl.AttachShader(program, shader);
            gl.LinkProgram(program);

            //Checking the linking for errors.
            gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                error = $"Error linking shader {gl.GetProgramInfoLog(program)}";
                gl.DeleteProgram(program);
                return false;
            }

            gl.DetachShader(program, shader);
            gl.DeleteShader(shader);

            return true;
        }

        public Shader* GetProgramBinary(uint program)
        {
            var length = gl.GetProgram(program, GLEnum.ProgramBinaryLength);
            var buffer = (byte*)Alloc((nint)(length + 4));
            uint written;
            GLEnum format;
            gl.GetProgramBinary(program, (uint)length, &written, &format, buffer + 4);

            var shader = AllocT<Shader>();
            shader->Bytecode = buffer;
            shader->Length = (nuint)(length + 4);
            var span = shader->AsSpan();
            BinaryPrimitives.WriteInt32LittleEndian(span, (int)format);

            return shader;
        }

        public unsafe void GetProgramOrCompile(ComputePipelineDesc desc, ShaderMacro[] macros, out uint program, bool bypassCache = false)
        {
            Shader* pShader;
            if (bypassCache || !ShaderCache.GetShader(desc.Path, SourceLanguage.GLSL, macros, &pShader, out _))
            {
                CompileProgram(desc, macros, out program, out _);
                if (program == 0)
                {
                    return;
                }

                pShader = GetProgramBinary(program);

                ShaderCache.CacheShader(desc.Path, SourceLanguage.GLSL, macros, Array.Empty<InputElementDescription>(), pShader);
            }

            program = gl.CreateProgram();
            var span = pShader->AsSpan();
            GLEnum format = (GLEnum)BinaryPrimitives.ReadInt32LittleEndian(span);
            gl.ProgramBinary(program, format, pShader->Bytecode + 4, (uint)(pShader->Length - 4));
            gl.LinkProgram(program);
        }
    }
}