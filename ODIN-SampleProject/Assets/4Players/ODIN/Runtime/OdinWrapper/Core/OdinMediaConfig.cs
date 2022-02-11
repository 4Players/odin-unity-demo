using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OdinNative.Core.Imports.NativeBindings;

namespace OdinNative.Core
{
    /// <summary>
    /// ODIN audio stream configuration
    /// </summary>
    public class OdinMediaConfig
    {
        /// <summary>
        /// The number audio samples carried per second in Hz
        /// </summary>
        public MediaSampleRate SampleRate
        {
            get { return (MediaSampleRate)sampleRate; }
            set { sampleRate = (uint)value; }
        }

        /// <summary>
        /// The number of audio channels
        /// </summary>
        public MediaChannels Channels
        {
            get { return (MediaChannels)channelCount; }
            set { channelCount = (byte)value; }
        }

        private uint sampleRate
        {
            get { return AudioStreamConfig.sample_rate; }
            set { AudioStreamConfig.sample_rate = value; }
        }

        private byte channelCount
        {
            get { return AudioStreamConfig.channel_count; }
            set { AudioStreamConfig.channel_count = value; }
        }

        internal bool RemoteConfig { get; private set; }

        internal OdinAudioStreamConfig GetOdinAudioStreamConfig() => AudioStreamConfig;

        private OdinAudioStreamConfig AudioStreamConfig = new OdinAudioStreamConfig();

        private OdinMediaConfig(OdinAudioStreamConfig config) : this(config.sample_rate, config.channel_count, true) { }
        public OdinMediaConfig(MediaSampleRate rate, MediaChannels channels) : this((uint)rate, (byte)channels, false) { }
        internal OdinMediaConfig(uint rate, byte channels, bool remote = false)
        {
            sampleRate = rate;
            channelCount = channels;
            RemoteConfig = remote;
        }
    }

    /// <summary>
    /// Supported audio sample rate values
    /// </summary>
    public enum MediaSampleRate : uint
    {
        Device_Min,
        Device_Max,
        Hz8000 = 8000,
        Hz11025 = 11025,
        Hz12000 = 12000,
        Hz16000 = 16000,
        Hz22050 = 22050,
        Hz24000 = 24000,
        Hz32000 = 32000,
        Hz44100 = 44100,
        Hz48000 = 48000,
        Hz96000 = 96000,
        Hz128000 = 128000,
        Hz144000 = 144000,
        Hz192000 = 192000,
    }

    /// <summary>
    /// Supported values for audio channel count
    /// </summary>
    public enum MediaChannels : byte
    {
        Mono = 1,          // Defines a single (monaural) channel
        Stereo = 2,        // Defines two (stereo) channels
        //DolbySurround = 3,  // Defines three channels
        //DolbyProLogic = 4,  // Defines four channels
        //DolbyDigital = 6,   // Defines six channels
    }
}
