using OdinNative.Core;
using OdinNative.Odin;
using OdinNative.Odin.Media;
using System;
using System.Linq;
using UnityEngine;

namespace OdinNative.Unity.Audio
{
    /// <summary>
    /// Handles the Playback for received ODIN audio data.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PlaybackComponent : MonoBehaviour
    {
        /// <summary>
        /// The Unity AudioSource component for playback
        /// </summary>
        /// <remarks>Unity controls the playback device: no ConfigurationChanged event</remarks>
        public AudioSource PlaybackSource;
        private int UnitySampleRate;
        private bool UseResampler;
        private float[] ResampleBuffer;
        private double ResamplerCapacity;
        /// <summary>
        /// The Unity AudioSource mute property
        /// </summary>
        /// <remarks>Sets volume to 0 or restore original volume</remarks>
        public bool Mute { get { return PlaybackSource?.mute ?? true; } set { if (PlaybackSource == null) return; PlaybackSource.mute = value; } }
        /// <summary>
        /// The Odin PlaybackStream underlying media stream calls
        /// </summary>
        /// <remarks>on true ignores stream calls</remarks>
        public bool MuteStream { get { return OdinMedia?.IsMuted ?? true; } set { OdinMedia?.SetMute(value); } }
        internal PlaybackStream OdinMedia => OdinHandler.Instance.Client
            .Rooms[RoomName]?
            .RemotePeers[PeerId]?
            .Medias[MediaStreamId] as PlaybackStream;

        private string _RoomName;
        /// <summary>
        /// Room name for this playback. Change this value to change the PlaybackStream by Rooms from the Client.
        /// </summary>
        /// <remarks>Invalid values will cause errors.</remarks>
        public string RoomName
        {
            get { return _RoomName; }
            set
            {
                _RoomName = value;
                PlaybackMedia = OdinMedia;
            }
        }
        private ulong _PeerId;
        /// <summary>
        /// Peer id for this playback. Change this value to change the PlaybackStream by RemotePeers in the Room.
        /// </summary>
        /// <remarks>Invalid values will cause errors.</remarks>
        public ulong PeerId
        {
            get { return _PeerId; }
            set
            {
                _PeerId = value;
                PlaybackMedia = OdinMedia;
            }
        }
        private long _MediaStreamId;
        /// <summary>
        /// Media id for this playback. Change this value to pick a PlaybackStream by media id from peers Medias.
        /// </summary>
        /// <remarks>Invalid values will cause errors.</remarks>
        public long MediaStreamId
        {
            get { return _MediaStreamId; }
            set
            {
                _MediaStreamId = value;
                PlaybackMedia = OdinMedia;
            }
        }
        private PlaybackStream PlaybackMedia;
        /// <summary>
        /// On true destroy the <see cref="PlaybackSource"/> in dispose to not leak 
        /// <see cref="UnityEngine.AudioSource"/> <see href="https://docs.unity3d.com/ScriptReference/AudioSource.html">(AudioSource)</see>
        /// or false for manually manage sources
        /// </summary>
        public bool AutoDestroyAudioSource = true;
        /// <summary>
        /// On true destroy the <see cref="OdinNative.Odin.Media.MediaStream"/> in dispose to not leak 
        /// or false for manually manage stream
        /// </summary>
        /// <remarks>On room leave/destroy the underlying streams will still be freed up</remarks>
        public bool AutoDestroyMediaStream = true;
        internal bool RedirectPlaybackAudio = true;

        private float[] ReadBuffer;

        /// <summary>
        /// Use set <see cref="SampleRate"/> on true, <see cref="OdinEditorConfig.RemoteSampleRate"/> on false
        /// </summary>
        public bool OverrideSampleRate;
        /// <summary>
        /// The playback <see cref="OdinNative.Core.MediaSampleRate"/>
        /// </summary>
        /// <remarks>Set value is ignored on 
        /// <see cref="UnityEngine.AudioClip"/> <see href="https://docs.unity3d.com/ScriptReference/AudioClip.html">(AudioClip)</see>
        /// creation if <see cref="OverrideSampleRate"/> is false</remarks>
        public MediaSampleRate SampleRate;

        public bool HasActivity
        {
            get
            {
                if (PlaybackMedia == null)
                    return false;

                return PlaybackMedia.IsActive;
            }
        }

        void Awake()
        {
            if (PlaybackSource == null)
                PlaybackSource = gameObject.GetComponents<AudioSource>()
                    .Where(s => s.clip == null)
                    .FirstOrDefault() ?? gameObject.AddComponent<AudioSource>();

            PlaybackSource.loop = true;
        }

        void OnEnable()
        {
            if (PlaybackSource.isPlaying == false)
                PlaybackSource.Play();

            if (OverrideSampleRate)
                AudioSettings.outputSampleRate = (int)SampleRate;

            RedirectPlaybackAudio = true;
            if (OdinHandler.Config.VerboseDebug)
                Debug.Log($"## {nameof(PlaybackComponent)}.OnEnable AudioSettings: outputSampleRate {AudioSettings.outputSampleRate}, driverCapabilities {Enum.GetName(typeof(AudioSpeakerMode), AudioSettings.driverCapabilities)}, speakerMode {Enum.GetName(typeof(AudioSpeakerMode), AudioSettings.speakerMode)}");

            UnitySampleRate = AudioSettings.outputSampleRate;
            if (UnitySampleRate != (int)OdinHandler.Config.RemoteSampleRate)
            {
                Debug.LogWarning($"{nameof(PlaybackComponent)} AudioSettings.outputSampleRate ({AudioSettings.outputSampleRate}) does NOT match RemoteSampleRate ({OdinHandler.Config.RemoteSampleRate})!");
                UseResampler = true;
                AudioSettings.GetDSPBufferSize(out int dspBufferSize, out int dspBufferCount);
                ResamplerCapacity = dspBufferSize * ((uint)OdinDefaults.RemoteSampleRate / UnitySampleRate) / (int)AudioSettings.speakerMode;
            }
        }

        void Reset()
        {
            OverrideSampleRate = false;
            SampleRate = OdinHandler.Config.RemoteSampleRate;
            UnitySampleRate = AudioSettings.outputSampleRate;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (PlaybackMedia == null || PlaybackMedia.IsMuted || RedirectPlaybackAudio == false) return;

            if (!UseResampler && ReadBuffer == null)
                ReadBuffer = new float[data.Length / channels];

            if (UseResampler && ResampleBuffer == null)
            {
                ResamplerCapacity = data.Length / channels;
                ResampleBuffer = new float[(int)ResamplerCapacity];

                double bufferSize = Math.Ceiling(ResamplerCapacity * ((double)OdinDefaults.RemoteSampleRate / UnitySampleRate));
                ReadBuffer = new float[(int)bufferSize];
            }

            uint read = PlaybackMedia.AudioReadData(ReadBuffer, ReadBuffer.Length);
            if (UseResampler)
            {
                uint readResampled = PlaybackMedia.AudioResample(ReadBuffer, (uint)UnitySampleRate, ResampleBuffer, ResampleBuffer.Length);
                SetData(ResampleBuffer, 0, (int)readResampled);
            }
            else
                SetData(ReadBuffer, 0, (int)read);

            int SetData(float[] buffer, int offset, int count)
            {
                int i = 0;
                switch (channels)
                {
                    case 1:
                        foreach (float sample in buffer.Skip(offset).Take(count))
                            data[i++] = sample;
                        break;
                    case 2:
                        foreach (float sample in buffer.Skip(offset).Take(count))
                        {
                            data[i] = sample;
                            data[i + 1] = sample;
                            i += channels;
                        }
                        break;
                    default:
                        Debug.LogException(new NotSupportedException($"channels {channels} is currently not supported"));
                        break;
                }
                return i;
            }
        }

        void OnDisable()
        {
            CancelInvoke();
            PlaybackSource.Stop();
            RedirectPlaybackAudio = false;
        }

        private void OnDestroy()
        {
            if (AutoDestroyAudioSource)
                Destroy(PlaybackSource);

            if (AutoDestroyMediaStream)
                OdinHandler.Instance.Client?
                .Rooms[RoomName]?
                .RemotePeers[PeerId]?
                .Medias.Free(MediaStreamId);
        }
    }
}
