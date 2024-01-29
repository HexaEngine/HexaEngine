# Looking for help, if anyone wanna help me out feel free to text me.

# HexaEngine
<p align="center">
  <img width="300" height="300" src="https://raw.githubusercontent.com/JunaMeinhold/HexaEngine/master/icon.png">
</p>

# Build
- .NET SDK 8.0
- CMake<br/>
Native libs source can be found under https://github.com/JunaMeinhold/HexaEngine.Native

# Features
- BRDF (Roughness, Metallic, Reflectance, Ao workflow)
- IBL
- Shadow Mapping (PSM, OSM, CSM) (note: OSM and PSM uses an shadow atlas)
- Clustered Forward and Clustered Deferred (Hybrid renderer)
- Many post processing effects (DoF, SSR, Tonemapping, LUT, Bloom, Motion blur)
- Ambient Occlusion (SSAO, HBAO+, GTAO and ASSAO are comming soon)
- Level-Editor (still in work, but it's usable)
- Physics Engine (multiple usable collider types are already implemented)
- Plugin System
- Audio System (Wav-file support)
- Culling (Frustum and Occlusion) (doesn't work/integrated properly)

# (WIP) Material editor + Shader generator
![image](https://github.com/JunaMeinhold/HexaEngine/assets/46632782/8a3acc3d-3fad-4083-88fd-3613ffd6b30f)

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
