# HexaEngine
<p align="center">
  <img width="300" height="300" src="https://raw.githubusercontent.com/JunaMeinhold/HexaEngine/master/icon.png">
</p>

# Build
- .NET SDK 7.0
- CMake<br/>
Native libs source can be found under https://github.com/JunaMeinhold/HexaEngine.Native

# Features
- Principled BSDF
- IBL
- Shadow Mapping (PSM, CSM, OSM)
- Clustered Forward and Clustered Deferred (Hybrid renderer)
- Many post processing effects (DoF, SSR, Tonemapping, LUT, Bloom, Motion blur)
- Level-Editor
- Physics Engine
- Plugin System
- Audio System (Wav-file support)
- Culling (Frustum and Occlusion)


# Backends
| API     | Supported          |
| ------- | ------------------ |
| D3D11   | :white_check_mark: |
| D3D12   | :ballot_box_with_check: (wip) |
| Vulkan  | :ballot_box_with_check: (wip) |
| OpenGL  | :ballot_box_with_check: (wip) |

# Credits
https://github.com/ocornut/imgui  
https://github.com/CedricGuillemet/ImGuizmo  
https://github.com/Nelarius/imnodes  
https://github.com/epezent/implot  
https://github.com/dotnet/Silk.NET  
https://github.com/bepu/bepuphysics2  
https://github.com/MiloszKrajewski/K4os.Compression.LZ4  
https://www.newtonsoft.com/json  
https://github.com/mellinoe/ImGui.NET *modified*  
https://github.com/FaberSanZ/SPIRV-Cross.NET *modified*  
