namespace HexaEngine.Core
{
    using HexaEngine.Core.Audio;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.Windows;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.Mathematics;
    using Silk.NET.SDL;
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
        public static readonly Sdl Sdl = Sdl.GetApi();

        private static bool initialized = false;
        private static bool exiting = false;
        private static bool supressQuitApp = false;
        private static readonly Dictionary<uint, ICoreWindow> windowIdToWindow = new();
        private static readonly List<ICoreWindow> windows = new();
        private static readonly List<Func<Event, bool>> hooks = new();

        private static bool inEditorMode;
        private static EditorPlayState editorPlayState;

#nullable disable
        private static ICoreWindow mainWindow;
        private static IGraphicsDevice graphicsDevice;
        private static IGraphicsContext graphicsContext;
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
        /// Gets or sets the graphics backend used by the application.
        /// </summary>
        public static GraphicsBackend GraphicsBackend
        {
            get; set;
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
        /// Initializes the application and necessary subsystems.
        /// </summary>
        public static void Boot(GraphicsBackend backend, bool disableLogging = false)
        {
            GraphicsBackend = backend;
#if DEBUG
            GraphicsDebugging = true;
#endif
            if (disableLogging)
                CrashLogger.Initialize();
            FileSystem.Initialize();
            ImGuiConsole.Initialize();

            Sdl.SetHint(Sdl.HintMouseFocusClickthrough, "1");
            Sdl.SetHint(Sdl.HintAutoUpdateJoysticks, "1");
            Sdl.SetHint(Sdl.HintJoystickHidapiPS4, "1");
            Sdl.SetHint(Sdl.HintJoystickHidapiPS4Rumble, "1");
            Sdl.SetHint(Sdl.HintJoystickRawinput, "0");
            Sdl.SetHint(Sdl.HintWindowsDisableThreadNaming, "1");
            Sdl.SetHint(Sdl.HintMouseNormalSpeedScale, "1");

            Sdl.Init(Sdl.InitEvents + Sdl.InitGamecontroller + Sdl.InitHaptic + Sdl.InitJoystick + Sdl.InitSensor);

            SdlCheckError();

            Keyboard.Init();
            SdlCheckError();
            Mouse.Init();
            SdlCheckError();
            Gamepads.Init();
            SdlCheckError();
            TouchDevices.Init();
            SdlCheckError();

            Sdl.EventState((uint)EventType.Dropfile, Sdl.Enable);
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
            Event evnt;
            Time.ResetTime();

            while (!exiting)
            {
                Sdl.PumpEvents();
                while (Sdl.PollEvent(&evnt) == (int)SdlBool.True)
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
            Sdl.Quit();
        }

        private class Program
        {
            private static bool exiting = false;

            private static unsafe void PlatformRun()
            {
                Event evnt;
                Time.ResetTime();

                while (!exiting)
                {
                    Sdl.PumpEvents();
                    while (Sdl.PollEvent(&evnt) == (int)SdlBool.True)
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
                Sdl.Quit();
            }
        }

        public static void StartTextInput()
        {
            Sdl.StartTextInput();
        }

        public static void StopTextInput()
        {
            Sdl.StopTextInput();
        }

        public static void SetTextInputRect(Rectangle rect)
        {
            Sdl.SetTextInputRect((Silk.NET.Maths.Rectangle<int>*)&rect);
        }

        private static void HandleEvent(Event evnt)
        {
            EventType type = (EventType)evnt.Type;
            switch (type)
            {
                case EventType.Firstevent:
                    break;

                case EventType.Quit:
                    if (!supressQuitApp)
                    {
                        exiting = true;
                    }
                    supressQuitApp = false;
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
                        {
                            ((SdlWindow)mainWindow).ProcessInputKeyboard(even);
                        }
                    }
                    break;

                case EventType.Keyup:
                    {
                        var even = evnt.Key;
                        Keyboard.OnKeyUp(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputKeyboard(even);
                        }
                    }
                    break;

                case EventType.Textediting:
                    {
                        var even = evnt.Edit;
                        Keyboard.OnTextEditing(even);
                    }
                    break;

                case EventType.Textinput:
                    {
                        var even = evnt.Text;
                        Keyboard.OnTextInput(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputText(even);
                        }
                    }
                    break;

                case EventType.Keymapchanged:
                    break;

                case EventType.Mousemotion:
                    {
                        var even = evnt.Motion;
                        Mouse.OnMotion(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case EventType.Mousebuttondown:
                    {
                        var even = evnt.Button;
                        Mouse.OnButtonDown(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case EventType.Mousebuttonup:
                    {
                        var even = evnt.Button;
                        Mouse.OnButtonUp(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
                    }
                    break;

                case EventType.Mousewheel:
                    {
                        var even = evnt.Wheel;
                        Mouse.OnWheel(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputMouse(even);
                        }
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
                        Gamepads.OnAxisMotion(even);
                    }
                    break;

                case EventType.Controllerbuttondown:
                    {
                        var even = evnt.Cbutton;
                        Gamepads.OnButtonDown(even);
                    }
                    break;

                case EventType.Controllerbuttonup:
                    {
                        var even = evnt.Cbutton;
                        Gamepads.OnButtonUp(even);
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
                        Gamepads.OnRemapped(even);
                    }
                    break;

                case EventType.Controllertouchpaddown:
                    {
                        var even = evnt.Ctouchpad;
                        Gamepads.OnTouchPadDown(even);
                    }
                    break;

                case EventType.Controllertouchpadmotion:
                    {
                        var even = evnt.Ctouchpad;
                        Gamepads.OnTouchPadMotion(even);
                    }
                    break;

                case EventType.Controllertouchpadup:
                    {
                        var even = evnt.Ctouchpad;
                        Gamepads.OnTouchPadUp(even);
                    }
                    break;

                case EventType.Controllersensorupdate:
                    {
                        var even = evnt.Csensor;
                        Gamepads.OnSensorUpdate(even);
                    }
                    break;

                case EventType.Fingerdown:
                    {
                        var even = evnt.Tfinger;
                        TouchDevices.FingerDown(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputTouchDown(even);
                        }
                    }
                    break;

                case EventType.Fingerup:
                    {
                        var even = evnt.Tfinger;
                        TouchDevices.FingerUp(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputTouchUp(even);
                        }
                    }
                    break;

                case EventType.Fingermotion:
                    {
                        var even = evnt.Tfinger;
                        TouchDevices.FingerMotion(even);
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessInputTouchMotion(even);
                        }
                    }
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
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropFile(even);
                        }

                        Sdl.Free(evnt.Drop.File);
                    }
                    break;

                case EventType.Droptext:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropText(even);
                        }

                        Sdl.Free(evnt.Drop.File);
                    }
                    break;

                case EventType.Dropbegin:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropBegin(even);
                        }
                    }
                    break;

                case EventType.Dropcomplete:
                    {
                        var even = evnt.Drop;
                        if (even.WindowID == mainWindow.WindowID)
                        {
                            ((SdlWindow)mainWindow).ProcessDropComplete(even);
                        }
                    }
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

                default:
                    return;
            }
        }
    }
}