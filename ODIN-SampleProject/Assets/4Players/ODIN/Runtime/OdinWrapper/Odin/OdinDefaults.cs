using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OdinNative.Core;

namespace OdinNative.Odin
{
    /// <summary>
    /// ODIN default configuration
    /// </summary>
    public static class OdinDefaults
    {
        /// <summary>
        /// Enable additional Logs
        /// </summary>
        public static bool Verbose = false;

        public static string AccessKey { get; set; } = "";
        public static string Server { get; set; } = "https://gateway.odin.4players.io";
        public static string UserDataText { get; set; } = "";

        /// <summary>
        /// Microphone default Sample-Rate
        /// </summary>
        public static MediaSampleRate DeviceSampleRate { get; set; } = MediaSampleRate.Device_Max;
        /// <summary>
        /// Microphone default Channel
        /// </summary>
        public static MediaChannels DeviceChannels { get; set; } = MediaChannels.Mono;

        /// <summary>
        /// Playback default Sample-Rate
        /// </summary>
        public static MediaSampleRate RemoteSampleRate { get; set; } = MediaSampleRate.Hz48000;
        /// <summary>
        /// Playback default Channel
        /// </summary>
        public static MediaChannels RemoteChannels { get; set; } = MediaChannels.Mono;

        #region Events
        public static bool PeerJoinedEvent = true;
        public static bool PeerLeftEvent = true;
        public static bool PeerUpdatedEvent = true;
        public static bool MediaAddedEvent = true;
        public static bool MediaRemovedEvent = true;
        public static bool RoomUpdatedEvent = true;
        public static bool MediaActiveStateChangedEvent = true;
        public static bool MessageReceivedEvent = true;
        #endregion Events

        public static ulong TokenLifetime { get; set; } = 300;

        #region Apm
        public static bool VoiceActivityDetection = true;
        public static float VoiceActivityDetectionAttackProbability = 0.9f;
        public static float VoiceActivityDetectionReleaseProbability = 0.8f;
        public static bool VolumeGate = false;
        public static float VolumeGateAttackLoudness = -30;
        public static float VolumeGateReleaseLoudness = -40;
        public static bool EchoCanceller = false;
        public static bool HighPassFilter = false;
        public static bool PreAmplifier = false;
        public static Core.Imports.NativeBindings.OdinNoiseSuppressionLevel NoiseSuppressionLevel = Core.Imports.NativeBindings.OdinNoiseSuppressionLevel.None;
        public static bool TransientSuppressor = false;
        #endregion Apm
    }
}
