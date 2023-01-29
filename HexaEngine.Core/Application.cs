﻿namespace HexaEngine.Core
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Windows;
    using Silk.NET.SDL;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static unsafe class Application
    {
        private static bool exiting = false;
        private static readonly Dictionary<uint, SdlWindow> windows = new();
        private static readonly Sdl sdl = Sdl.GetApi();
        private static readonly List<Action<Event>> hooks = new();
        private static IRenderWindow? mainWindow;
        private static bool inDesignMode;
        private static bool inEditorMode;

#nullable disable
        public static IRenderWindow MainWindow => mainWindow;
#nullable enable

        public enum SpecialFolder
        {
            Assets,
            Shaders,
            Sounds,
            Models,
            Textures,
            Scenes,
        }

        public static bool InDesignMode
        {
            get => inDesignMode; set
            {
                inDesignMode = value;
                OnDesignModeChanged?.Invoke(value);
            }
        }

        public static bool InEditorMode
        {
            get => inEditorMode; set
            {
                inEditorMode = value;
                OnEditorModeChanged?.Invoke(value);
            }
        }

        public static event Action<bool>? OnDesignModeChanged;

        public static event Action<bool>? OnEditorModeChanged;

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

        public static void Run(IRenderWindow mainWindow)
        {
            sdl.Init(Sdl.InitEvents);
            sdl.SetHint(Sdl.HintMouseFocusClickthrough, "1");
            Application.mainWindow = mainWindow;
            mainWindow.Closing += MainWindow_Closing;
            Keyboard.Init(sdl);
            Mouse.Init(sdl);
            mainWindow.Show();
            PlatformRun();
        }

        internal static void RegisterWindow(SdlWindow window)
        {
            windows.Add(window.WindowID, window);
        }

        private static void MainWindow_Closing(object? sender, Events.CloseEventArgs e)
        {
            if (!e.Handled)
                exiting = true;
        }

        public static void RegisterHook(Action<Event> hook)
        {
            hooks.Add(hook);
        }

        private static void PlatformRun()
        {
            Event evnt;
            Time.Initialize();
            mainWindow.RenderInitialize();

            while (!exiting)
            {
                sdl.PumpEvents();
                while (sdl.PollEvent(&evnt) == (int)SdlBool.True)
                {
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
                                SdlWindow window = windows[evnt.Window.WindowID];
                                window.ProcessEvent(evnt.Window);
                                if ((WindowEventID)evnt.Window.Event == WindowEventID.Close && window == mainWindow)
                                {
                                    exiting = true;
                                }
                            }
                            break;

                        case EventType.Syswmevent:
                            break;

                        case EventType.Keydown:
                            {
                                var even = evnt.Key;
                                if (windows.TryGetValue(even.WindowID, out var window))
                                {
                                    window.ProcessInputKeyboard(even);
                                    Keyboard.Enqueue(even);
                                }
                            }
                            break;

                        case EventType.Keyup:
                            {
                                var even = evnt.Key;
                                if (windows.TryGetValue(even.WindowID, out var window))
                                {
                                    window.ProcessInputKeyboard(even);
                                    Keyboard.Enqueue(even);
                                }
                            }
                            break;

                        case EventType.Textediting:
                            break;

                        case EventType.Textinput:
                            {
                                var even = evnt.Text;
                                if (windows.TryGetValue(even.WindowID, out var window))
                                {
                                    window.ProcessInputText(even);
                                    Keyboard.Enqueue(even);
                                }
                            }
                            break;

                        case EventType.Keymapchanged:
                            break;

                        case EventType.Mousemotion:
                            {
                                var even = evnt.Motion;
                                if (windows.TryGetValue(even.WindowID, out var window))
                                {
                                    window.ProcessInputMouse(even);
                                    Mouse.Enqueue(even);
                                }
                            }
                            break;

                        case EventType.Mousebuttondown:
                            {
                                var even = evnt.Button;
                                if (windows.TryGetValue(even.WindowID, out var window))
                                {
                                    window.ProcessInputMouse(even);
                                    Mouse.Enqueue(even);
                                }
                            }
                            break;

                        case EventType.Mousebuttonup:
                            {
                                var even = evnt.Button;
                                if (windows.TryGetValue(even.WindowID, out var window))
                                {
                                    window.ProcessInputMouse(even);
                                    Mouse.Enqueue(even);
                                }
                            }
                            break;

                        case EventType.Mousewheel:
                            {
                                var even = evnt.Wheel;
                                if (windows.TryGetValue(even.WindowID, out var window))
                                {
                                    window.ProcessInputMouse(even);
                                    Mouse.Enqueue(even);
                                }
                            }
                            break;

                        case EventType.Joyaxismotion:
                            break;

                        case EventType.Joyballmotion:
                            break;

                        case EventType.Joyhatmotion:
                            break;

                        case EventType.Joybuttondown:
                            break;

                        case EventType.Joybuttonup:
                            break;

                        case EventType.Joydeviceadded:
                            break;

                        case EventType.Joydeviceremoved:
                            break;

                        case EventType.Controlleraxismotion:
                            break;

                        case EventType.Controllerbuttondown:
                            break;

                        case EventType.Controllerbuttonup:
                            break;

                        case EventType.Controllerdeviceadded:
                            break;

                        case EventType.Controllerdeviceremoved:
                            break;

                        case EventType.Controllerdeviceremapped:
                            break;

                        case EventType.Controllertouchpaddown:
                            break;

                        case EventType.Controllertouchpadmotion:
                            break;

                        case EventType.Controllertouchpadup:
                            break;

                        case EventType.Controllersensorupdate:
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

                    for (int i = 0; i < hooks.Count; i++)
                        hooks[i](evnt);
                }

                mainWindow.Render();

                mainWindow.ClearState();

                Time.FrameUpdate();
            }
            mainWindow.RenderDispose();

            sdl.Quit();
        }
    }
}