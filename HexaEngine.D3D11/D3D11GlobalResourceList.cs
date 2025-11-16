namespace HexaEngine.D3D11
{
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;

    public class D3D11GlobalResourceList
    {
        private static readonly Dictionary<string, D3D11ShaderParameterState> states = new();
        private static readonly SemaphoreSlim semaphore = new(1);

        public static event StateChangedEventHandler? StateChanged;

        public delegate void StateChangedEventHandler(string name, D3D11ShaderParameterState oldState, D3D11ShaderParameterState state);

        public static void SetSRV(string name, IShaderResourceView? srv)
        {
            var p = srv?.NativePointer ?? 0; // Get native pointer or use 0 if null
            UpdateState(name, p, ShaderParameterType.SRV);
        }

        public static void SetUAV(string name, IUnorderedAccessView? uav, uint initialCount = unchecked((uint)-1))
        {
            var p = uav?.NativePointer ?? 0; // Get native pointer or use 0 if null
            UpdateState(name, p, ShaderParameterType.UAV, initialCount);
        }

        public static void SetCBV(string name, IBuffer? cbv)
        {
            var p = cbv?.NativePointer ?? 0; // Get native pointer or use 0 if null
            UpdateState(name, p, ShaderParameterType.CBV);
        }

        public static void SetSampler(string name, ISamplerState? sampler)
        {
            var p = sampler?.NativePointer ?? 0; // Get native pointer or use 0 if null
            UpdateState(name, p, ShaderParameterType.Sampler);
        }

        private static unsafe void UpdateState(string name, nint pointer, ShaderParameterType type, uint initialCount = unchecked((uint)-1))
        {
            semaphore.Wait();
            try
            {
                if (pointer == 0)
                {
                    states.TryGetValue(name, out var oldState);

                    // If pointer is 0, remove the state if it exists to handle resource cleanup
                    states.Remove(name);

                    StateChanged?.Invoke(name, oldState, new() { Type = type });
                }
                else
                {
                    states.TryGetValue(name, out var oldState);

                    D3D11ShaderParameterState state = new()
                    {
                        Type = type,
                        Resource = (void*)pointer, // Cast nint to void*
                        InitialCount = initialCount
                    };

                    // Update or create new state
                    states[name] = state;

                    // notify change.
                    StateChanged?.Invoke(name, oldState, state);
                }
            }
            finally
            {
                // events could fail somewhere better safe then sorry.
                semaphore.Release();
            }
        }

        public static unsafe void SetState(D3D11ResourceBindingList resourceBindingList)
        {
            semaphore.Wait();

            try
            {
                resourceBindingList.Hook(); // hook inside of the semaphore lock so that we wont miss any events.
                foreach (var pair in states)
                {
                    string name = pair.Key;
                    var state = pair.Value;
                    switch (state.Type)
                    {
                        case ShaderParameterType.SRV:
                            resourceBindingList.SetSRV(name, state.Resource);
                            break;

                        case ShaderParameterType.UAV:
                            resourceBindingList.SetUAV(name, state.Resource, state.InitialCount);
                            break;

                        case ShaderParameterType.CBV:
                            resourceBindingList.SetCBV(name, state.Resource);
                            break;

                        case ShaderParameterType.Sampler:
                            resourceBindingList.SetSampler(name, state.Resource);
                            break;
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}