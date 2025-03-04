namespace HexaEngine.Core
{
    using Hexa.NET.Logging;
    using Hexa.NET.Mathematics;
    using Hexa.NET.SDL3;
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
            SDL.SetHint(SDL.SDL_HINT_JOYSTICK_RAWINPUT, "0");
            SDL.SetHint(SDL.SDL_HINT_MOUSE_NORMAL_SPEED_SCALE, "1");

            SDL.Init(SDLInitFlags.Events | SDLInitFlags.Gamepad | SDLInitFlags.Haptic | SDLInitFlags.Joystick | SDLInitFlags.Sensor | SDLInitFlags.Video);

            SdlCheckError();

            Keyboard.Init();
            SdlCheckError();
            Mouse.Init();
            SdlCheckError();
            Displays.Init();
            SdlCheckError();
            Gamepads.Init();
            SdlCheckError();
            TouchDevices.Init();
            SdlCheckError();

            SDL.SetEventEnabled((uint)SDLEventType.DropFile, true);
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
                while (SDL.PollEvent(&evnt))
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

        private static void HandleEvent(SDLEvent evnt)
        {
            SDLEventType type = (SDLEventType)evnt.Type;
            switch (type)
            {
                case SDLEventType.First:
                    break;

                case SDLEventType.Quit:
                    if (!supressQuitApp)
                    {
                        exiting = true;
                    }
                    supressQuitApp = false;
                    break;

                case SDLEventType.Terminating:
                    exiting = true;
                    break;

                case SDLEventType.LowMemory:
                    break;

                case SDLEventType.WillEnterBackground:
                    break;

                case SDLEventType.DidEnterBackground:
                    break;

                case SDLEventType.WillEnterForeground:
                    break;

                case SDLEventType.DidEnterForeground:
                    break;

                case SDLEventType.LocaleChanged:
                    break;

                case >= SDLEventType.DisplayFirst and <= SDLEventType.DisplayLast:
                    {
                        var even = evnt.Display;
                        Displays.ProcessEvent(even);
                    }
                    break;

                case >= SDLEventType.WindowFirst and <= SDLEventType.WindowLast:
                    {
                        var even = evnt.Window;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessEvent(even);
                        }
                    }

                    break;

                case SDLEventType.KeyDown:
                    {
                        var even = evnt.Key;
                        Keyboard.OnKeyDown(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputKeyboard(even);
                        }
                    }
                    break;

                case SDLEventType.KeyUp:
                    {
                        var even = evnt.Key;
                        Keyboard.OnKeyUp(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputKeyboard(even);
                        }
                    }
                    break;

                case SDLEventType.TextEditing:
                    {
                        var even = evnt.Edit;
                        Keyboard.OnTextEditing(even);
                    }
                    break;

                case SDLEventType.TextInput:
                    {
                        var even = evnt.Text;
                        Keyboard.OnTextInput(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputText(even);
                        }
                    }
                    break;

                case SDLEventType.KeymapChanged:
                    break;

                case SDLEventType.MouseMotion:
                    {
                        var even = evnt.Motion;
                        Mouse.OnMotion(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case SDLEventType.MouseButtonDown:
                    {
                        var even = evnt.Button;
                        Mouse.OnButtonDown(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case SDLEventType.MouseButtonUp:
                    {
                        var even = evnt.Button;
                        Mouse.OnButtonUp(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case SDLEventType.MouseWheel:
                    {
                        var even = evnt.Wheel;
                        Mouse.OnWheel(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case SDLEventType.JoystickAxisMotion:
                    {
                        var even = evnt.Jaxis;
                        Joysticks.OnAxisMotion(even);
                    }
                    break;

                case SDLEventType.JoystickBallMotion:
                    {
                        var even = evnt.Jball;
                        Joysticks.OnBallMotion(even);
                    }
                    break;

                case SDLEventType.JoystickHatMotion:
                    {
                        var even = evnt.Jhat;
                        Joysticks.OnHatMotion(even);
                    }
                    break;

                case SDLEventType.JoystickButtonDown:
                    {
                        var even = evnt.Jbutton;
                        Joysticks.OnButtonDown(even);
                    }
                    break;

                case SDLEventType.JoystickButtonUp:
                    {
                        var even = evnt.Jbutton;
                        Joysticks.OnButtonUp(even);
                    }
                    break;

                case SDLEventType.JoystickAdded:
                    {
                        var even = evnt.Jdevice;
                        Joysticks.AddJoystick(even);
                    }
                    break;

                case SDLEventType.JoystickRemoved:
                    {
                        var even = evnt.Jdevice;
                        Joysticks.RemoveJoystick(even);
                    }
                    break;

                case SDLEventType.GamepadAxisMotion:
                    {
                        var even = evnt.Gaxis;
                        Gamepads.OnAxisMotion(even);
                    }
                    break;

                case SDLEventType.GamepadButtonDown:
                    {
                        var even = evnt.Gbutton;
                        Gamepads.OnButtonDown(even);
                    }
                    break;

                case SDLEventType.GamepadButtonUp:
                    {
                        var even = evnt.Gbutton;
                        Gamepads.OnButtonUp(even);
                    }
                    break;

                case SDLEventType.GamepadAdded:
                    {
                        var even = evnt.Gdevice;
                        Gamepads.AddController(even);
                    }
                    break;

                case SDLEventType.GamepadRemoved:
                    {
                        var even = evnt.Gdevice;
                        Gamepads.RemoveController(even);
                    }
                    break;

                case SDLEventType.GamepadRemapped:
                    {
                        var even = evnt.Gdevice;
                        Gamepads.OnRemapped(even);
                    }
                    break;

                case SDLEventType.GamepadTouchpadDown:
                    {
                        var even = evnt.Gtouchpad;
                        Gamepads.OnTouchPadDown(even);
                    }
                    break;

                case SDLEventType.GamepadTouchpadMotion:
                    {
                        var even = evnt.Gtouchpad;
                        Gamepads.OnTouchPadMotion(even);
                    }
                    break;

                case SDLEventType.GamepadTouchpadUp:
                    {
                        var even = evnt.Gtouchpad;
                        Gamepads.OnTouchPadUp(even);
                    }
                    break;

                case SDLEventType.GamepadSensorUpdate:
                    {
                        var even = evnt.Gsensor;
                        Gamepads.OnSensorUpdate(even);
                    }
                    break;

                case SDLEventType.FingerDown:
                    {
                        var even = evnt.Tfinger;
                        TouchDevices.FingerDown(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputTouchDown(even);
                        }
                    }
                    break;

                case SDLEventType.FingerUp:
                    {
                        var even = evnt.Tfinger;
                        TouchDevices.FingerUp(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputTouchUp(even);
                        }
                    }
                    break;

                case SDLEventType.FingerMotion:
                    {
                        var even = evnt.Tfinger;
                        TouchDevices.FingerMotion(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputTouchMotion(even);
                        }
                    }
                    break;

                case SDLEventType.ClipboardUpdate:
                    break;

                case SDLEventType.DropFile:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropFile(even);
                        }

                        SDL.Free(evnt.Drop.Data);
                    }
                    break;

                case SDLEventType.DropText:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropText(even);
                        }

                        SDL.Free(evnt.Drop.Data);
                    }
                    break;

                case SDLEventType.DropBegin:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropBegin(even);
                        }
                    }
                    break;

                case SDLEventType.DropComplete:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropComplete(even);
                        }
                    }
                    break;

                case SDLEventType.AudioDeviceAdded:
                    break;

                case SDLEventType.AudioDeviceRemoved:
                    break;

                case SDLEventType.SensorUpdate:
                    break;

                case SDLEventType.RenderTargetsReset:
                    break;

                case SDLEventType.RenderDeviceReset:
                    break;

                case SDLEventType.User:
                    break;

                case SDLEventType.Last:
                    break;

                default:
                    return;
            }
        }
    }
}