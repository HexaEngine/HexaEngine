namespace DiscordRPC
{
    using DiscordRPC.Entities;
    using HexaEngine.Core;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Editor.Plugins;
    using HexaEngine.Projects;
    using System;
    using System.Diagnostics;

    public class Plugin : IPlugin
    {
        private const ulong AppID = 1064854600173236264;
        private RpcClient? rpcClient;

        public string Name => "Discord RPC";

        public string Version => "1.0.0.0";

        public string Description => "Discord RPC";

        public void OnEnable()
        {
            ProjectManager.ProjectChanged += ProjectChanged;
            SceneManager.SceneChanged += SceneChanged;
            Application.OnDesignModeChanged += OnDesignModeChanged;

            try
            {
                rpcClient = new RpcClient(AppID);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            SetIdle();
        }

        private DateTime GetTime()
        {
            return DateTime.Now;
        }

        private void SetIdle()
        {
            if (rpcClient == null) return;

            rpcClient.Presence = new RichPresence()
                .WithState("Idle")
                .WithLargeImage(new("icon", "Editor"))
                .WithTimestamp(new(GetTime(), RichPresenceTimestamp.TimestampDisplayType.Elapsed));
        }

        private void SetSceneState()
        {
            if (rpcClient == null) return;
            if (ProjectManager.Project == null)
            {
                SetIdle();
                return;
            }

            if (SceneManager.Current == null)
            {
                SetIdle();
                return;
            }

            if (Application.InDesignMode)
            {
                rpcClient.Presence = new RichPresence()
                    .WithState($"{ProjectManager.Project.Name}: editing {Path.GetFileName(SceneManager.Current.Path)}")
                    .WithLargeImage(new("icon", "Editor"))
                    .WithTimestamp(new(GetTime(), RichPresenceTimestamp.TimestampDisplayType.Elapsed));
            }
            else
            {
                rpcClient.Presence = new RichPresence()
                    .WithState($"{ProjectManager.Project.Name}: playing {Path.GetFileName(SceneManager.Current.Path)}")
                    .WithLargeImage(new("icon", "Editor"))
                    .WithTimestamp(new(GetTime(), RichPresenceTimestamp.TimestampDisplayType.Elapsed));
            }
        }

        private void OnDesignModeChanged(bool obj)
        {
            SetSceneState();
        }

        private void SceneChanged(object? sender, SceneChangedEventArgs e)
        {
            if (rpcClient == null) return;
            SetSceneState();
        }

        private void ProjectChanged(HexaProject? obj)
        {
            if (rpcClient == null) return;
            if (obj == null)
            {
                SetIdle();
                return;
            }
            rpcClient.Presence = new RichPresence()
                    .WithState($"{obj.Name}: Idle")
                    .WithLargeImage(new("icon", "Editor"))
                    .WithTimestamp(new(GetTime(), RichPresenceTimestamp.TimestampDisplayType.Elapsed));
        }

        public void OnDisable()
        {
            rpcClient?.Dispose();
            rpcClient = null;
        }

        public void OnInitialize()
        {
        }

        public void OnUninitialize()
        {
            if (rpcClient != null)
            {
                ProjectManager.ProjectChanged -= ProjectChanged;
                SceneManager.SceneChanged -= SceneChanged;
                Application.OnDesignModeChanged -= OnDesignModeChanged;
                rpcClient.Dispose();
                rpcClient = null;
            }
        }
    }
}