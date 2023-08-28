namespace HexaEngine.Core
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using Silk.NET.SDL;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static unsafe class Application
    {
        internal static readonly Sdl sdl = Sdl.GetApi();

        private static bool initialized = false;
        private static bool exiting = false;
        private static readonly Dictionary<uint, IRenderWindow> windowIdToWindow = new();
        private static readonly List<IRenderWindow> windows = new();
        private static readonly List<Func<Event, bool>> hooks = new();
        private static IRenderWindow? mainWindow;
        private static bool inDesignMode;
        private static bool inEditorMode;

        private static IGraphicsDevice graphicsDevice;
        private static IGraphicsContext graphicsContext;

        private static IAudioDevice audioDevice;

        /// <summary>
        /// Gets the main window of the application.
        /// </summary>
        public static IRenderWindow MainWindow => mainWindow;

        /// <summary>
        /// Gets the graphics device used by the application.
        /// </summary>
        public static IGraphicsDevice GraphicsDevice => graphicsDevice;

        /// <summary>
        /// Gets the graphics context used by the application.
        /// </summary>
        public static IGraphicsContext GraphicsContext => graphicsContext;

        public static GraphicsBackend GraphicsBackend
        {
            get; set;
        }

        public enum SpecialFolder
        {
            Assets,
            Shaders,
            Sounds,
            Models,
            Textures,
            Scenes,
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is in design mode.
        /// </summary>
        public static bool InDesignMode
        {
            get => inDesignMode; set
            {
                inDesignMode = value;
                OnDesignModeChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is in editor mode.
        /// </summary>
        public static bool InEditorMode
        {
            get => inEditorMode; set
            {
                inEditorMode = value;
                OnEditorModeChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether graphics debugging is enabled.
        /// </summary>
        public static bool GraphicsDebugging { get; set; }

        /// <summary>
        /// Occurs when the design mode state of the application changes.
        /// </summary>
        public static event Action<bool>? OnDesignModeChanged;

        /// <summary>
        /// Occurs when the editor mode state of the application changes.
        /// </summary>
        public static event Action<bool>? OnEditorModeChanged;

        /// <summary>
        /// Gets the folder path for the specified special folder.
        /// </summary>
        /// <param name="folder">The special folder.</param>
        /// <returns>The folder path.</returns>
        public static string GetFolder(SpecialFolder folder)
        {
            return folder switch
            {
                SpecialFolder.Assets => Path.GetFullPath("assets/"),
                SpecialFolder.Shaders => Path.GetFullPath("assets/shaders/"),
                SpecialFolder.Sounds => Path.GetFullPath("assets/sounds/"),
                SpecialFolder.Models => Path.GetFullPath("assets/models/"),
                SpecialFolder.Textures => Path.GetFullPath("assets/textures/"),
                SpecialFolder.Scenes => Path.GetFullPath("assets/scenes/"),
                _ => throw new ArgumentOutOfRangeException(nameof(folder)),
            };
        }

        /// <summary>
        /// Initializes the application and necessary subsystems.
        /// </summary>
        public static void Boot()
        {
            CrashLogger.Initialize();
            ImGuiConsole.Initialize();

            sdl.SetHint(Sdl.HintMouseFocusClickthrough, "1");
            sdl.SetHint(Sdl.HintAutoUpdateJoysticks, "1");
            sdl.SetHint(Sdl.HintJoystickHidapiPS4, "1");
            sdl.SetHint(Sdl.HintJoystickHidapiPS4Rumble, "1");
            sdl.SetHint(Sdl.HintJoystickRawinput, "0");
            sdl.Init(Sdl.InitEvents + Sdl.InitGamecontroller + Sdl.InitHaptic + Sdl.InitJoystick + Sdl.InitSensor);

            SdlCheckError();

            Keyboard.Init();
            SdlCheckError();
            Mouse.Init();
            SdlCheckError();
            Gamepads.Init();
            SdlCheckError();
            TouchDevices.Init();
            SdlCheckError();
        }

        /// <summary>
        /// Runs the application with the specified main window.
        /// </summary>
        /// <param name="mainWindow">The main window of the application.</param>
        public static void Run(IRenderWindow mainWindow)
        {
            Application.mainWindow = mainWindow;
            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            mainWindow.Show();
            Init();
            mainWindow.Closing += MainWindowClosing;

            PlatformRun();
        }

        /// <summary>
        /// Registers a hook function that will be invoked for each event received by the application.
        /// </summary>
        /// <param name="hook">The hook function to register.</param>
        public static void RegisterHook(Func<Event, bool> hook)
        {
            hooks.Add(hook);
        }

        /// <summary>
        /// Unregisters a previously registered hook function from the application.
        /// </summary>
        /// <param name="hook">The hook function to unregister.</param>
        public static void UnregisterHook(Func<Event, bool> hook)
        {
            hooks.Remove(hook);
        }

        private static void Init()
        {
            graphicsDevice = GraphicsAdapter.CreateGraphicsDevice(GraphicsBackend, GraphicsDebugging);
            graphicsContext = graphicsDevice.Context;
            audioDevice = AudioAdapter.CreateAudioDevice(AudioBackend.Auto, null);

            ResourceManager2.Shared = new(graphicsDevice);
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].Initialize(audioDevice, graphicsDevice);
            }

            initialized = true;
        }

        /// <summary>
        /// Registers a window to the application.
        /// </summary>
        /// <param name="window">The window to register.</param>
        internal static void RegisterWindow(IRenderWindow window)
        {
            windows.Add(window);
            windowIdToWindow.Add(window.WindowID, window);
            if (initialized)
            {
                window.Initialize(audioDevice, graphicsDevice);
            }
        }

        private static void MainWindowClosing(object? sender, CloseEventArgs e)
        {
            if (!e.Handled)
            {
                exiting = true;
            }
        }

        /// <summary>
        /// The main loop of the application.
        /// </summary>
        private static void PlatformRun()
        {
            Event evnt;
            Time.Initialize();

            while (!exiting)
            {
                sdl.PumpEvents();
                while (sdl.PollEvent(&evnt) == (int)SdlBool.True)
                {
                    for (int i = 0; i < hooks.Count; i++)
                    {
                        hooks[i](evnt);
                    }
                    EventType type = (EventType)evnt.Type;
                    switch (type)
                    {
                        case EventType.Firstevent:
                            break;

                        case EventType.Quit:
                            exiting = true;
                            break;

                        case EventType.AppTerminating:
                            exiting = true;
                            break;

                        case EventType.AppLowmemory:
                            break;

                        case EventType.AppWillenterbackground:
                            break;

                        case EventType.AppDidenterbackground:
                            break;

                        case EventType.AppWillenterforeground:
                            break;

                        case EventType.AppDidenterforeground:
                            break;

                        case EventType.Localechanged:
                            break;

                        case EventType.Displayevent:
                            break;

                        case EventType.Windowevent:
                            {
                                var even = evnt.Window;
                                if (even.WindowID == mainWindow.WindowID)
                                {
                                    ((SdlWindow)mainWindow).ProcessEvent(even);
                                    if ((WindowEventID)evnt.Window.Event == WindowEventID.Close)
                                    {
                                        exiting = true;
                                    }
                                }
                            }

                            break;

                        case EventType.Syswmevent:
                            break;

                        case EventType.Keydown:
                            {
                                var even = evnt.Key;
                                Keyboard.OnKeyDown(even);
                                if (even.WindowID == mainWindow.WindowID)
                                    ((SdlWindow)mainWindow).ProcessInputKeyboard(even);
                            }
                            break;

                        case EventType.Keyup:
                            {
                                var even = evnt.Key;
                                Keyboard.OnKeyUp(even);
                                if (even.WindowID == mainWindow.WindowID)
                                    ((SdlWindow)mainWindow).ProcessInputKeyboard(even);
                            }
                            break;

                        case EventType.Textediting:
                            break;

                        case EventType.Textinput:
                            {
                                var even = evnt.Text;
                                Keyboard.OnTextInput(even);
                                if (even.WindowID == mainWindow.WindowID)
                                    ((SdlWindow)mainWindow).ProcessInputText(even);
                            }
                            break;

                        case EventType.Keymapchanged:
                            break;

                        case EventType.Mousemotion:
                            {
                                var even = evnt.Motion;
                                Mouse.OnMotion(even);
                                if (even.WindowID == mainWindow.WindowID)
                                    ((SdlWindow)mainWindow).ProcessInputMouse(even);
                            }
                            break;

                        case EventType.Mousebuttondown:
                            {
                                var even = evnt.Button;
                                Mouse.OnButtonDown(even);
                                if (even.WindowID == mainWindow.WindowID)
                                    ((SdlWindow)mainWindow).ProcessInputMouse(even);
                            }
                            break;

                        case EventType.Mousebuttonup:
                            {
                                var even = evnt.Button;
                                Mouse.OnButtonUp(even);
                                if (even.WindowID == mainWindow.WindowID)
                                    ((SdlWindow)mainWindow).ProcessInputMouse(even);
                            }
                            break;

                        case EventType.Mousewheel:
                            {
                                var even = evnt.Wheel;
                                Mouse.OnWheel(even);
                                if (even.WindowID == mainWindow.WindowID)
                                    ((SdlWindow)mainWindow).ProcessInputMouse(even);
                            }
                            break;

                        case EventType.Joyaxismotion:
                            {
                                var even = evnt.Jaxis;
                                Joysticks.OnAxisMotion(even);
                            }
                            break;

                        case EventType.Joyballmotion:
                            {
                                var even = evnt.Jball;
                                Joysticks.OnBallMotion(even);
                            }
                            break;

                        case EventType.Joyhatmotion:
                            {
                                var even = evnt.Jhat;
                                Joysticks.OnHatMotion(even);
                            }
                            break;

                        case EventType.Joybuttondown:
                            {
                                var even = evnt.Jbutton;
                                Joysticks.OnButtonDown(even);
                            }
                            break;

                        case EventType.Joybuttonup:
                            {
                                var even = evnt.Jbutton;
                                Joysticks.OnButtonUp(even);
                            }
                            break;

                        case EventType.Joydeviceadded:
                            {
                                var even = evnt.Jdevice;
                                Joysticks.AddJoystick(even);
                            }
                            break;

                        case EventType.Joydeviceremoved:
                            {
                                var even = evnt.Jdevice;
                                Joysticks.RemoveJoystick(even);
                            }
                            break;

                        case EventType.Controlleraxismotion:
                            {
                                var even = evnt.Caxis;
                                Gamepads.AxisMotion(even);
                            }
                            break;

                        case EventType.Controllerbuttondown:
                            {
                                var even = evnt.Cbutton;
                                Gamepads.ButtonDown(even);
                            }
                            break;

                        case EventType.Controllerbuttonup:
                            {
                                var even = evnt.Cbutton;
                                Gamepads.ButtonUp(even);
                            }
                            break;

                        case EventType.Controllerdeviceadded:
                            {
                                var even = evnt.Cdevice;
                                Gamepads.AddController(even);
                            }
                            break;

                        case EventType.Controllerdeviceremoved:
                            {
                                var even = evnt.Cdevice;
                                Gamepads.RemoveController(even);
                            }
                            break;

                        case EventType.Controllerdeviceremapped:
                            {
                                var even = evnt.Cdevice;
                                Gamepads.Remapped(even);
                            }
                            break;

                        case EventType.Controllertouchpaddown:
                            {
                                var even = evnt.Ctouchpad;
                                Gamepads.TouchPadDown(even);
                            }
                            break;

                        case EventType.Controllertouchpadmotion:
                            {
                                var even = evnt.Ctouchpad;
                                Gamepads.TouchPadMotion(even);
                            }
                            break;

                        case EventType.Controllertouchpadup:
                            {
                                var even = evnt.Ctouchpad;
                                Gamepads.TouchPadUp(even);
                            }
                            break;

                        case EventType.Controllersensorupdate:
                            {
                                var even = evnt.Csensor;
                                Gamepads.SensorUpdate(even);
                            }
                            break;

                        case EventType.Fingerdown:
                            break;

                        case EventType.Fingerup:
                            break;

                        case EventType.Fingermotion:
                            break;

                        case EventType.Dollargesture:
                            break;

                        case EventType.Dollarrecord:
                            break;

                        case EventType.Multigesture:
                            break;

                        case EventType.Clipboardupdate:
                            break;

                        case EventType.Dropfile:
                            break;

                        case EventType.Droptext:
                            break;

                        case EventType.Dropbegin:
                            break;

                        case EventType.Dropcomplete:
                            break;

                        case EventType.Audiodeviceadded:
                            break;

                        case EventType.Audiodeviceremoved:
                            break;

                        case EventType.Sensorupdate:
                            break;

                        case EventType.RenderTargetsReset:
                            break;

                        case EventType.RenderDeviceReset:
                            break;

                        case EventType.Userevent:
                            break;

                        case EventType.Lastevent:
                            break;
                    }
                }

                mainWindow.Render(graphicsContext);
                mainWindow.ClearState();
                GraphicsAdapter.Current.PumpDebugMessages();
                Time.FrameUpdate();
            }
            ResourceManager2.Shared.Dispose();
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].Uninitialize();
            }

            ((SdlWindow)mainWindow).DestroyWindow();

            SdlCheckError();
            sdl.Quit();
            //SdlCheckError();
        }
    }
}