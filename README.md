# HexaEngine

<p align="center">
  <img width="300" height="300" src="https://raw.githubusercontent.com/JunaMeinhold/HexaEngine/master/icon.png">
</p>

A work in progress Game Engine written in C#, aiming for high performance and cross-platform compatibility (currently only Windows). It incorporates SIMD optimizations and multithreading throughout. The engine adopts an ECS-like approach but with unique twists to enhance flexibility and performance.

## Community
- Discord: [https://discord.gg/VawN5d8HMh](https://discord.gg/VawN5d8HMh)

## Build
- .NET SDK 8.0

### Platforms
| OS       | Supported          |
| -------- | ------------------ |
| Windows (10 & 11)  | ✅ |
| Linux    | 🚧 |
| Android  | 🚧 |
| macOS    | 🚧 |

### Graphics Backends
| API      | Supported          |
| -------- | ------------------ |
| D3D11    | ✅ |
| D3D12    | 🚧 |
| Vulkan   | 🚧 |
| OpenGL   | 🚧 |

### Audio Backends
| API          | Supported          |
| ------------ | ------------------ |
| OpenAL Soft  | ✅ |
| XAudio2      | 🚧 |

# Features

## Integrated Level Editor
- Project Management
- Package Manager
- Plugin System
- Advanced and flexible asset import pipeline supporting async
- Git Integration
- Performance Profiler (WIP)
- Integrated text editor
- Integrated image editor
- Node-based material editor
- Intuitive terrain editor with procedural generation
- And much more...

## Physics (PhysX 5.3.1)
- Colliders:
  - Box
  - Capsule
  - Convex Mesh
  - Mesh
  - Plane
  - Sphere
  - Terrain
- Rigid Bodies (Static, Dynamic, Kinematic)
- Character Controllers:
  - Capsule
  - Box
- Joints:
  - Ball
  - D6
  - Distance
  - Fixed
  - Hinge
  - Slider

## Engine Architecture
- ECS-like Scene Architecture with support for data-driven and non-data-driven execution
- Multi-threaded scene updates (Rendering and Scene ticks run in parallel for maximum performance)
- Thread-Safe Resource Management via Factories with automatic cleanup
- Abstraction layer for various Graphics and Audio APIs like D3D11, D3D12, Vulkan, OpenALSoft, and XAudio2 + X3DAudio
- Asynchronous loading for meshes and more
- Thread-safe caches with lazy disk writing

## Scripting with C#
- Unity-like Coroutines with reusable coroutines
- Job-System with reusable jobs
- Multithreaded script execution (coming soon)
- Order of execution via DAG and Priorities (coming soon)
- Scene-independent Global Scripts (coming soon)
- Scene Queries for fast accessing a specific GameObject or Component depending on user-defined filters via events
- Easy integration of editor variables using the [EditorProperty] Attribute
- Unity-like Input System (offers maximum flexibility)

## Rendering
- Physically based Rendering (Roughness, Metallic, Reflectance, AO Workflow)
- Skinned Mesh Rendering
- Multi-Layered Terrain rendering with optional GPU Tessellation
- Level of detail:
  - LOD Generator (Fast Quadric Mesh Simplification)
- GPU Particle Systems:
  - Render over thousands particles without performance impact
- Lighting:
  - Ambient Lights
  - Directional Lights
  - Point Lights
  - Spotlights
  - Global Illumination over IBL importance sampling, SSGI, and Voxel Global Illumination (coming soon)
- Skybox Models:
  - Environment Texture
  - Hosek Wilkie
  - Preetham
  - Custom Sky
- Shadow Mapping:
  - Perspective Shadow Mapping (PSM)
  - Omni Directional Shadow Mapping (OSM)
  - Dual Parabolic Shadow Mapping (DPSM)
  - Cascaded Shadow Mapping (CSM)
  - A Shadow Atlas for OSM DPSM and PSM
  - Percent Closer Filter, Variance Shadow Mapping, and more
- Hybrid renderer (Clustered Forward and Clustered Deferred)
- Many post-processing effects (including DoF, SSR, Tonemapping, LUT, Bloom, Motion blur)
- Ambient Occlusion (SSAO, HBAO+, GTAO, and ASSAO are coming soon)
- Frustum and Occlusion Culling (GPU based)

## UI System (WIP)
- XAML Support
- Routed events
- Automatic layouting
- Controls:
  - Button, TextBox, Image, Grid, StackPanel

## Audio System
- Playing Wav files
- 3D-Spatial audio processing
- Listeners
- Emitters

## Screenshots
- Comming soon

# Credits
- [imgui](https://github.com/ocornut/imgui)
- [ImGuizmo](https://github.com/CedricGuillemet/ImGuizmo)
- [imnodes](https://github.com/Nelarius/imnodes)
- [implot](https://github.com/epezent/implot)
- [Silk.NET](https://github.com/dotnet/Silk.NET)
- [K4os.Compression.LZ4](https://github.com/MiloszKrajewski/K4os.Compression.LZ4)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [libgit2sharp](https://github.com/libgit2/libgit2sharp/)
- [Hardware.Info](https://github.com/Jinjinov/Hardware.Info)
- [MagicPhysX](https://github.com/Cysharp/MagicPhysX)
- [octokit.net](https://github.com/octokit/octokit.net)
- [YamlDotNet](https://github.com/aaubry/YamlDotNet)
- [commandline](https://github.com/commandlineparser/commandline)
