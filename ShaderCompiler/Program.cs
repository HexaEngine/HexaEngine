// See https://aka.ms/new-console-template for more information
using ShaderCompiler;

Compiler.Compile(args[0], "main", args[1], out var blob);

var name = Path.GetFileNameWithoutExtension(args[0]) + ".shd";

File.WriteAllBytes(name, blob.AsBytes());