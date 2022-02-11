using System.Collections;
using System.Collections.Generic;
using System;
using static OdinNative.Core.Imports.NativeBindings;

namespace OdinNative.Core
{
    /// <summary>
    /// Audio processing configuration of an ODIN room
    /// </summary>
    public class OdinRoomConfig
    {
        /// <summary>
        /// Enables or disables RNN-based voice activity detection
        /// </summary>
        public bool VadEnable
        {
            get { return ApmConfig.vad_enable; }
            set { ApmConfig.vad_enable = value; }
        }

        /// <summary>
        /// Enable or disable echo cancellation
        /// </summary>
        public bool EchoCanceller
        {
            get { return ApmConfig.echo_canceller; }
            set { ApmConfig.echo_canceller = value; }
        }

        /// <summary>
        /// Enable or disable high pass filtering
        /// </summary>
        public bool HighPassFilter
        {
            get { return ApmConfig.high_pass_filter; }
            set { ApmConfig.high_pass_filter = value; }
        }

        /// <summary>
        /// Enable or disable the pre amplifier
        /// </summary>
        public bool PreAmplifier
        {
            get { return ApmConfig.pre_amplifier; }
            set { ApmConfig.pre_amplifier = value; }
        }

        /// <summary>
        /// Set the aggressiveness of the suppression
        /// </summary>
        public OdinNoiseSuppressionLevel OdinNoiseSuppressionLevel
        {
            get { return ApmConfig.noise_suppression_level; }
            set { ApmConfig.noise_suppression_level = value; }
        }
        /// <summary>
        /// Enable or disable the transient suppressor
        /// </summary>
        public bool TransientSuppressor
        {
            get { return ApmConfig.transient_suppressor; }
            set { ApmConfig.transient_suppressor = value; }
        }

        internal bool RemoteConfig { get; private set; }

        internal OdinApmConfig GetOdinApmConfig() => ApmConfig;

        private OdinApmConfig ApmConfig = new OdinApmConfig();

        private OdinRoomConfig(OdinApmConfig config) : this(config.vad_enable, config.echo_canceller, config.high_pass_filter, config.pre_amplifier, config.noise_suppression_level, config.transient_suppressor, true) { }
        public OdinRoomConfig(bool vadEnable = false, bool echoCanceller = false, bool highPassFilter = false, bool preAmplifier = false, OdinNoiseSuppressionLevel noiseSuppressionLevel = OdinNoiseSuppressionLevel.None, bool transientSuppressor = false)
            : this(vadEnable, echoCanceller, highPassFilter, preAmplifier, noiseSuppressionLevel, transientSuppressor, false) { }
        internal OdinRoomConfig(bool vadEnable, bool echoCanceller, bool highPassFilter, bool preAmplifier, OdinNoiseSuppressionLevel noiseSuppressionLevel, bool transientSuppressor, bool remote = false)
        {
            VadEnable = vadEnable;
            EchoCanceller = echoCanceller;
            HighPassFilter = highPassFilter;
            PreAmplifier = preAmplifier;
            OdinNoiseSuppressionLevel = noiseSuppressionLevel;
            TransientSuppressor = transientSuppressor;
            RemoteConfig = remote;
        }
    }
}