﻿namespace HexaEngine.Scripting
{
    using HexaEngine.Core.Scenes;

    public interface IScript
    {
        public GameObject GameObject { get; set; }

        public void Awake()
        {
        }

        public void FixedUpdate()
        {
        }

        public void Update()
        {
        }

        public void Destroy()
        {
        }
    }
}