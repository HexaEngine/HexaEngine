namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.ImGui;
    using Hexa.NET.SDL2;
    using Hexa.NET.Utilities;
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using static HexaEngine.Core.Extensions.SdlErrorHandlingExtensions;

    public static class ImGuiSDL2Platform
    {
        public enum GamepadMode
        {
            AutoFirst,
            AutoAll,
            Manual
        }

        /// <summary>
        /// SDL Data
        /// </summary>
        private unsafe struct BackendData
        {
            public SDLWindow* Window;
            public SDLRenderer* Renderer;
            public ulong Time;
            public byte* ClipboardTextData;
            public bool UseVulkan;
            public bool WantUpdateMonitors;

            public uint MouseWindowID;
            public int MouseButtonsDown;
            public SDLCursor** MouseCursors;
            public SDLCursor* LastMouseCursor;
            public int MouseLastLeaveFrame;
            public bool MouseCanUseGlobalState;
            public bool MouseCanReportHoveredViewport;  // This is hard to use/unreliable on SDL so we'll set ImGuiBackendFlags_HasMouseHoveredViewport dynamically based on state.

            public UnsafeList<Pointer<SDLGameController>> Gamepads;
            public GamepadMode GamepadMode;
            public bool WantUpdateGamepadsList;
        }

        /// <summary>
        /// Backend data stored in io.BackendPlatformUserData to allow support for multiple Dear ImGui contexts
        /// It is STRONGLY preferred that you use docking branch with multi-viewports (== single Dear ImGui context + multiple windows) instead of multiple Dear ImGui contexts.
        /// FIXME: multi-context support is not well tested and probably dysfunctional in this backend.
        /// FIXME: some shared resources (mouse cursor shape, gamepad) are mishandled when using multi-context.
        /// </summary>
        /// <returns></returns>
        private static unsafe BackendData* GetBackendData()
        {
            return !ImGui.GetCurrentContext().IsNull ? (BackendData*)ImGui.GetIO().BackendPlatformUserData : null;
        }

        private static unsafe byte* GetClipboardText(void* data)
        {
            BackendData* bd = GetBackendData();
            if (bd->ClipboardTextData != null)
            {
                SDL.Free(bd->ClipboardTextData);
            }

            bd->ClipboardTextData = SDL.GetClipboardText();
            return bd->ClipboardTextData;
        }

        private static unsafe void SetClipboardText(void* data, byte* text)
        {
            SDL.SetClipboardText(text);
        }

        /// <summary>
        /// Note: native IME will only display if user calls SDL_SetHint(SDL_HINT_IME_SHOW_UI, "1") _before_ SDL_CreateWindow().
        /// </summary>
        /// <param name="vp"></param>
        /// <param name="data"></param>
        private static unsafe void SetPlatformImeData(ImGuiContext* ctx, ImGuiViewport* vp, ImGuiPlatformImeData* data)
        {
            if (data->WantVisible == 1)
            {
                SDLRect r = new((int)data->InputPos.X, (int)data->InputPos.Y, 1, (int)data->InputLineHeight);
                SDL.SetTextInputRect(&r);
            }
        }

        private static ImGuiKey KeycodeToImGuiKey(int keycode)
        {
            return (SDLKeyCode)keycode switch
            {
                SDLKeyCode.Tab => ImGuiKey.Tab,
                SDLKeyCode.Left => ImGuiKey.LeftArrow,
                SDLKeyCode.Right => ImGuiKey.RightArrow,
                SDLKeyCode.Up => ImGuiKey.UpArrow,
                SDLKeyCode.Down => ImGuiKey.DownArrow,
                SDLKeyCode.Pageup => ImGuiKey.PageUp,
                SDLKeyCode.Pagedown => ImGuiKey.PageDown,
                SDLKeyCode.Home => ImGuiKey.Home,
                SDLKeyCode.End => ImGuiKey.End,
                SDLKeyCode.Insert => ImGuiKey.Insert,
                SDLKeyCode.Delete => ImGuiKey.Delete,
                SDLKeyCode.Backspace => ImGuiKey.Backspace,
                SDLKeyCode.Space => ImGuiKey.Space,
                SDLKeyCode.Return => ImGuiKey.Enter,
                SDLKeyCode.Escape => ImGuiKey.Escape,
                SDLKeyCode.Quote => ImGuiKey.Apostrophe,
                SDLKeyCode.Comma => ImGuiKey.Comma,
                SDLKeyCode.Minus => ImGuiKey.Minus,
                SDLKeyCode.Period => ImGuiKey.Period,
                SDLKeyCode.Slash => ImGuiKey.Slash,
                SDLKeyCode.Semicolon => ImGuiKey.Semicolon,
                SDLKeyCode.Equals => ImGuiKey.Equal,
                SDLKeyCode.Leftbracket => ImGuiKey.LeftBracket,
                SDLKeyCode.Backslash => ImGuiKey.Backslash,
                SDLKeyCode.Rightbracket => ImGuiKey.RightBracket,
                SDLKeyCode.Backquote => ImGuiKey.GraveAccent,
                SDLKeyCode.Capslock => ImGuiKey.CapsLock,
                SDLKeyCode.Scrolllock => ImGuiKey.ScrollLock,
                SDLKeyCode.Numlockclear => ImGuiKey.NumLock,
                SDLKeyCode.Printscreen => ImGuiKey.PrintScreen,
                SDLKeyCode.Pause => ImGuiKey.Pause,
                SDLKeyCode.Kp0 => ImGuiKey.Keypad0,
                SDLKeyCode.Kp1 => ImGuiKey.Keypad1,
                SDLKeyCode.Kp2 => ImGuiKey.Keypad2,
                SDLKeyCode.Kp3 => ImGuiKey.Keypad3,
                SDLKeyCode.Kp4 => ImGuiKey.Keypad4,
                SDLKeyCode.Kp5 => ImGuiKey.Keypad5,
                SDLKeyCode.Kp6 => ImGuiKey.Keypad6,
                SDLKeyCode.Kp7 => ImGuiKey.Keypad7,
                SDLKeyCode.Kp8 => ImGuiKey.Keypad8,
                SDLKeyCode.Kp9 => ImGuiKey.Keypad9,
                SDLKeyCode.KpPeriod => ImGuiKey.KeypadDecimal,
                SDLKeyCode.KpDivide => ImGuiKey.KeypadDivide,
                SDLKeyCode.KpMultiply => ImGuiKey.KeypadMultiply,
                SDLKeyCode.KpMinus => ImGuiKey.KeypadSubtract,
                SDLKeyCode.KpPlus => ImGuiKey.KeypadAdd,
                SDLKeyCode.KpEnter => ImGuiKey.KeypadEnter,
                SDLKeyCode.KpEquals => ImGuiKey.KeypadEqual,
                SDLKeyCode.Lctrl => ImGuiKey.LeftCtrl,
                SDLKeyCode.Lshift => ImGuiKey.LeftShift,
                SDLKeyCode.Lalt => ImGuiKey.LeftAlt,
                SDLKeyCode.Lgui => ImGuiKey.LeftSuper,
                SDLKeyCode.Rctrl => ImGuiKey.RightCtrl,
                SDLKeyCode.Rshift => ImGuiKey.RightShift,
                SDLKeyCode.Ralt => ImGuiKey.RightAlt,
                SDLKeyCode.Rgui => ImGuiKey.RightSuper,
                SDLKeyCode.Application => ImGuiKey.Menu,
                SDLKeyCode.K0 => ImGuiKey.Key0,
                SDLKeyCode.K1 => ImGuiKey.Key1,
                SDLKeyCode.K2 => ImGuiKey.Key2,
                SDLKeyCode.K3 => ImGuiKey.Key3,
                SDLKeyCode.K4 => ImGuiKey.Key4,
                SDLKeyCode.K5 => ImGuiKey.Key5,
                SDLKeyCode.K6 => ImGuiKey.Key6,
                SDLKeyCode.K7 => ImGuiKey.Key7,
                SDLKeyCode.K8 => ImGuiKey.Key8,
                SDLKeyCode.K9 => ImGuiKey.Key9,
                SDLKeyCode.A => ImGuiKey.A,
                SDLKeyCode.B => ImGuiKey.B,
                SDLKeyCode.C => ImGuiKey.C,
                SDLKeyCode.D => ImGuiKey.D,
                SDLKeyCode.E => ImGuiKey.E,
                SDLKeyCode.F => ImGuiKey.F,
                SDLKeyCode.G => ImGuiKey.G,
                SDLKeyCode.H => ImGuiKey.H,
                SDLKeyCode.I => ImGuiKey.I,
                SDLKeyCode.J => ImGuiKey.J,
                SDLKeyCode.K => ImGuiKey.K,
                SDLKeyCode.L => ImGuiKey.L,
                SDLKeyCode.M => ImGuiKey.M,
                SDLKeyCode.N => ImGuiKey.N,
                SDLKeyCode.O => ImGuiKey.O,
                SDLKeyCode.P => ImGuiKey.P,
                SDLKeyCode.Q => ImGuiKey.Q,
                SDLKeyCode.R => ImGuiKey.R,
                SDLKeyCode.S => ImGuiKey.S,
                SDLKeyCode.T => ImGuiKey.T,
                SDLKeyCode.U => ImGuiKey.U,
                SDLKeyCode.V => ImGuiKey.V,
                SDLKeyCode.W => ImGuiKey.W,
                SDLKeyCode.X => ImGuiKey.X,
                SDLKeyCode.Y => ImGuiKey.Y,
                SDLKeyCode.Z => ImGuiKey.Z,
                SDLKeyCode.F1 => ImGuiKey.F1,
                SDLKeyCode.F2 => ImGuiKey.F2,
                SDLKeyCode.F3 => ImGuiKey.F3,
                SDLKeyCode.F4 => ImGuiKey.F4,
                SDLKeyCode.F5 => ImGuiKey.F5,
                SDLKeyCode.F6 => ImGuiKey.F6,
                SDLKeyCode.F7 => ImGuiKey.F7,
                SDLKeyCode.F8 => ImGuiKey.F8,
                SDLKeyCode.F9 => ImGuiKey.F9,
                SDLKeyCode.F10 => ImGuiKey.F10,
                SDLKeyCode.F11 => ImGuiKey.F11,
                SDLKeyCode.F12 => ImGuiKey.F12,
                SDLKeyCode.AcBack => ImGuiKey.AppBack,
                SDLKeyCode.AcForward => ImGuiKey.AppForward,
                _ => ImGuiKey.None,
            };
        }

        private static void UpdateKeyModifiers(SDLKeymod keymods)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.AddKeyEvent(ImGuiKey.ModCtrl, (keymods & SDLKeymod.Ctrl) != 0);
            io.AddKeyEvent(ImGuiKey.ModShift, (keymods & SDLKeymod.Shift) != 0);
            io.AddKeyEvent(ImGuiKey.ModAlt, (keymods & SDLKeymod.Alt) != 0);
            io.AddKeyEvent(ImGuiKey.ModSuper, (keymods & SDLKeymod.Gui) != 0);
        }

        /// <summary>
        /// You can read the io.WantCaptureMouse, io.WantCaptureKeyboard flags to tell if dear imgui wants to use your inputs.
        /// - When io.WantCaptureMouse is true, do not dispatch mouse input data to your main application, or clear/overwrite your copy of the mouse data.
        /// - When io.WantCaptureKeyboard is true, do not dispatch keyboard input data to your main application, or clear/overwrite your copy of the keyboard data.
        /// Generally you may always pass all inputs to dear imgui, and hide them from your application based on those two flags.
        /// If you have multiple SDL events and some of them are not meant to be used by dear imgui, you may need to filter events based on their windowID field.
        /// </summary>
        private static unsafe bool ProcessEvent(SDLEvent env)
        {
            var io = ImGui.GetIO();
            var bd = GetBackendData();

            switch ((SDLEventType)env.Type)
            {
                case SDLEventType.Mousemotion:
                    {
                        Vector2 mouse_pos = new(env.Motion.X, env.Motion.Y);
                        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
                        {
                            int window_x, window_y;
                            var window = SDL.GetWindowFromID(env.Motion.WindowID);
                            if (window == null)
                            {
                                SdlCheckError();
                            }

                            SDL.GetWindowPosition(window, &window_x, &window_y);
                            mouse_pos.X += window_x;
                            mouse_pos.Y += window_y;
                        }

                        io.AddMouseSourceEvent(env.Motion.Which == unchecked((uint)-1) ? ImGuiMouseSource.TouchScreen : ImGuiMouseSource.Mouse);
                        io.AddMousePosEvent(mouse_pos.X, mouse_pos.Y);
                        return true;
                    }
                case SDLEventType.Mousewheel:
                    {
                        //IMGUI_DEBUG_LOG("wheel %.2f %.2f, precise %.2f %.2f\n", (float)event->wheel.x, (float)event->wheel.y, event->wheel.preciseX, event->wheel.preciseY);

                        float wheel_x = -(float)env.Wheel.X;
                        float wheel_y = env.Wheel.Y;

                        //wheel_x /= 100.0f;

                        io.AddMouseSourceEvent(env.Wheel.Which == unchecked((uint)-1) ? ImGuiMouseSource.TouchScreen : ImGuiMouseSource.Mouse);
                        io.AddMouseWheelEvent(wheel_x, wheel_y);
                        return true;
                    }
                case SDLEventType.Mousebuttondown:
                case SDLEventType.Mousebuttonup:
                    {
                        int mouse_button = -1;
                        if (env.Button.Button == SDL.SDL_BUTTON_LEFT) { mouse_button = 0; }
                        if (env.Button.Button == SDL.SDL_BUTTON_RIGHT) { mouse_button = 1; }
                        if (env.Button.Button == SDL.SDL_BUTTON_MIDDLE) { mouse_button = 2; }
                        if (env.Button.Button == SDL.SDL_BUTTON_X1) { mouse_button = 3; }
                        if (env.Button.Button == SDL.SDL_BUTTON_X2) { mouse_button = 4; }
                        if (mouse_button == -1)
                        {
                            break;
                        }

                        io.AddMouseSourceEvent(env.Button.Which == unchecked((uint)-1) ? ImGuiMouseSource.TouchScreen : ImGuiMouseSource.Mouse);
                        io.AddMouseButtonEvent(mouse_button, env.Type == (int)SDLEventType.Mousebuttondown);
                        bd->MouseButtonsDown = (env.Type == (int)SDLEventType.Mousebuttondown) ? (bd->MouseButtonsDown | (1 << mouse_button)) : (bd->MouseButtonsDown & ~(1 << mouse_button));
                        return true;
                    }
                case SDLEventType.Textinput:
                    {
                        io.AddInputCharactersUTF8(&env.Text.Text_0);
                        return true;
                    }
                case SDLEventType.Keydown:
                case SDLEventType.Keyup:
                    {
                        UpdateKeyModifiers((SDLKeymod)env.Key.Keysym.Mod);
                        ImGuiKey key = KeycodeToImGuiKey(env.Key.Keysym.Sym);
                        io.AddKeyEvent(key, env.Type == (int)SDLEventType.Keydown);
                        io.SetKeyEventNativeData(key, env.Key.Keysym.Sym, (int)env.Key.Keysym.Scancode, (int)env.Key.Keysym.Scancode); // To support legacy indexing (<1.87 user code). Legacy backend uses SDLK_*** as indices to IsKeyXXX() functions.
                        return true;
                    }

                case SDLEventType.Displayevent:
                    {
                        // 2.0.26 has SDL_DISPLAYEVENT_CONNECTED/SDL_DISPLAYEVENT_DISCONNECTED/SDL_DISPLAYEVENT_ORIENTATION,
                        // so change of DPI/Scaling are not reflected in this event. (SDL3 has it)
                        bd->WantUpdateMonitors = true;
                        return true;
                    }

                case SDLEventType.Windowevent:
                    {
                        // - When capturing mouse, SDL will send a bunch of conflicting LEAVE/ENTER event on every mouse move, but the final ENTER tends to be right.
                        // - However we won't get a correct LEAVE event for a captured window.
                        // - In some cases, when detaching a window from main viewport SDL may send SDL_WINDOWEVENT_ENTER one frame too late,
                        //   causing SDL_WINDOWEVENT_LEAVE on previous frame to interrupt drag operation by clear mouse position. This is why
                        //   we delay process the SDL_WINDOWEVENT_LEAVE events by one frame. See issue #5012 for details.
                        SDLWindowEventID window_event = (SDLWindowEventID)env.Window.Type;
                        if (window_event == SDLWindowEventID.Enter)
                        {
                            bd->MouseWindowID = env.Window.WindowID;
                            bd->MouseLastLeaveFrame = 0;
                        }
                        if (window_event == SDLWindowEventID.Leave)
                        {
                            bd->MouseLastLeaveFrame = ImGui.GetFrameCount() + 1;
                        }

                        if (window_event == SDLWindowEventID.FocusGained)
                        {
                            io.AddFocusEvent(true);
                        }
                        else if (window_event == SDLWindowEventID.FocusLost)
                        {
                            io.AddFocusEvent(false);
                        }

                        if (window_event == SDLWindowEventID.Close || window_event == SDLWindowEventID.Moved || window_event == SDLWindowEventID.Resized)
                        {
                            var window = SDL.GetWindowFromID(env.Window.WindowID);
                            if (window == null)
                            {
                                SdlCheckError();
                            }

                            ImGuiViewport* viewport = ImGui.FindViewportByPlatformHandle(window);

                            if (viewport != null)
                            {
                                if (window_event == SDLWindowEventID.Close)
                                {
                                    viewport->PlatformRequestClose = 1;
                                }

                                if (window_event == SDLWindowEventID.Moved)
                                {
                                    viewport->PlatformRequestMove = 1;
                                }

                                if (window_event == SDLWindowEventID.SizeChanged)
                                {
                                    viewport->PlatformRequestResize = 1;
                                }

                                return true;
                            }
                        }

                        return true;
                    }

                case SDLEventType.Controllerdeviceadded:
                case SDLEventType.Controllerdeviceremoved:
                    {
                        bd->WantUpdateGamepadsList = true;
                        return true;
                    }
            }

            return false;
        }

        public static unsafe bool Init(SDLWindow* window, SDLRenderer* renderer, void* sdlGLContext)
        {
            var io = ImGui.GetIO();
            Trace.Assert(io.BackendPlatformUserData == null, "Already initialized a platform backend!");

            Application.RegisterHook(ProcessEvent);

            bool mouse_can_use_global_state = false;
            string sdl_backend = SDL.GetCurrentVideoDriverS();
            string[] global_mouse_whitelist = { "windows", "cocoa", "x11", "DIVE", "VMAN" };
            for (int n = 0; n < global_mouse_whitelist.Length; n++)
            {
                if (sdl_backend == global_mouse_whitelist[n])
                {
                    mouse_can_use_global_state = true;
                }
            }

            BackendData* bd = AllocT<BackendData>();
            ZeroMemoryT(bd);
            io.BackendPlatformUserData = bd;
            io.BackendPlatformName = "ImGui_SDL2_Platform".ToUTF8Ptr();
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos;

            if (mouse_can_use_global_state)
            {
                io.BackendFlags |= ImGuiBackendFlags.PlatformHasViewports;
            }

            bd->Window = window;
            bd->Renderer = renderer;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                bd->MouseCanReportHoveredViewport = false;
            }
            else
            {
                bd->MouseCanReportHoveredViewport = bd->MouseCanUseGlobalState;
            }
            bd->WantUpdateMonitors = true;

            io.SetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate<SetClipboardTextFn>(SetClipboardText);
            io.GetClipboardTextFn = (void*)Marshal.GetFunctionPointerForDelegate<GetClipboardTextFn>(GetClipboardText);
            io.ClipboardUserData = null;
            io.PlatformSetImeDataFn = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetImeDataFn>(SetPlatformImeData);

            bd->MouseCursors = (SDLCursor**)AllocArray((uint)ImGuiMouseCursor.Count);
            bd->MouseCursors[(int)ImGuiMouseCursor.Arrow] = SdlCheckError(SDL.CreateSystemCursor(SDLSystemCursor.Arrow));
            bd->MouseCursors[(int)ImGuiMouseCursor.TextInput] = SdlCheckError(SDL.CreateSystemCursor(SDLSystemCursor.Ibeam));
            bd->MouseCursors[(int)ImGuiMouseCursor.ResizeAll] = SdlCheckError(SDL.CreateSystemCursor(SDLSystemCursor.Sizeall));
            bd->MouseCursors[(int)ImGuiMouseCursor.ResizeNs] = SdlCheckError(SDL.CreateSystemCursor(SDLSystemCursor.Sizens));
            bd->MouseCursors[(int)ImGuiMouseCursor.ResizeEw] = SdlCheckError(SDL.CreateSystemCursor(SDLSystemCursor.Sizewe));
            bd->MouseCursors[(int)ImGuiMouseCursor.ResizeNesw] = SdlCheckError(SDL.CreateSystemCursor(SDLSystemCursor.Sizenesw));
            bd->MouseCursors[(int)ImGuiMouseCursor.ResizeNwse] = SdlCheckError(SDL.CreateSystemCursor(SDLSystemCursor.Sizenwse));
            bd->MouseCursors[(int)ImGuiMouseCursor.Hand] = SdlCheckError(SDL.CreateSystemCursor(SDLSystemCursor.Hand));
            bd->MouseCursors[(int)ImGuiMouseCursor.NotAllowed] = SdlCheckError(SDL.CreateSystemCursor(SDLSystemCursor.No));

            // Set platform dependent data in viewport
            // Our mouse update function expect PlatformHandle to be filled for the main viewport
            ImGuiViewport* main_viewport = ImGui.GetMainViewport();
            main_viewport->PlatformHandle = window;
            main_viewport->PlatformHandleRaw = null;
            SDLSysWMInfo info;
            SDL.GetVersion(&info.Version);
            if (SDL.GetWindowWMInfo(window, &info) == SDLBool.True)
            {
                if (sdl_backend == "windows")
                {
                    main_viewport->PlatformHandleRaw = (void*)info.Info.Win.Window;
                }
                else if (sdl_backend == "cocoa")
                {
                    main_viewport->PlatformHandleRaw = (void*)info.Info.Cocoa.Window;
                }
            }
            else
            {
                SdlCheckError();
            }
            // From 2.0.5: Set SDL hint to receive mouse click events on window focus, otherwise SDL doesn't emit the event.
            // Without this, when clicking to gain focus, our widgets wouldn't activate even though they showed as hovered.
            // (This is unfortunately a global SDL setting, so enabling it might have a side-effect on your application.
            // It is unlikely to make a difference, but if your app absolutely needs to ignore the initial on-focus click:
            // you can ignore SDL_MOUSEBUTTONDOWN events coming right after a SDL_WINDOWEVENT_FOCUS_GAINED)
            SDL.SetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");

            // From 2.0.18: Enable native IME.
            // IMPORTANT: This is used at the time of SDL_CreateWindow() so this will only affects secondary windows, if any.
            // For the main window to be affected, your application needs to call this manually before calling SDL_CreateWindow().
            SDL.SetHint(SDL.SDL_HINT_IME_SHOW_UI, "1");

            // From 2.0.22: Disable auto-capture, this is preventing drag and drop across multiple windows (see #5710)
            SDL.SetHint(SDL.SDL_HINT_MOUSE_AUTO_CAPTURE, "0");

            // We need SDL_CaptureMouse(), SDL_GetGlobalMouseState() from SDL 2.0.4+ to support multiple viewports.
            // We left the call to ImGui_ImplSDL2_InitPlatformInterface() outside of #ifdef to avoid unused-function warnings.
            if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0 && (io.BackendFlags & ImGuiBackendFlags.PlatformHasViewports) != 0)
            {
                InitPlatformInterface(window, sdlGLContext);
            }

            return true;
        }

        public static unsafe bool InitForOpenGL(SDLWindow* window, void* sdl_gl_context)
        {
            return Init(window, null, sdl_gl_context);
        }

        public static unsafe bool InitForVulkan(SDLWindow* window)
        {
            if (!Init(window, null, null))
            {
                return false;
            }

            BackendData* bd = GetBackendData();
            bd->UseVulkan = true;
            return true;
        }

        public static unsafe bool InitForD3D(SDLWindow* window)
        {
            return Init(window, null, null);
        }

        public static unsafe bool InitForMetal(SDLWindow* window)
        {
            return Init(window, null, null);
        }

        public static unsafe bool InitForSDLRenderer(SDLWindow* window, SDLRenderer* renderer)
        {
            return Init(window, renderer, null);
        }

        public static unsafe void Shutdown()
        {
            BackendData* bd = GetBackendData();
            Trace.Assert(bd != null, "No platform backend to shutdown, or already shutdown?");
            var io = ImGui.GetIO();

            ShutdownPlatformInterface();

            if (bd->ClipboardTextData != null)
            {
                SDL.Free(bd->ClipboardTextData);
            }

            for (ImGuiMouseCursor cursor_n = 0; cursor_n < ImGuiMouseCursor.Count; cursor_n++)
            {
                SDL.FreeCursor(bd->MouseCursors[(int)cursor_n]);
            }

            Free(bd->MouseCursors);
            CloseGamepads();

            io.BackendPlatformName = null;
            io.BackendPlatformUserData = null;
            io.BackendFlags &= ~(ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos | ImGuiBackendFlags.HasGamepad | ImGuiBackendFlags.PlatformHasViewports | ImGuiBackendFlags.HasMouseHoveredViewport);
            Free(bd);
        }

        private static unsafe void UpdateMouseData()
        {
            var bd = GetBackendData();
            var io = ImGui.GetIO();

            SDL.CaptureMouse(bd->MouseButtonsDown != 0 ? SDLBool.True : SDLBool.False).SdlThrowIfNeg();
            SDLWindow* focused_window = SDL.GetKeyboardFocus();
            bool isAppFocused = focused_window != null && (bd->Window == focused_window || !ImGui.FindViewportByPlatformHandle(focused_window).IsNull);

            if (isAppFocused)
            {
                if (io.WantSetMousePos)
                {
                    // (Optional) Set OS mouse position from Dear ImGui if requested (rarely used, only when ImGuiConfigFlags_NavEnableSetMousePos is enabled by user)
                    if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
                    {
                        SDL.WarpMouseGlobal((int)io.MousePos.X, (int)io.MousePos.Y).SdlThrowIfNeg();
                    }
                    else
                    {
                        SDL.WarpMouseInWindow(focused_window, (int)io.MousePos.X, (int)io.MousePos.Y);
                    }
                }

                // (Optional) Fallback to provide mouse position when focused (SDL_MOUSEMOTION already provides this when hovered or captured)
                if (bd->MouseCanUseGlobalState && bd->MouseButtonsDown == 0)
                {
                    // Single-viewport mode: mouse position in client window coordinates (io.MousePos is (0,0) when the mouse is on the upper-left corner of the app window)
                    // Multi-viewport mode: mouse position in OS absolute coordinates (io.MousePos is (0,0) when the mouse is on the upper-left of the primary monitor)
                    var global = Mouse.Global;
                    if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) == 0)
                    {
                        int x, y;
                        SDL.GetWindowPosition(focused_window, &x, &y);
                        global.X -= x;
                        global.Y -= y;
                    }
                    io.AddMousePosEvent(global.X, global.Y);
                }
            }

            // (Optional) When using multiple viewports: call io.AddMouseViewportEvent() with the viewport the OS mouse cursor is hovering.
            // If ImGuiBackendFlags_HasMouseHoveredViewport is not set by the backend, Dear imGui will ignore this field and infer the information using its flawed heuristic.
            // - [!] SDL backend does NOT correctly ignore viewports with the _NoInputs flag.
            //       Some backend are not able to handle that correctly. If a backend report an hovered viewport that has the _NoInputs flag (e.g. when dragging a window
            //       for docking, the viewport has the _NoInputs flag in order to allow us to find the viewport under), then Dear ImGui is forced to ignore the value reported
            //       by the backend, and use its flawed heuristic to guess the viewport behind.
            // - [X] SDL backend correctly reports this regardless of another viewport behind focused and dragged from (we need this to find a useful drag and drop target).
            if ((io.BackendFlags & ImGuiBackendFlags.HasMouseHoveredViewport) != 0)
            {
                uint mouse_viewport_id = 0;
                SDLWindow* sdl_mouse_window = SDL.GetWindowFromID(bd->MouseWindowID);
                if (sdl_mouse_window != null)
                {
                    ImGuiViewport* mouse_viewport = ImGui.FindViewportByPlatformHandle(sdl_mouse_window);
                    if (mouse_viewport != null)
                    {
                        mouse_viewport_id = mouse_viewport->ID;
                    }
                }

                io.AddMouseViewportEvent(mouse_viewport_id);
            }
        }

        private static unsafe void UpdateMouseCursor()
        {
            var io = ImGui.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
            {
                return;
            }

            var bd = GetBackendData();

            var imgui_cursor = ImGui.GetMouseCursor();
            if (io.MouseDrawCursor || imgui_cursor == ImGuiMouseCursor.None)
            {
                // Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
                SDL.ShowCursor((int)SDLBool.False).SdlThrowIfNeg();
            }
            else
            {
                // Show OS mouse cursor
                SDLCursor* expected_cursor = bd->MouseCursors[(int)imgui_cursor] != null ? bd->MouseCursors[(int)imgui_cursor] : bd->MouseCursors[(int)ImGuiMouseCursor.Arrow];
                if (bd->LastMouseCursor != expected_cursor)
                {
                    SDL.SetCursor(expected_cursor); // SDL function doesn't have an early out (see #6113)
                    bd->LastMouseCursor = expected_cursor;
                }
                SDL.ShowCursor((int)SDLBool.True).SdlThrowIfNeg();
            }
        }

        private static unsafe void CloseGamepads()
        {
            var bd = GetBackendData();
            if (bd->GamepadMode != GamepadMode.Manual)
            {
                for (int i = 0; i < bd->Gamepads.Size; i++)
                {
                    SDLGameController* gamepad = bd->Gamepads[i];
                    SDL.GameControllerClose(gamepad);
                }
            }

            bd->Gamepads.Resize(0);
        }

        public static unsafe void SetGamepadMode(GamepadMode mode, SDLGameController** manual_gamepads_array, int manual_gamepads_count)
        {
            BackendData* bd = GetBackendData();
            CloseGamepads();
            if (mode == GamepadMode.Manual)
            {
                Debug.Assert(manual_gamepads_array != null && manual_gamepads_count > 0);
                for (int n = 0; n < manual_gamepads_count; n++)
                {
                    bd->Gamepads.PushBack(manual_gamepads_array[n]);
                }
            }
            else
            {
                Debug.Assert(manual_gamepads_array == null && manual_gamepads_count <= 0);
                bd->WantUpdateGamepadsList = true;
            }
            bd->GamepadMode = mode;
        }

        private static unsafe void UpdateGamepadButton(BackendData* bd, ImGuiIO* io, ImGuiKey key, SDLGameControllerButton button_no)
        {
            bool merged_value = false;
            for (int i = 0; i < bd->Gamepads.Size; i++)
            {
                SDLGameController* gamepad = bd->Gamepads[i];
                merged_value |= SDL.GameControllerGetButton(gamepad, button_no) != 0;
            }

            io->AddKeyEvent(key, merged_value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Saturate(float v)
        {
            return v < 0.0f ? 0.0f : v > 1.0f ? 1.0f : v;
        }

        private static unsafe void UpdateGamepadAnalog(BackendData* bd, ImGuiIO* io, ImGuiKey key, SDLGameControllerAxis axis_no, float v0, float v1)
        {
            float merged_value = 0.0f;
            for (int i = 0; i < bd->Gamepads.Size; i++)
            {
                SDLGameController* gamepad = bd->Gamepads[i];
                float vn = Saturate((float)(SDL.GameControllerGetAxis(gamepad, axis_no) - v0) / (float)(v1 - v0));
                if (merged_value < vn)
                {
                    merged_value = vn;
                }
            }
            io->AddKeyAnalogEvent(key, merged_value > 0.1f, merged_value);
        }

        private static unsafe void UpdateGamepads()
        {
            BackendData* bd = GetBackendData();
            ImGuiIOPtr io = ImGui.GetIO();

            // Update list of controller(s) to use
            if (bd->WantUpdateGamepadsList && bd->GamepadMode != GamepadMode.Manual)
            {
                CloseGamepads();
                int joystick_count = SDL.NumJoysticks();
                for (int n = 0; n < joystick_count; n++)
                {
                    if (SDL.IsGameController(n) == SDLBool.True)
                    {
                        SDLGameController* gamepad = SDL.GameControllerOpen(n);
                        if (gamepad != null)
                        {
                            bd->Gamepads.PushBack(gamepad);
                            if (bd->GamepadMode == GamepadMode.AutoFirst)
                            {
                                break;
                            }
                        }
                    }
                }

                bd->WantUpdateGamepadsList = false;
            }

            // FIXME: Technically feeding gamepad shouldn't depend on this now that they are regular inputs.
            if ((io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) == 0)
            {
                return;
            }

            io.BackendFlags &= ~ImGuiBackendFlags.HasGamepad;
            if (bd->Gamepads.Size == 0)
            {
                return;
            }

            io.BackendFlags |= ImGuiBackendFlags.HasGamepad;

            // Update gamepad inputs
            const int thumb_dead_zone = 8000; // SDL_gamecontroller.h suggests using this value.
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadStart, SDLGameControllerButton.Start);
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadBack, SDLGameControllerButton.Back);
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadFaceLeft, SDLGameControllerButton.X);              // Xbox X, PS Square
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadFaceRight, SDLGameControllerButton.B);              // Xbox B, PS Circle
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadFaceUp, SDLGameControllerButton.Y);              // Xbox Y, PS Triangle
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadFaceDown, SDLGameControllerButton.A);              // Xbox A, PS Cross
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadDpadLeft, SDLGameControllerButton.DpadLeft);
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadDpadRight, SDLGameControllerButton.DpadRight);
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadDpadUp, SDLGameControllerButton.DpadUp);
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadDpadDown, SDLGameControllerButton.DpadDown);
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadL1, SDLGameControllerButton.Leftshoulder);
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadR1, SDLGameControllerButton.Rightshoulder);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadL2, SDLGameControllerAxis.Triggerleft, 0.0f, 32767);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadR2, SDLGameControllerAxis.Triggerright, 0.0f, 32767);
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadL3, SDLGameControllerButton.Leftstick);
            UpdateGamepadButton(bd, io, ImGuiKey.GamepadR3, SDLGameControllerButton.Rightstick);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadLStickLeft, SDLGameControllerAxis.Leftx, -thumb_dead_zone, -32768);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadLStickRight, SDLGameControllerAxis.Leftx, +thumb_dead_zone, +32767);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadLStickUp, SDLGameControllerAxis.Lefty, -thumb_dead_zone, -32768);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadLStickDown, SDLGameControllerAxis.Lefty, +thumb_dead_zone, +32767);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadRStickLeft, SDLGameControllerAxis.Rightx, -thumb_dead_zone, -32768);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadRStickRight, SDLGameControllerAxis.Rightx, +thumb_dead_zone, +32767);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadRStickUp, SDLGameControllerAxis.Righty, -thumb_dead_zone, -32768);
            UpdateGamepadAnalog(bd, io, ImGuiKey.GamepadRStickDown, SDLGameControllerAxis.Righty, +thumb_dead_zone, +32767);
        }

        /// <summary>
        /// FIXME: Note that doesn't update with DPI/Scaling change only as SDL2 doesn't have an event for it (SDL3 has).
        /// </summary>
        private static unsafe void UpdateMonitors()
        {
            BackendData* bd = GetBackendData();
            ImGuiPlatformIO* platform_io = ImGui.GetPlatformIO();
            ImVector<ImGuiPlatformMonitor>* monitors = &platform_io->Monitors;
            monitors->Resize(0);
            bd->WantUpdateMonitors = false;
            int display_count = SDL.GetNumVideoDisplays();
            for (int n = 0; n < display_count; n++)
            {
                // Warning: the validity of monitor DPI information on Windows depends on the application DPI awareness settings, which generally needs to be set in the manifest or at runtime.
                ImGuiPlatformMonitor monitor = default;
                SDLRect r;
                SDL.GetDisplayBounds(n, &r);
                monitor.MainPos = monitor.WorkPos = new Vector2(r.X, r.Y);
                monitor.MainSize = monitor.WorkSize = new Vector2(r.W, r.H);

                SDL.GetDisplayUsableBounds(n, &r);
                monitor.WorkPos = new(r.X, r.Y);
                monitor.WorkSize = new(r.W, r.H);

                float dpi;
                SDL.GetDisplayDPI(n, &dpi, null, null);
                monitor.DpiScale = dpi / 96.0f;

                monitors->PushBack(monitor);
            }
        }

        public static unsafe void NewFrame()
        {
            var bd = GetBackendData();
            var io = ImGui.GetIO();
            int w, h, displayW, displayH;
            SDL.GetWindowSize(bd->Window, &w, &h);
            if (((SDLWindowFlags)SDL.GetWindowFlags(bd->Window) & SDLWindowFlags.Minimized) != 0)
            {
                w = h = 0;
            }
            if (bd->Renderer != null)
            {
                SDL.GetRendererOutputSize(bd->Renderer, &displayW, &displayH);
            }
            else
            {
                SDL.GLGetDrawableSize(bd->Window, &displayW, &displayH);
            }

            io.DisplaySize = new(w, h);
            if (w > 0 && h > 0)
            {
                io.DisplayFramebufferScale = new((float)displayW / w, (float)displayH / h);
            }

            if (bd->WantUpdateMonitors)
            {
                UpdateMonitors();
            }

            io.DeltaTime = Time.Delta;

            if (bd->MouseLastLeaveFrame != 0 && bd->MouseLastLeaveFrame >= ImGui.GetFrameCount() && bd->MouseButtonsDown == 0)
            {
                bd->MouseWindowID = 0;
                bd->MouseLastLeaveFrame = 0;
                io.AddMousePosEvent(-float.MaxValue, -float.MaxValue);
            }

            var mouseCursor = ImGui.GetIO().MouseDrawCursor ? ImGuiMouseCursor.None : ImGui.GetMouseCursor();

            if (bd->MouseCanReportHoveredViewport && ImGui.GetDragDropPayload().IsNull)
            {
                io.BackendFlags |= ImGuiBackendFlags.HasMouseHoveredViewport;
            }
            else
            {
                io.BackendFlags &= ~ImGuiBackendFlags.HasMouseHoveredViewport;
            }

            UpdateMouseData();
            UpdateMouseCursor();

            // Update game controllers (if enabled and available)
            UpdateGamepads();
        }

        //--------------------------------------------------------------------------------------------------------
        // MULTI-VIEWPORT / PLATFORM INTERFACE SUPPORT
        // This is an _advanced_ and _optional_ feature, allowing the backend to create and handle multiple viewports simultaneously.
        // If you are new to dear imgui or creating a new binding for dear imgui, it is recommended that you completely ignore this section first..
        //--------------------------------------------------------------------------------------------------------

        // Helper structure we store in the void* RendererUserData field of each ImGuiViewport to easily retrieve our backend data.
        private unsafe struct ViewportData
        {
            public SDLWindow* Window;
            public uint WindowID;
            public bool WindowOwned;
            public void* GLContext;
        }

        private static unsafe void CreateWindow(ImGuiViewport* viewport)
        {
            BackendData* bd = GetBackendData();
            ViewportData* vd = AllocT<ViewportData>();
            ZeroMemoryT(vd);
            viewport->PlatformUserData = vd;

            ImGuiViewport* main_viewport = ImGui.GetMainViewport();
            ViewportData* main_viewport_data = (ViewportData*)main_viewport->PlatformUserData;

            // Share GL resources with main context
            bool use_opengl = main_viewport_data->GLContext != null;
            void* backup_context = null;
            if (use_opengl)
            {
                backup_context = SdlCheckError((void*)SDL.GLGetCurrentContext().Handle);
                SDL.GLSetAttribute(SDLGLattr.GlShareWithCurrentContext, 1).SdlThrowIfNeg();
                SDL.GLMakeCurrent(main_viewport_data->Window, new((nint)main_viewport_data->GLContext)).SdlThrowIfNeg();
            }

            SDLWindowFlags sdl_flags = 0;
            sdl_flags |= use_opengl ? SDLWindowFlags.Opengl : bd->UseVulkan ? SDLWindowFlags.Vulkan : 0;
            sdl_flags |= (SDLWindowFlags)SDL.GetWindowFlags(bd->Window) & SDLWindowFlags.AllowHighdpi;
            sdl_flags |= SDLWindowFlags.Hidden;
            sdl_flags |= (viewport->Flags & ImGuiViewportFlags.NoDecoration) != 0 ? SDLWindowFlags.Borderless : 0;
            sdl_flags |= (viewport->Flags & ImGuiViewportFlags.NoDecoration) != 0 ? 0 : SDLWindowFlags.Resizable;
            sdl_flags |= (viewport->Flags & ImGuiViewportFlags.NoTaskBarIcon) != 0 ? SDLWindowFlags.SkipTaskbar : 0;
            sdl_flags |= (viewport->Flags & ImGuiViewportFlags.TopMost) != 0 ? SDLWindowFlags.AlwaysOnTop : 0;

            vd->Window = SdlCheckError(SDL.CreateWindow("No Title Yet", (int)viewport->Pos.X, (int)viewport->Pos.Y, (int)viewport->Size.X, (int)viewport->Size.Y, (uint)sdl_flags));
            vd->WindowOwned = true;
            if (use_opengl)
            {
                vd->GLContext = SdlCheckError((void*)SDL.GLCreateContext(vd->Window).Handle);
                SDL.GLSetSwapInterval(0).SdlThrowIfNeg();
            }
            if (use_opengl && backup_context != null)
            {
                SDL.GLMakeCurrent(vd->Window, new((nint)backup_context)).SdlThrowIfNeg();
            }

            viewport->PlatformHandle = vd->Window;
            viewport->PlatformHandleRaw = null;
            var sdl_backend = SDL.GetCurrentVideoDriverS();
            SDLSysWMInfo info;
            SDL.GetVersion(&info.Version);
            if (SDL.GetWindowWMInfo(vd->Window, &info) == SDLBool.True)
            {
                if (sdl_backend == "windows")
                {
                    main_viewport->PlatformHandleRaw = (void*)info.Info.Win.Window;
                }
                else if (sdl_backend == "cocoa")
                {
                    main_viewport->PlatformHandleRaw = (void*)info.Info.Cocoa.Window;
                }
            }
            else
            {
                SdlCheckError();
            }
        }

        private static unsafe void DestroyWindow(ImGuiViewport* viewport)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            if (vd != null)
            {
                if (vd->GLContext != null && vd->WindowOwned)
                {
                    SDL.GLDeleteContext(new((nint)vd->GLContext));
                }

                if (vd->Window != null && vd->WindowOwned)
                {
                    SDL.DestroyWindow(vd->Window);
                }

                SdlCheckError();
                vd->GLContext = null;
                vd->Window = null;
                Free(vd);
            }
            viewport->PlatformUserData = viewport->PlatformHandle = null;
        }

        private static unsafe void ShowWindow(ImGuiViewport* viewport)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            SDL.ShowWindow(vd->Window);
        }

        private static unsafe Vector2* GetWindowPos(Vector2* size, ImGuiViewport* viewport)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            int x = 0, y = 0;
            SDL.GetWindowPosition(vd->Window, &x, &y);
            *size = new Vector2(x, y);
            return size;
        }

        private static unsafe void SetWindowPos(ImGuiViewport* viewport, Vector2 pos)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            SDL.SetWindowPosition(vd->Window, (int)pos.X, (int)pos.Y);
        }

        private static unsafe Vector2* GetWindowSize(Vector2* size, ImGuiViewport* viewport)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            int w = 0, h = 0;
            SDL.GetWindowSize(vd->Window, &w, &h);
            *size = new Vector2(w, h);
            return size;
        }

        private static unsafe void SetWindowSize(ImGuiViewport* viewport, Vector2 size)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            SDL.SetWindowSize(vd->Window, (int)size.X, (int)size.Y);
        }

        private static unsafe void SetWindowTitle(ImGuiViewport* viewport, byte* title)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            SDL.SetWindowTitle(vd->Window, title);
        }

        private static unsafe void SetWindowAlpha(ImGuiViewport* viewport, float alpha)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            SDL.SetWindowOpacity(vd->Window, alpha);
        }

        private static unsafe void SetWindowFocus(ImGuiViewport* viewport)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            SDL.RaiseWindow(vd->Window);
        }

        private static unsafe byte GetWindowFocus(ImGuiViewport* viewport)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            var focused = ((SDLWindowFlags)SDL.GetWindowFlags(vd->Window) & SDLWindowFlags.InputFocus) != 0;
            return (byte)(focused ? 1 : 0);
        }

        private static unsafe byte GetWindowMinimized(ImGuiViewport* viewport)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            var minimized = ((SDLWindowFlags)SDL.GetWindowFlags(vd->Window) & SDLWindowFlags.Minimized) != 0;
            return (byte)(minimized ? 1 : 0);
        }

        private static unsafe void RenderWindow(ImGuiViewport* viewport, void* unknown)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            if (vd->GLContext != null)
            {
                SDL.GLMakeCurrent(vd->Window, new((nint)vd->GLContext)).SdlThrowIfNeg();
            }
        }

        private static unsafe void SwapBuffers(ImGuiViewport* viewport, void* unknown)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            if (vd->GLContext != null)
            {
                SDL.GLMakeCurrent(vd->Window, new((nint)vd->GLContext)).SdlThrowIfNeg();
                SDL.GLSwapWindow(vd->Window);
            }
        }

        private static unsafe int CreateVkSurface(ImGuiViewport* viewport, ulong vk_instance, void* vk_allocator, ulong* out_vk_surface)
        {
            ViewportData* vd = (ViewportData*)viewport->PlatformUserData;
            SDLBool ret = SDL.VulkanCreateSurface(vd->Window, *(VkInstance*)&vk_instance, (VkSurfaceKHR*)out_vk_surface);
            return ret == SDLBool.True ? 0 : 1; // ret ? VK_SUCCESS : VK_NOT_READY
        }

        private static unsafe void InitPlatformInterface(SDLWindow* window, void* sdl_gl_context)
        {
            ImGuiPlatformIO* platform_io = ImGui.GetPlatformIO();

            platform_io->PlatformCreateWindow = (void*)Marshal.GetFunctionPointerForDelegate<PlatformCreateWindow>(CreateWindow);
            platform_io->PlatformDestroyWindow = (void*)Marshal.GetFunctionPointerForDelegate<PlatformDestroyWindow>(DestroyWindow);
            platform_io->PlatformShowWindow = (void*)Marshal.GetFunctionPointerForDelegate<PlatformShowWindow>(ShowWindow);
            platform_io->PlatformSetWindowPos = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowPos>(SetWindowPos);
            platform_io->PlatformGetWindowPos = (void*)Marshal.GetFunctionPointerForDelegate<PlatformGetWindowPos>(GetWindowPos);
            platform_io->PlatformSetWindowSize = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowSize>(SetWindowSize);
            platform_io->PlatformGetWindowSize = (void*)Marshal.GetFunctionPointerForDelegate<PlatformGetWindowSize>(GetWindowSize);
            platform_io->PlatformSetWindowFocus = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowFocus>(DestroyWindow);
            platform_io->PlatformGetWindowFocus = (void*)Marshal.GetFunctionPointerForDelegate<PlatformGetWindowFocus>(GetWindowFocus);
            platform_io->PlatformGetWindowMinimized = (void*)Marshal.GetFunctionPointerForDelegate<PlatformGetWindowMinimized>(GetWindowMinimized);
            platform_io->PlatformSetWindowTitle = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowTitle>(SetWindowTitle);
            platform_io->PlatformRenderWindow = (void*)Marshal.GetFunctionPointerForDelegate<PlatformRenderWindow>(RenderWindow);
            platform_io->PlatformSwapBuffers = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSwapBuffers>(SwapBuffers);
            platform_io->PlatformSetWindowAlpha = (void*)Marshal.GetFunctionPointerForDelegate<PlatformSetWindowAlpha>(SetWindowAlpha);
            platform_io->PlatformCreateVkSurface = (void*)Marshal.GetFunctionPointerForDelegate<PlatformCreateVkSurface>(CreateVkSurface);

            ImGuiViewport* main_viewport = ImGui.GetMainViewport().Handle;
            ViewportData* vd = AllocT<ViewportData>();
            ZeroMemoryT(vd);
            vd->Window = window;
            vd->WindowID = SDL.GetWindowID(window).SdlThrowIf();
            vd->WindowOwned = false;
            vd->GLContext = sdl_gl_context;
            main_viewport->PlatformUserData = vd;
            main_viewport->PlatformHandle = vd->Window;
        }

        private static void ShutdownPlatformInterface()
        {
            ImGui.DestroyPlatformWindows();
        }
    }
}