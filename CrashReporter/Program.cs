// See https://aka.ms/new-console-template for more information
using CrashReporter;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.ImGui.Backends.SDL3;
using Hexa.NET.OpenGL;
using Hexa.NET.SDL3;
using System.Numerics;
using SDLEvent = Hexa.NET.SDL3.SDLEvent;
using SDLWindowPtr = Hexa.NET.ImGui.Backends.SDL3.SDLWindowPtr;

SDL.SetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");
SDL.Init((uint)(SDLInitFlags.Events | SDLInitFlags.Video));
unsafe
{
    float main_scale = SDL.GetDisplayContentScale(SDL.GetPrimaryDisplay());
    var window = SDL.CreateWindow("CrashPad", (int)(600 * main_scale), (int)(700 * main_scale), (ulong)(SDLWindowFlags.Resizable | SDLWindowFlags.Opengl | SDLWindowFlags.HighPixelDensity | SDLWindowFlags.Borderless));
    var windowId = SDL.GetWindowID(window);

    var guiContext = ImGui.CreateContext();
    ImGui.SetCurrentContext(guiContext);

    // Setup ImGui config.
    var io = ImGui.GetIO();
    io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;     // Enable Keyboard Controls
    io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;      // Enable Gamepad Controls
    io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;         // Enable Docking
    io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;       // Enable Multi-Viewport / Platform Windows
    io.IniFilename = null;
    io.ConfigViewportsNoAutoMerge = false;
    io.ConfigViewportsNoTaskBarIcon = false;

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
    colors[(int)ImGuiCol.TabSelected] = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
    colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
    colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
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
    colors[(int)ImGuiCol.NavCursor] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
    colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
    colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
    colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.10f, 0.10f, 0.10f, 0.00f);

    style.WindowPadding = new Vector2(8.00f, 8.00f);
    style.FramePadding = new Vector2(8.00f, 6.00f);
    style.CellPadding = new Vector2(6.00f, 6.00f);
    style.ItemSpacing = new Vector2(6.00f, 6.00f);
    style.ItemInnerSpacing = new Vector2(6.00f, 6.00f);
    style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
    style.IndentSpacing = 25;
    style.ScrollbarSize = 15;
    style.GrabMinSize = 12;
    style.WindowBorderSize = 1;
    style.ChildBorderSize = 1;
    style.PopupBorderSize = 1;
    style.FrameBorderSize = 1;
    style.TabBorderSize = 1;
    style.WindowRounding = 7;
    style.ChildRounding = 4;
    style.FrameRounding = 12;
    style.PopupRounding = 4;
    style.ScrollbarRounding = 9;
    style.GrabRounding = 3;
    style.LogSliderDeadzone = 4;
    style.TabRounding = 4;

    style.ScaleAllSizes(main_scale);        // Bake a fixed style scale. (until we have a solution for dynamic style scaling, changing this requires resetting Style + calling this again)
    style.FontScaleDpi = main_scale;        // Set initial font scale. (using io.ConfigDpiScaleFonts=true makes this unnecessary. We leave both here for documentation purpose)
    io.ConfigDpiScaleFonts = true;          // [Experimental] Automatically overwrite style.FontScaleDpi in Begin() when Monitor DPI changes. This will scale fonts but _NOT_ scale sizes/padding for now.
    io.ConfigDpiScaleViewports = true;

    var context = SDL.GLCreateContext(window);

    ImGuiImplSDL3.SetCurrentContext(guiContext);
    if (!ImGuiImplSDL3.InitForOpenGL(new SDLWindowPtr((Hexa.NET.ImGui.Backends.SDL3.SDLWindow*)window.Handle), (void*)context.Handle))
    {
        Console.WriteLine("Failed to init ImGui Impl SDL3");
        SDL.Quit();
        return;
    }

    ImGuiImplOpenGL3.SetCurrentContext(guiContext);
    if (!ImGuiImplOpenGL3.Init((byte*)null))
    {
        Console.WriteLine("Failed to init ImGui Impl OpenGL3");
        SDL.Quit();
        return;
    }

    GL GL = new(new BindingsContext(window, context));

    SDLEvent sdlEvent = default;
    bool exiting = false;

    CrashWindow crashWindow = new();
    crashWindow.Closing += (sender) =>
    {
        exiting = true;
    };

    while (!exiting)
    {
        SDL.PumpEvents();

        while (SDL.PollEvent(ref sdlEvent))
        {
            ImGuiImplSDL3.ProcessEvent((Hexa.NET.ImGui.Backends.SDL3.SDLEvent*)&sdlEvent);

            switch ((SDLEventType)sdlEvent.Type)
            {
                case SDLEventType.Quit:
                    exiting = true;
                    break;

                case SDLEventType.Terminating:
                    exiting = true;
                    break;

                case SDLEventType.WindowCloseRequested:
                    var windowEvent = sdlEvent.Window;
                    if (windowEvent.WindowID == windowId)
                    {
                        exiting = true;
                    }
                    break;
            }
        }

        GL.MakeCurrent();
        GL.ClearColor(0, 0, 0, 1);
        GL.Clear(GLClearBufferMask.ColorBufferBit);

        ImGuiImplOpenGL3.NewFrame();
        ImGuiImplSDL3.NewFrame();
        ImGui.NewFrame();

        crashWindow.Draw();

        ImGui.Render();
        ImGui.EndFrame();

        GL.MakeCurrent();
        ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());

        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        }

        GL.MakeCurrent();

        // Swap front and back buffers (double buffering)
        GL.SwapBuffers();
    }

    ImGuiImplOpenGL3.Shutdown();
    ImGuiImplSDL3.Shutdown();
    ImGui.DestroyContext();
    GL.Dispose();

    SDL.DestroyWindow(window);
    SDL.Quit();
}