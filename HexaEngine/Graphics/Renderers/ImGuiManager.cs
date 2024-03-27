namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGuizmo;
    using Hexa.NET.ImNodes;
    using Hexa.NET.ImPlot;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Windows;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class ImGuiManager
    {
        private ImGuiContextPtr guiContext;
        private ImNodesContextPtr nodesContext;
        private ImPlotContextPtr plotContext;

        private const string defaultIniConfig = @"
[Window][DockSpaceViewport_11111111]
Pos=0,17
Size=2560,1400
Collapsed=0

[Window][Debug##Default]
Pos=956,296
Size=384,132
Collapsed=0

[Window][Project]
Pos=223,17
Size=1241,802
Collapsed=0
DockId=0x0000001A,5

[Window][Assets]
Pos=0,813
Size=273,604
Collapsed=0
DockId=0x00000016,0

[Window][Layout]
Pos=0,17
Size=288,350
Collapsed=0
DockId=0x00000007,0

[Window][Properties]
Pos=2074,17
Size=486,359
Collapsed=0
DockId=0x00000019,0

[Window][Materials]
Pos=0,804
Size=1920,253
Collapsed=0

[Window][Console]
Pos=275,813
Size=2285,604
Collapsed=0
DockId=0x00000017,0

[Window][Framebuffer]
Pos=192,17
Size=812,503
Collapsed=0
DockId=0x0000001A,0

[Window][Open Project]
Pos=1009,643
Size=542,107
Collapsed=0

[Window][Preferences]
Pos=468,368
Size=967,633
Collapsed=0

[Window][File picker]
Pos=248,469
Size=936,521
Collapsed=0

[Window][Pipelines]
Pos=372,411
Size=669,402
Collapsed=0
DockId=0x00000011,0

[Window][Scene Variables]
Pos=447,635
Size=729,399
Collapsed=0

[Window][Rename file]
Pos=404,607
Size=553,73
Collapsed=0

[Window][Import Scene]
Pos=306,218
Size=868,539
Collapsed=0

[Window][Debug]
Pos=352,402
Size=383,514
Collapsed=0

[Window][Renderer]
Pos=2074,17
Size=486,792
Collapsed=0
DockId=0x00000019,1

[Window][Mixer]
Pos=606,308
Size=457,364
Collapsed=0

[Window][Pipeline editor]
Pos=250,17
Size=1370,793
Collapsed=0
DockId=0x0000001A,2

[Window][ImPlot Demo]
Pos=298,17
Size=1254,793
Collapsed=0
DockId=0x0000001A,1

[Window][SceneProfiler]
Pos=805,564
Size=1167,849
Collapsed=0

[Window][Warning]
Pos=574,289
Size=40,50
Collapsed=0

[Window][Effects]
Pos=170,17
Size=1571,837
Collapsed=0
DockId=0x0000001A,1

[Window][Create]
Pos=671,137
Size=286,753
Collapsed=0

[Window][Publish]
Pos=403,188
Size=483,448
Collapsed=0

[Window][Mat Editor]
Pos=170,17
Size=1571,837
Collapsed=0
DockId=0x0000001A,2

[Window][Material Editor]
Pos=275,813
Size=2285,604
Collapsed=0
DockId=0x00000017,1

[Window][Meshes]
Pos=197,17
Size=1323,754
Collapsed=0
DockId=0x0000001A,2

[Window][Post Process]
Pos=213,486
Size=1099,702
Collapsed=0

[Window][Scene]
Pos=290,17
Size=1782,794
Collapsed=0
DockId=0x0000001A,0

[Window][Texture Painter]
Pos=192,17
Size=1386,838
Collapsed=0
DockId=0x0000001A,3

[Window][Tools::TexturePainter]
Pos=123,345
Size=294,209
Collapsed=0

[Window][Tools]
Pos=0,17
Size=190,838
Collapsed=0
DockId=0x0000001A,1

[Window][Model Editor]
Pos=167,17
Size=1276,814
Collapsed=0
DockId=0x0000001A,2

[Window][Color]
Pos=1580,17
Size=340,337
Collapsed=0
DockId=0x00000004,1

[Window][Image Properties]
Pos=940,220
Size=340,298
Collapsed=0
DockId=0x00000019,1

[Window][Colorpicker]
Pos=1580,17
Size=340,337
Collapsed=0
DockId=0x00000004,1

[Window][Toolbox]
Pos=0,17
Size=288,350
Collapsed=0
DockId=0x00000007,1

[Window][Brushes]
Pos=0,524
Size=190,331
Collapsed=0
DockId=0x0000001A,0

[Window][Color-picker]
Pos=2074,17
Size=486,359
Collapsed=0
DockId=0x00000019,1

[Window][Image Painter]
Pos=290,17
Size=1782,794
Collapsed=0
DockId=0x0000001A,3

[Window][Tool properties]
Pos=0,369
Size=288,442
Collapsed=0
DockId=0x00000008,0

[Window][Image properties]
Pos=2074,378
Size=486,194
Collapsed=0
DockId=0x00000013,0

[Window][Masks]
Pos=2074,574
Size=486,237
Collapsed=0
DockId=0x0000000C,0

[Window][Export Image]
Pos=341,167
Size=971,513
Collapsed=0

[Window][Input Window]
Pos=327,218
Size=1241,802
Collapsed=0

[Window][IBL Prefilter]
Pos=-1643,300
Size=321,119
Collapsed=0

[Window][Pre-Filter]
Pos=257,126
Size=891,417
Collapsed=0

[Window][Bake Irradiance]
Pos=340,388
Size=321,96
Collapsed=0

[Window][Create new]
Pos=518,257
Size=293,165
Collapsed=0

[Window][Convert to cube]
Pos=831,480
Size=258,73
Collapsed=0

[Window][Generate MipMaps]
Pos=890,503
Size=140,50
Collapsed=0

[Window][Convert Format]
Pos=824,492
Size=272,73
Collapsed=0

[Window][Resize]
Pos=824,480
Size=272,96
Collapsed=0

[Window][Mesh Editor]
Pos=290,17
Size=1795,924
Collapsed=0
DockId=0x0000001A,1

[Window][Import Model]
Pos=416,432
Size=595,383
Collapsed=0

[Window][Model Nodes]
Pos=0,-23
Size=32,33
Collapsed=0
DockId=0x00000019,2

[Window][Node Properties]
Pos=0,-23
Size=32,33
Collapsed=0
DockId=0x0000000E,0

[Window][Model Bones]
Pos=0,-23
Size=32,33
Collapsed=0
DockId=0x00000019,1

[Window][Pose Editor]
Pos=290,17
Size=1782,794
Collapsed=0
DockId=0x0000001A,2

[Window][Sequencer]
Pos=0,1216
Size=2560,201
Collapsed=0
DockId=0x0000001B,0

[Window][Assets 2]
Pos=182,542
Size=508,302
Collapsed=0

[Window][Heatmap]
Pos=1243,701
Size=517,419
Collapsed=1

[Window][Weather]
Pos=1937,552
Size=426,410
Collapsed=0

[Window][Dear ImGui Demo]
Pos=485,254
Size=903,752
Collapsed=0

[Window][Reset Settings?]
Pos=60,60
Size=16,33
Collapsed=0

[Window][Render Graph]
ViewportPos=-1928,-9
ViewportId=0xAE416BC3
Size=1927,1086
Collapsed=0

[Window][Add node]
Pos=0,17
Size=221,806
Collapsed=0
DockId=0x00000007,1

[Window][Preview]
Pos=1969,17
Size=591,561
Collapsed=0
DockId=0x00000019,1

[Window][Memory usage]
Pos=444,191
Size=1140,697
Collapsed=0

[Window][Dbg]
Pos=216,721
Size=768,265
Collapsed=0

[Window][PostFx]
Pos=180,772
Size=352,227
Collapsed=0
DockId=0x00000010,0

[Window][Crash report]
Size=700,400
Collapsed=0

[Window][Material Library]
Pos=2087,482
Size=473,193
Collapsed=0
DockId=0x0000000A,0

[Window][Model GameObjects]
Pos=608,535
Size=282,345
Collapsed=0

[Window][Generate BRDF LUT for IBL]
Pos=584,377
Size=286,142
Collapsed=0

[Window][Assets Old]
Pos=343,333
Size=170,235
Collapsed=0

[Window][Delete file]
Pos=752,484
Size=409,88
Collapsed=0

[Window][Assets (Old)]
Pos=766,281
Size=439,380
Collapsed=0

[Window][Git]
Pos=1466,17
Size=454,374
Collapsed=0
DockId=0x00000001,0

[Window][Rename folder]
Pos=481,699
Size=553,73
Collapsed=0

[Window][Delete directory]
Pos=703,484
Size=514,88
Collapsed=0

[Window][Checkout Branch]
Pos=794,484
Size=332,88
Collapsed=0

[Window][Delete Branch]
Pos=801,491
Size=318,75
Collapsed=0

[Window][Text Editor]
Pos=290,17
Size=1782,794
Collapsed=0
DockId=0x0000001A,1

[Window][Lens Editor]
Pos=290,17
Size=1711,1104
Collapsed=0
DockId=0x0000001A,6

[Window][Add Effects]
Pos=0,-23
Size=32,33
Collapsed=0
DockId=0x00000007,1

[Window][Lens Preview]
Pos=0,-23
Size=32,33
Collapsed=0
DockId=0x00000018,0

[Window][Add SplitNode]
Pos=0,17
Size=288,1160
Collapsed=0
DockId=0x00000007,1

[Window][Terminal]
Pos=635,1123
Size=1925,294
Collapsed=0
DockId=0x00000017,1

[Window][New Draw Layer]
Pos=1008,659
Size=276,73
Collapsed=0

[Window][Input Manager]
Pos=1225,519
Size=673,572
Collapsed=0

[Window][Bake]
Pos=358,538
Size=1769,764
Collapsed=0

[Window][Profiler2]
Pos=1185,580
Size=417,305
Collapsed=0

[Table][0x8DFA6E86,2]
Column 0  Weight=1.0000
Column 1  Weight=1.0000

[Table][0xFABAAEF7,2]
Column 0  Weight=1.0000
Column 1  Weight=1.0000

[Table][0xC179E37C,3]
RefScale=13
Column 0  Width=108 Sort=0v
Column 1  Weight=1.0000
Column 2  Width=-1

[Table][0x861D378E,3]
Column 0  Weight=1.0000
Column 1  Weight=1.0000
Column 2  Weight=1.0000

[Table][0x1F146634,3]
RefScale=13
Column 0  Width=63
Column 1  Width=63
Column 2  Width=63

[Table][0xDA36A7E0,6]
RefScale=13
Column 0  Width=28 Sort=0v
Column 1  Width=42
Column 2  Width=75
Column 3  Width=69
Column 4  Weight=1.0000
Column 5  Width=-1

[Table][0xA9198EC7,2]
Column 0  Weight=0.3680
Column 1  Weight=0.0752

[Table][0xE9AEB3C1,2]
RefScale=13
Column 0  Weight=1.0000
Column 1  Width=337

[Docking][Data]
DockNode                  ID=0x0000000F Pos=372,434 Size=669,402 Split=X
  DockNode                ID=0x00000010 Parent=0x0000000F SizeRef=256,415 Selected=0x5B9D583B
  DockNode                ID=0x00000011 Parent=0x0000000F SizeRef=442,415 Selected=0x9A5A9887
DockSpace                 ID=0x8B93E3BD Window=0xA787BDB4 Pos=0,40 Size=2560,1400 Split=Y Selected=0x92A5F8F1
  DockNode                ID=0x00000006 Parent=0x8B93E3BD SizeRef=1920,794 Split=X
    DockNode              ID=0x00000003 Parent=0x00000006 SizeRef=2072,519 Split=X Selected=0xE192E354
      DockNode            ID=0x00000002 Parent=0x00000003 SizeRef=288,1040 Split=Y Selected=0x0339283F
        DockNode          ID=0x00000007 Parent=0x00000002 SizeRef=221,350 Selected=0x0339283F
        DockNode          ID=0x00000008 Parent=0x00000002 SizeRef=221,442 Selected=0xDA1ACE14
      DockNode            ID=0x00000005 Parent=0x00000003 SizeRef=1782,1040 Split=Y Selected=0xCEFDDD0F
        DockNode          ID=0x0000001A Parent=0x00000005 SizeRef=1801,901 CentralNode=1 Selected=0xC498E03B
        DockNode          ID=0x0000001B Parent=0x00000005 SizeRef=1801,201 Selected=0x3E6B10D2
    DockNode              ID=0x00000004 Parent=0x00000006 SizeRef=486,519 Split=Y Selected=0x199AB496
      DockNode            ID=0x0000000B Parent=0x00000004 SizeRef=340,555 Split=Y Selected=0x382916D5
        DockNode          ID=0x00000009 Parent=0x0000000B SizeRef=491,463 Split=Y Selected=0x7EEC4370
          DockNode        ID=0x0000000D Parent=0x00000009 SizeRef=454,311 Split=Y Selected=0x559CBBD5
            DockNode      ID=0x00000012 Parent=0x0000000D SizeRef=454,385 Split=Y Selected=0x199AB496
              DockNode    ID=0x00000001 Parent=0x00000012 SizeRef=454,374 Selected=0x97EB5450
              DockNode    ID=0x00000015 Parent=0x00000012 SizeRef=454,428 Split=Y Selected=0x199AB496
                DockNode  ID=0x00000018 Parent=0x00000015 SizeRef=365,495 Selected=0x1FB9232D
                DockNode  ID=0x00000019 Parent=0x00000015 SizeRef=365,607 Selected=0x559CBBD5
            DockNode      ID=0x00000013 Parent=0x0000000D SizeRef=454,208 Selected=0x0B40521E
          DockNode        ID=0x0000000E Parent=0x00000009 SizeRef=454,489 Selected=0x11D88C76
        DockNode          ID=0x0000000A Parent=0x0000000B SizeRef=491,193 Selected=0xDE92E756
      DockNode            ID=0x0000000C Parent=0x00000004 SizeRef=340,237 Selected=0x7CD627AF
  DockNode                ID=0x00000014 Parent=0x8B93E3BD SizeRef=1920,604 Split=X Selected=0x26CE0345
    DockNode              ID=0x00000016 Parent=0x00000014 SizeRef=273,234 Selected=0x26CE0345
    DockNode              ID=0x00000017 Parent=0x00000014 SizeRef=2285,234 Selected=0x49278EEE

";

        public unsafe ImGuiManager(SdlWindow window, IGraphicsDevice device, IGraphicsContext context, ImGuiConfigFlags flags = ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.NavEnableGamepad | ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable)
        {
            if (!File.Exists("imgui.ini"))
            {
                File.WriteAllText("imgui.ini", defaultIniConfig);
            }

            guiContext = ImGui.CreateContext(null);
            ImGui.SetCurrentContext(guiContext);

            ImGui.SetCurrentContext(guiContext);
            ImGuizmo.SetImGuiContext(guiContext);
            ImPlot.SetImGuiContext(guiContext);
            ImNodes.SetImGuiContext(guiContext);

            nodesContext = ImNodes.CreateContext();
            ImNodes.SetCurrentContext(nodesContext);
            ImNodes.StyleColorsDark(ImNodes.GetStyle());

            plotContext = ImPlot.CreateContext();
            ImPlot.SetCurrentContext(plotContext);
            ImPlot.StyleColorsDark(ImPlot.GetStyle());

            var io = ImGui.GetIO();
            io.ConfigFlags |= flags;
            io.ConfigViewportsNoAutoMerge = false;
            io.ConfigViewportsNoTaskBarIcon = false;

            var config = ImGui.ImFontConfig();
            io.Fonts.AddFontDefault(config);

            config.MergeMode = true;
            config.GlyphMinAdvanceX = 18;
            config.GlyphOffset = new(0, 4);
            var glyphRanges = new char[]
            {
                (char)0xE700, (char)0xF800,
                (char)0 // null terminator
            };

            fixed (char* pGlyphRanges = glyphRanges)
            {
                byte[] fontBytes = File.ReadAllBytes("assets/shared/fonts/SEGMDL2.TTF");
                byte* pFontBytes = (byte*)Marshal.AllocHGlobal((nint)fontBytes.Length);
                Marshal.Copy(fontBytes, 0, (nint)pFontBytes, fontBytes.Length);

                // IMPORTANT: AddFontFromMemoryTTF() by default transfer ownership of the data buffer to the font atlas, which will attempt to free it on destruction.
                // This was to avoid an unnecessary copy, and is perhaps not a good API (a future version will redesign it).
                io.Fonts.AddFontFromMemoryTTF(pFontBytes, fontBytes.Length, 14, config, pGlyphRanges);
            }

            var style = ImGui.GetStyle();
            var colors = style.Colors;

            colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.13f, 0.13f, 0.13f, 1.00f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
            colors[(int)ImGuiCol.Separator] = new Vector4(0.48f, 0.48f, 0.48f, 0.39f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
            colors[(int)ImGuiCol.Tab] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.TabActive] = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
            colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
            colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.NavHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.10f, 0.10f, 0.10f, 0.00f);

            style.WindowPadding = new Vector2(8.00f, 8.00f);
            style.FramePadding = new Vector2(5.00f, 2.00f);
            style.CellPadding = new Vector2(6.00f, 6.00f);
            style.ItemSpacing = new Vector2(6.00f, 6.00f);
            style.ItemInnerSpacing = new Vector2(6.00f, 6.00f);
            style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
            style.IndentSpacing = 25;
            style.ScrollbarSize = 15;
            style.GrabMinSize = 10;
            style.WindowBorderSize = 1;
            style.ChildBorderSize = 1;
            style.PopupBorderSize = 1;
            style.FrameBorderSize = 1;
            style.TabBorderSize = 1;
            style.WindowRounding = 7;
            style.ChildRounding = 4;
            style.FrameRounding = 3;
            style.PopupRounding = 4;
            style.ScrollbarRounding = 9;
            style.GrabRounding = 3;
            style.LogSliderDeadzone = 4;
            style.TabRounding = 4;

            if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
            {
                style.WindowRounding = 0.0f;
                style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
            }

            ImGuiSDL2Platform.Init(window.GetWindow(), null, null);
            ImGuiRenderer.Init(device, context);
        }

        public unsafe void NewFrame()
        {
            ImGui.SetCurrentContext(guiContext);
            ImGuizmo.SetImGuiContext(guiContext);
            ImPlot.SetImGuiContext(guiContext);
            ImNodes.SetImGuiContext(guiContext);

            ImNodes.SetCurrentContext(nodesContext);
            ImPlot.SetCurrentContext(plotContext);

            ImGuiSDL2Platform.NewFrame();
            ImGui.NewFrame();
            ImGuizmo.BeginFrame();

            ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
            DockSpaceId = ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode, null);
            ImGui.PopStyleColor(1);
        }

        public static int DockSpaceId { get; private set; }

        public unsafe void EndFrame()
        {
            var io = ImGui.GetIO();
            ImGui.Render();
            ImGui.EndFrame();
            ImGuiRenderer.RenderDrawData(ImGui.GetDrawData());

            if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
            {
                ImGui.UpdatePlatformWindows();
                ImGui.RenderPlatformWindowsDefault();
            }
        }

        public void Dispose()
        {
            ImGuiRenderer.Shutdown();
            ImGuiSDL2Platform.Shutdown();
        }
    }
}