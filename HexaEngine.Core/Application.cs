﻿namespace HexaEngine.Core
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using Hexa.NET.SDL2;
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Logging;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using System.Collections.Generic;
    using System.Diagnostics;
    using static Extensions.SdlErrorHandlingExtensions;

    /// <summary>
    /// Provides data for the EditorPlayStateTransition event.
    /// </summary>
    public class EditorPlayStateTransitionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorPlayStateTransitionEventArgs"/> class with the specified new state.
        /// </summary>
        /// <param name="newState">The new state of the editor.</param>
        public EditorPlayStateTransitionEventArgs(EditorPlayState newState)
        {
            NewState = newState;
        }

        /// <summary>
        /// Gets the new state of the editor.
        /// </summary>
        public EditorPlayState NewState { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the play state transition should be canceled.
        /// </summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Provides functionality for managing and running the application.
    /// </summary>
    public static unsafe class Application
    {
        private static bool initialized = false;
        private static bool exiting = false;
        private static bool supressQuitApp = false;
        private static readonly Dictionary<uint, ICoreWindow> windowIdToWindow = [];
        private static readonly List<ICoreWindow> windows = [];
        private static readonly List<Func<SDLEvent, bool>> hooks = [];

        private static bool inEditorMode;
        private static EditorPlayState editorPlayState;

#nullable disable
        private static ICoreWindow mainWindow;
        private static IGraphicsDevice graphicsDevice;
        private static IGraphicsContext graphicsContext;
        private static IGPUProfiler gpuProfiler;
        private static IAudioDevice audioDevice;

#nullable restore

        /// <summary>
        /// Gets the main window of the application.
        /// </summary>
        public static ICoreWindow MainWindow => mainWindow;

        /// <summary>
        /// Gets the graphics device used by the application.
        /// </summary>
        public static IGraphicsDevice GraphicsDevice => graphicsDevice;

        /// <summary>
        /// Gets the graphics context used by the application.
        /// </summary>
        public static IGraphicsContext GraphicsContext => graphicsContext;

        /// <summary>
        /// Gets the graphics context used by the application.
        /// </summary>
        public static IGPUProfiler GPUProfiler => gpuProfiler;

        /// <summary>
        /// Gets or sets the graphics backend used by the application.
        /// </summary>
        public static GraphicsBackend GraphicsBackend
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets the audio backend used by the application.
        /// </summary>
        public static AudioBackend AudioBackend
        {
            get; private set;
        }

        /// <summary>
        /// Represents special folders used by the application.
        /// </summary>
        public enum SpecialFolder
        {
            /// <summary>
            /// The assets folder <see cref="Paths.CurrentAssetsPath"/>
            /// </summary>
            Assets,

            /// <summary>
            /// The shaders folder <see cref="Paths.CurrentShaderPath"/>
            /// </summary>
            Shaders,

            /// <summary>
            /// The sounds folder <see cref="Paths.CurrentSoundPath"/>
            /// </summary>
            Sounds,

            /// <summary>
            /// The models folder <see cref="Paths.CurrentMeshesPath"/>
            /// </summary>
            Models,

            /// <summary>
            /// The textures folder <see cref="Paths.CurrentTexturePath"/>
            /// </summary>
            Textures,

            /// <summary>
            /// The scenes folder <see cref="Paths.CurrentAssetsPath"/>
            /// </summary>
            Scenes,
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is in editor mode.
        /// </summary>
        public static bool InEditorMode
        {
            get => inEditorMode;
            set
            {
                inEditorMode = value;
                OnEditorModeChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the editor is in edit mode.
        /// </summary>
        public static bool InEditMode
        {
            get => editorPlayState == EditorPlayState.Edit;
        }

        /// <summary>
        /// Gets a value indicating whether the editor is in play mode.
        /// </summary>
        public static bool InPlayMode
        {
            get => editorPlayState == EditorPlayState.Play;
        }

        /// <summary>
        /// Gets a value indicating whether the editor is in pause mode.
        /// </summary>
        public static bool InPauseState
        {
            get => editorPlayState == EditorPlayState.Pause;
        }

        /// <summary>
        /// Gets or sets the current editor play state.
        /// </summary>
        public static EditorPlayState EditorPlayState
        {
            get => editorPlayState;
            set
            {
                if (editorPlayState == value)
                {
                    return;
                }

                editorPlayState = value;
                OnEditorPlayStateChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether graphics debugging is enabled.
        /// </summary>
        public static bool GraphicsDebugging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether graphics is disabled.
        /// </summary>
        public static bool GraphicsDisabled => GraphicsBackend == GraphicsBackend.Disabled;

        /// <summary>
        /// Occurs when the editor mode state of the application changes.
        /// </summary>
        public static event Action<bool>? OnEditorModeChanged;

        /// <summary>
        /// Occurs when the application shuts down.
        /// </summary>
        public static event Action? OnApplicationClose;

        /// <summary>
        /// Represents the method that will handle a transition in the editor play state event.
        /// </summary>
        /// <param name="args">The arguments associated with the transition.</param>
        public delegate void EditorPlayStateTransitionEventHandler(EditorPlayStateTransitionEventArgs args);

        /// <summary>
        /// Represents the method that will handle a cancellation of the editor play state event.
        /// </summary>
        /// <param name="restoreState">The editor play state to restore if the cancellation is handled.</param>
        public delegate void EditorPlayStateCancelEventHandler(EditorPlayState restoreState);

        /// <summary>
        /// Represents the method that will handle a change in the editor play state event.
        /// </summary>
        /// <param name="newState">The new editor play state.</param>
        public delegate void EditorPlayStateChangedEventHandler(EditorPlayState newState);

        /// <summary>
        /// Event triggered when there's a transition in the editor play state.
        /// </summary>
        public static event EditorPlayStateTransitionEventHandler? OnEditorPlayStateTransition;

        /// <summary>
        /// Event triggered when the editor play state cancellation is requested.
        /// </summary>
        public static event EditorPlayStateCancelEventHandler? OnEditorPlayStateCancel;

        /// <summary>
        /// Event triggered when the editor play state changes.
        /// </summary>
        public static event EditorPlayStateChangedEventHandler? OnEditorPlayStateChanged;

        /// <summary>
        /// Notifies subscribers about a transition in the editor play state and returns whether the transition should be canceled.
        /// </summary>
        /// <param name="state">The new editor play state.</param>
        /// <returns><c>true</c> if the transition should be canceled, otherwise <c>false</c>.</returns>
        public static bool NotifyEditorPlayStateTransition(EditorPlayState state)
        {
            EditorPlayStateTransitionEventArgs eventArgs = new(state);
            OnEditorPlayStateTransition?.Invoke(eventArgs);

            if (eventArgs.Cancel)
            {
                OnEditorPlayStateCancel?.Invoke(editorPlayState);
            }

            return eventArgs.Cancel;
        }

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
        /// Gets the file log writer used for logging.
        /// </summary>
        public static LogFileWriter? LogFileWriter { get; private set; }

        /// <summary>
        /// Initializes the application and necessary subsystems.
        /// </summary>
        public static void Boot(GraphicsBackend graphicsBackend, AudioBackend audioBackend, bool disableLogging = false)
        {
            GraphicsBackend = graphicsBackend;
            AudioBackend = audioBackend;

#if DEBUG
            GraphicsDebugging = true;
#endif
            if (!disableLogging)
            {
                CrashLogger.Initialize();
                LogFileWriter = new("logs");
                LoggerFactory.AddGlobalWriter(LogFileWriter);
            }

            FileSystem.Initialize();
            ImGuiConsole.Initialize();

            SDL.SetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");
            SDL.SetHint(SDL.SDL_HINT_AUTO_UPDATE_JOYSTICKS, "1");
            SDL.SetHint(SDL.SDL_HINT_JOYSTICK_HIDAPI_PS4, "1");//HintJoystickHidapiPS4
            SDL.SetHint(SDL.SDL_HINT_JOYSTICK_HIDAPI_PS4_RUMBLE, "1"); //HintJoystickHidapiPS4Rumble
            SDL.SetHint(SDL.SDL_HINT_JOYSTICK_RAWINPUT, "0");
            SDL.SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1"); //HintWindowsDisableThreadNaming
            SDL.SetHint(SDL.SDL_HINT_MOUSE_NORMAL_SPEED_SCALE, "1");

            SDL.Init(SDL.SDL_INIT_EVENTS + SDL.SDL_INIT_GAMECONTROLLER + SDL.SDL_INIT_HAPTIC + SDL.SDL_INIT_JOYSTICK + SDL.SDL_INIT_SENSOR);

            SdlCheckError();

            Keyboard.Init();
            SdlCheckError();
            Mouse.Init();
            SdlCheckError();
            Gamepads.Init();
            SdlCheckError();
            TouchDevices.Init();
            SdlCheckError();

            SDL.EventState((uint)SDLEventType.Dropfile, SDL.SDL_ENABLE);
        }

        /// <summary>
        /// Runs the application with the specified main window.
        /// </summary>
        /// <param name="mainWindow">The main window of the application.</param>
        public static void Run(ICoreWindow mainWindow)
        {
            RegisterWindow(mainWindow);
            Application.mainWindow = mainWindow;
            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            Init();
            mainWindow.Closed += MainWindowClosed;

            PlatformRun();
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        public static void Exit()
        {
            exiting = true;
        }

        public static void Shutdown()
        {
            audioDevice?.Dispose();
            graphicsContext?.Dispose();
            graphicsDevice?.Dispose();

            SdlCheckError();
            SDL.Quit();
        }

        /// <summary>
        /// Registers a hook function that will be invoked for each event received by the application.
        /// </summary>
        /// <param name="hook">The hook function to register.</param>
        public static void RegisterHook(Func<SDLEvent, bool> hook)
        {
            hooks.Add(hook);
        }

        /// <summary>
        /// Unregisters a previously registered hook function from the application.
        /// </summary>
        /// <param name="hook">The hook function to unregister.</param>
        public static void UnregisterHook(Func<SDLEvent, bool> hook)
        {
            hooks.Remove(hook);
        }

        public static void Init()
        {
            graphicsDevice = GraphicsAdapter.CreateGraphicsDevice(GraphicsBackend, GraphicsDebugging);
            graphicsContext = graphicsDevice.Context;
            gpuProfiler = graphicsDevice.Profiler;
            audioDevice = AudioAdapter.CreateAudioDevice(AudioBackend.Auto, null);

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
        internal static void RegisterWindow(ICoreWindow window)
        {
            if (windows.Contains(window))
            {
                return;
            }
            windows.Add(window);
            windowIdToWindow.Add(window.WindowID, window);
            if (initialized)
            {
                window.Initialize(audioDevice, graphicsDevice);
            }
        }

        /// <summary>
        /// Suppresses the quit application action. Will be automatically reset.
        /// </summary>
        internal static void SuppressQuitApp()
        {
            supressQuitApp = true;
        }

        private static void MainWindowClosed(object? sender, CloseEventArgs e)
        {
            exiting = true;
        }

        /// <summary>
        /// The main loop of the application.
        /// </summary>
        private static unsafe void PlatformRun()
        {
            SDLEvent evnt;
            Time.ResetTime();

            while (!exiting)
            {
                SDL.PumpEvents();
                while (SDL.PollEvent(&evnt) == 1)
                {
                    for (int j = 0; j < hooks.Count; j++)
                    {
                        hooks[j](evnt);
                    }

                    HandleEvent(evnt);
                }

                Mouse.Poll();

                mainWindow.Render(graphicsContext);

                GraphicsAdapter.Current.PumpDebugMessages();

                Keyboard.Flush();
                Mouse.Flush();
                Time.FrameUpdate();
            }

            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].Dispose();
            }

            ((SdlWindow)mainWindow).DestroyWindow();

            OnApplicationClose?.Invoke();

            audioDevice.Dispose();
            graphicsContext.Dispose();
            graphicsDevice.Dispose();

            SdlCheckError();
            SDL.Quit();
        }

        public static void StartTextInput()
        {
            SDL.StartTextInput();
        }

        public static void StopTextInput()
        {
            SDL.StopTextInput();
        }

        public static void SetTextInputRect(Rectangle rect)
        {
            SDLRect sdlRect = new(rect.Left, rect.Top, rect.Size.X, rect.Size.Y);
            SDL.SetTextInputRect(&sdlRect);
        }

        private static void HandleEvent(SDLEvent evnt)
        {
            SDLEventType type = (SDLEventType)evnt.Type;
            switch (type)
            {
                case SDLEventType.Firstevent:
                    break;

                case SDLEventType.Quit:
                    if (!supressQuitApp)
                    {
                        exiting = true;
                    }
                    supressQuitApp = false;
                    break;

                case SDLEventType.AppTerminating:
                    exiting = true;
                    break;

                case SDLEventType.AppLowmemory:
                    break;

                case SDLEventType.AppWillenterbackground:
                    break;

                case SDLEventType.AppDidenterbackground:
                    break;

                case SDLEventType.AppWillenterforeground:
                    break;

                case SDLEventType.AppDidenterforeground:
                    break;

                case SDLEventType.Localechanged:
                    break;

                case SDLEventType.Displayevent:
                    break;

                case SDLEventType.Windowevent:
                    {
                        var even = evnt.Window;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessEvent(even);
                        }
                    }

                    break;

                case SDLEventType.Syswmevent:
                    break;

                case SDLEventType.Keydown:
                    {
                        var even = evnt.Key;
                        Keyboard.OnKeyDown(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputKeyboard(even);
                        }
                    }
                    break;

                case SDLEventType.Keyup:
                    {
                        var even = evnt.Key;
                        Keyboard.OnKeyUp(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputKeyboard(even);
                        }
                    }
                    break;

                case SDLEventType.Textediting:
                    {
                        var even = evnt.Edit;
                        Keyboard.OnTextEditing(even);
                    }
                    break;

                case SDLEventType.Textinput:
                    {
                        var even = evnt.Text;
                        Keyboard.OnTextInput(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputText(even);
                        }
                    }
                    break;

                case SDLEventType.Keymapchanged:
                    break;

                case SDLEventType.Mousemotion:
                    {
                        var even = evnt.Motion;
                        Mouse.OnMotion(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case SDLEventType.Mousebuttondown:
                    {
                        var even = evnt.Button;
                        Mouse.OnButtonDown(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case SDLEventType.Mousebuttonup:
                    {
                        var even = evnt.Button;
                        Mouse.OnButtonUp(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case SDLEventType.Mousewheel:
                    {
                        var even = evnt.Wheel;
                        Mouse.OnWheel(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case SDLEventType.Joyaxismotion:
                    {
                        var even = evnt.Jaxis;
                        Joysticks.OnAxisMotion(even);
                    }
                    break;

                case SDLEventType.Joyballmotion:
                    {
                        var even = evnt.Jball;
                        Joysticks.OnBallMotion(even);
                    }
                    break;

                case SDLEventType.Joyhatmotion:
                    {
                        var even = evnt.Jhat;
                        Joysticks.OnHatMotion(even);
                    }
                    break;

                case SDLEventType.Joybuttondown:
                    {
                        var even = evnt.Jbutton;
                        Joysticks.OnButtonDown(even);
                    }
                    break;

                case SDLEventType.Joybuttonup:
                    {
                        var even = evnt.Jbutton;
                        Joysticks.OnButtonUp(even);
                    }
                    break;

                case SDLEventType.Joydeviceadded:
                    {
                        var even = evnt.Jdevice;
                        Joysticks.AddJoystick(even);
                    }
                    break;

                case SDLEventType.Joydeviceremoved:
                    {
                        var even = evnt.Jdevice;
                        Joysticks.RemoveJoystick(even);
                    }
                    break;

                case SDLEventType.Controlleraxismotion:
                    {
                        var even = evnt.Caxis;
                        Gamepads.OnAxisMotion(even);
                    }
                    break;

                case SDLEventType.Controllerbuttondown:
                    {
                        var even = evnt.Cbutton;
                        Gamepads.OnButtonDown(even);
                    }
                    break;

                case SDLEventType.Controllerbuttonup:
                    {
                        var even = evnt.Cbutton;
                        Gamepads.OnButtonUp(even);
                    }
                    break;

                case SDLEventType.Controllerdeviceadded:
                    {
                        var even = evnt.Cdevice;
                        Gamepads.AddController(even);
                    }
                    break;

                case SDLEventType.Controllerdeviceremoved:
                    {
                        var even = evnt.Cdevice;
                        Gamepads.RemoveController(even);
                    }
                    break;

                case SDLEventType.Controllerdeviceremapped:
                    {
                        var even = evnt.Cdevice;
                        Gamepads.OnRemapped(even);
                    }
                    break;

                case SDLEventType.Controllertouchpaddown:
                    {
                        var even = evnt.Ctouchpad;
                        Gamepads.OnTouchPadDown(even);
                    }
                    break;

                case SDLEventType.Controllertouchpadmotion:
                    {
                        var even = evnt.Ctouchpad;
                        Gamepads.OnTouchPadMotion(even);
                    }
                    break;

                case SDLEventType.Controllertouchpadup:
                    {
                        var even = evnt.Ctouchpad;
                        Gamepads.OnTouchPadUp(even);
                    }
                    break;

                case SDLEventType.Controllersensorupdate:
                    {
                        var even = evnt.Csensor;
                        Gamepads.OnSensorUpdate(even);
                    }
                    break;

                case SDLEventType.Fingerdown:
                    {
                        var even = evnt.Tfinger;
                        TouchDevices.FingerDown(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputTouchDown(even);
                        }
                    }
                    break;

                case SDLEventType.Fingerup:
                    {
                        var even = evnt.Tfinger;
                        TouchDevices.FingerUp(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputTouchUp(even);
                        }
                    }
                    break;

                case SDLEventType.Fingermotion:
                    {
                        var even = evnt.Tfinger;
                        TouchDevices.FingerMotion(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputTouchMotion(even);
                        }
                    }
                    break;

                case SDLEventType.Dollargesture:
                    break;

                case SDLEventType.Dollarrecord:
                    break;

                case SDLEventType.Multigesture:
                    break;

                case SDLEventType.Clipboardupdate:
                    break;

                case SDLEventType.Dropfile:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropFile(even);
                        }

                        SDL.Free(evnt.Drop.File);
                    }
                    break;

                case SDLEventType.Droptext:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropText(even);
                        }

                        SDL.Free(evnt.Drop.File);
                    }
                    break;

                case SDLEventType.Dropbegin:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropBegin(even);
                        }
                    }
                    break;

                case SDLEventType.Dropcomplete:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropComplete(even);
                        }
                    }
                    break;

                case SDLEventType.Audiodeviceadded:
                    break;

                case SDLEventType.Audiodeviceremoved:
                    break;

                case SDLEventType.Sensorupdate:
                    break;

                case SDLEventType.RenderTargetsReset:
                    break;

                case SDLEventType.RenderDeviceReset:
                    break;

                case SDLEventType.Userevent:
                    break;

                case SDLEventType.Lastevent:
                    break;

                default:
                    return;
            }
        }
    }
}