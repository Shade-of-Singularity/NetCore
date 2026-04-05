using Steamworks;
using System;

namespace NetCore.SteamNetworking
{
#if GODOT
    /// <summary>
    /// Script for initializing steam systems.
    /// </summary>
    public sealed partial class SteamManager : Node
    {
        /// <summary>
        /// Whether <see cref="SteamAPI"/> is initialized or not.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <inheritdoc/>
        public override void _EnterTree()
        {
            var result = SteamAPI.InitEx(out string error);
            if (result == ESteamAPIInitResult.k_ESteamAPIInitResult_OK)
            {
                GD.Print("Steam API initialized.");

                // Makes sure to disable direct P2P to ensure the client IP will not get exposed.
                SteamNetworkingUtils.SetConfigValue(
                    ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_P2P_Transport_ICE_Enable,
                    ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
                    IntPtr.Zero,
                    ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                    pArg: 0); // Disabled.
                return;
            }

            GD.PrintErr($"Cannot initialize Steam API! Issue: {result}  Message: {error}");
        }

        /// <inheritdoc/>
        public override void _Process(double delta)
        {
            if (IsInitialized)
                SteamAPI.RunCallbacks();
        }

        /// <inheritdoc/>
        public override void _ExitTree()
        {
            if (IsInitialized)
                SteamAPI.Shutdown();
        }
    }
#elif UNITY
    // Unity support is pending.
#else
    // SteamNetworking outside of games are not yet supported.
#endif
}
