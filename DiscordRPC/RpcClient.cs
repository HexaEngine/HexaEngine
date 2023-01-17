// MIT License
//
// Copyright(c) 2022 Bujju
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DiscordGameSdk;
using System.Diagnostics;
using DiscordRPC.Entities;

namespace DiscordRPC
{
    /// <summary>
    ///     A client for Discord RPC.
    /// </summary>
    public class RpcClient : IDisposable
    {
        private Discord _client;

        private bool _isInitialized;

        private bool _isDisposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RpcClient" /> class.
        /// </summary>
        /// <param name="appId">Application/client ID from the <see href="https://discord.com/developers">Discord Developer Portal</see>.</param>
        /// <param name="steamId">Steam app ID if the app is on Steam.</param>
        /// <exception cref="DllNotFoundException">Thrown when the Discord Game SDK is not found.</exception>
        /// <exception cref="Exception">Thrown when the client fails to start.</exception>
        public RpcClient(ulong appId, uint? steamId = null)
        {
            //if (!File.Exists(".\\discord_game_sdk.dll"))
            //    throw new DllNotFoundException("The Discord Game SDK was not found. Please place the DLL file in the same directory as the executable.");

            try { _client = new Discord((long)appId, (ulong)CreateFlags.Default); }
            catch { throw new Exception("Failed to connect to Discord."); }

            if (steamId != null)
                _client.GetActivityManager().RegisterSteam(steamId.Value);
        }

        /// <summary>
        ///     Sets the Rich Presence.
        /// </summary>
        public RichPresence Presence
        {
            set
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("RpcClient");

                _client.GetActivityManager().UpdateActivity(value.ToActivity(), (result) =>
                {
                    if (result != Result.Ok)
                        throw new Exception($"Failed to update presence: {result}");
                });

                // Start the callbacks if not started already.
                if (!_isInitialized)
                {
                    Task.Run(async () =>
                    {
                        while (!_isDisposed)
                        {
                            _client.RunCallbacks();
                            await Task.Delay(1000 / 6);
                        }
                    }).ConfigureAwait(false);

                    _isInitialized = true;
                }
            }
        }

        /// <exception cref="Exception">Thrown when the presence fails to clear.</exception>
        public void Dispose()
        {
            _client.GetActivityManager().ClearActivity((result) =>
            {
                if (result != Result.Ok)
                    Debug.WriteLine($"Failed to clear presence: {result}");
            });

            _client.Dispose();
            _isInitialized = false;
            _isDisposed = true;

            GC.SuppressFinalize(this);
        }
    }
}