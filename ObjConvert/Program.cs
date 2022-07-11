// See https://aka.ms/new-console-template for more information

using HexaEngine.IO.ObjLoader.Loaders;
using HexaEngine.Meshes;
using HexaEngine.Objects;

ObjLoaderFactory objLoaderFactory = new();
var loader = objLoaderFactory.Create();

var fs = File.OpenRead(args[0]);
var result = loader.Load(fs);
fs.Close();

(MeshFile mesh, MeshMaterialLibrary library) = result.Convert();
MeshFactory factory = MeshFactory.Instance;

var fsOut = File.Create("output.msh");
var fsmOut = File.Create("output.mlb");
factory.Save(mesh, fsOut);
fsOut.Flush();
fsOut.Close();
factory.Save(library, fsmOut);
fsmOut.Flush();
fsmOut.Close();

factory.LoadLib("output.mlb");
MeshFile mesh1 = factory.Load("output.msh");

for (int i = 0; i < mesh.Vertices.Length; i++)
{
    if (mesh.Vertices[i] != mesh1.Vertices[i])
        throw new Exception();
}

for (int i = 0; i < mesh.Groups.Length; i++)
{
    for (int j = 0; j < mesh.Groups[i].Faces.Length; j++)
    {
        if (mesh.Groups[i].Faces[j] != mesh1.Groups[i].Faces[j])
            throw new Exception();
    }
}

for (int i = 0; i < mesh.Groups.Length; i++)
{
    for (int j = 0; j < mesh.Groups[i].Indices.Length; j++)
    {
        if (mesh.Groups[i].Indices[j] != mesh1.Groups[i].Indices[j])
            throw new Exception();
    }
}