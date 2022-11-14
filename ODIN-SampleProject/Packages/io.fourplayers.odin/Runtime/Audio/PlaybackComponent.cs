using System;
using System.Linq;
using OdinNative.Core;
using OdinNative.Odin;
using OdinNative.Odin.Media;
using UnityEngine;

namespace OdinNative.Unity.Audio
{
    /// <summary>
    ///     Handles the Playback for received ODIN audio data.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PlaybackComponent : MonoBehaviour
    {
        /// <summary>
        ///     The initial offset of the audio frame buffer from the Current Spatial Clip Sample Position.
        /// </summary>
        private const float InitialBufferOffset = 0.2f;

        /// <summary>
        ///     The Unity AudioSource component for playback
        /// </summary>
        /// <remarks>Unity controls the playback device: no ConfigurationChanged event</remarks>
        public AudioSource PlaybackSource;

        /// <summary>
        ///     On true destroy the <see cref="PlaybackSource" /> in dispose to not leak
        ///     <see cref="UnityEngine.AudioSource" />
        ///     <see href="https://docs.unity3d.com/ScriptReference/AudioSource.html">(AudioSource)</see>
        ///     or false for manually manage sources
        /// </summary>
        public bool AutoDestroyAudioSource = true;

        /// <summary>
        ///     On true destroy the <see cref="OdinNative.Odin.Media.MediaStream" /> in dispose to not leak
        ///     or false for manually manage stream
        /// </summary>
        /// <remarks>On room leave/destroy the underlying streams will still be freed up</remarks>
        public bool AutoDestroyMediaStream = true;

        /// <summary>
        ///     Use set <see cref="SampleRate" /> on true, <see cref="OdinEditorConfig.RemoteSampleRate" /> on false
        /// </summary>
        public bool OverrideSampleRate;

        /// <summary>
        ///     The playback <see cref="OdinNative.Core.MediaSampleRate" />
        /// </summary>
        /// <remarks>
        ///     Set value is ignored on
        ///     <see cref="UnityEngine.AudioClip" />
        ///     <see href="https://docs.unity3d.com/ScriptReference/AudioClip.html">(AudioClip)</see>
        ///     creation if <see cref="OverrideSampleRate" /> is false
        /// </remarks>
        public MediaSampleRate SampleRate;

        private bool _isDestroying;
        private long _MediaStreamId;
        private ulong _PeerId;

        private string _RoomName;
        private float[] AsyncClipBuffer;

        private float[] AudioFrameData;

        private int ClipSamples;

        /// <summary>
        ///     The end position of the buffered stream audio frames inside the Spatial Audio Clip. We use this to append
        ///     a new Audio Frame from the Media Stream.
        /// </summary>
        private int FrameBufferEndPos;

        /// <summary>
        ///     Whether there are any audio frames stored in the Spatial Audio Clip.
        /// </summary>
        private bool IsFrameBufferEmpty = true;

        private PlaybackStream PlaybackMedia;

        /// <summary>
        ///     The time sample Position in the Spatial Audio Clip of the last update.
        /// </summary>
        private int PreviousClipPos;

        internal bool RedirectPlaybackAudio = true;
        private float[] ResampleBuffer;

        private double ResamplerCapacity;

        private AudioClip SpatialClip;
        private float SpatialClipSilenceScale = 1000f;
        private int UnitySampleRate;
        private bool UseResampler;

        /// <summary>
        ///     The Unity AudioSource mute property
        /// </summary>
        /// <remarks>Sets volume to 0 or restore original volume</remarks>
        public bool Mute
        {
            get => PlaybackSource?.mute ?? true;
            set
            {
                if (PlaybackSource == null) return;
                PlaybackSource.mute = value;
            }
        }

        /// <summary>
        ///     The Odin PlaybackStream underlying media stream calls
        /// </summary>
        /// <remarks>on true ignores stream calls</remarks>
        public bool MuteStream
        {
            get => OdinMedia?.IsMuted ?? true;
            set => OdinMedia?.SetMute(value);
        }

        internal PlaybackStream OdinMedia => OdinHandler.Instance.Client
            .Rooms[RoomName]?
            .RemotePeers[PeerId]?
            .Medias[MediaStreamId] as PlaybackStream;

        /// <summary>
        ///     Room name for this playback. Change this value to change the PlaybackStream by Rooms from the Client.
        /// </summary>
        /// <remarks>Invalid values will cause errors.</remarks>
        public string RoomName
        {
            get => _RoomName;
            set
            {
                _RoomName = value;
                PlaybackMedia = OdinMedia;
            }
        }

        /// <summary>
        ///     Peer id for this playback. Change this value to change the PlaybackStream by RemotePeers in the Room.
        /// </summary>
        /// <remarks>Invalid values will cause errors.</remarks>
        public ulong PeerId
        {
            get => _PeerId;
            set
            {
                _PeerId = value;
                PlaybackMedia = OdinMedia;
            }
        }

        /// <summary>
        ///     Media id for this playback. Change this value to pick a PlaybackStream by media id from peers Medias.
        /// </summary>
        /// <remarks>Invalid values will cause errors.</remarks>
        public long MediaStreamId
        {
            get => _MediaStreamId;
            set
            {
                _MediaStreamId = value;
                PlaybackMedia = OdinMedia;
            }
        }

        private int CurrentClipPos => PlaybackSource.timeSamples;

        /// <summary>
        ///     Use the output settings given by unity. Most of the time this is 44100Hz
        /// </summary>
        private int OutSampleRate => AudioSettings.outputSampleRate;

        public bool HasActivity
        {
            get
            {
                if (PlaybackMedia == null)
                    return false;

                return PlaybackMedia.IsActive;
            }
        }

        private void Awake()
        {
            if (PlaybackSource == null)
                PlaybackSource = gameObject.GetComponents<AudioSource>()
                    .Where(s => s.clip == null)
                    .FirstOrDefault() ?? gameObject.AddComponent<AudioSource>();

            //
            // AudioFrameData = new float[960]; // 48khz * 20ms
        }

        private void Reset()
        {
            RedirectPlaybackAudio = true;
            OverrideSampleRate = false;
            SampleRate = OdinHandler.Config.RemoteSampleRate;
            UnitySampleRate = AudioSettings.outputSampleRate;
            AudioFrameData = null;
            ResampleBuffer = null;
        }

        
        private void FixedUpdate()
        {
            bool canRead = !(_isDestroying || PlaybackMedia == null || PlaybackMedia.HasErrors ||
                             RedirectPlaybackAudio == false);

            if (canRead)
            {
                int readBufferSize = Mathf.FloorToInt(Time.fixedUnscaledDeltaTime * OutSampleRate);
                float[] readBuffer = new float[readBufferSize];

                uint readResult = PlaybackMedia.AudioReadData(readBuffer, readBufferSize);

                int numZeros = 0;
                int firstZeroIndex = -1;
                for (var i = 0; i < readBuffer.Length; i++)
                {
                    float entry = readBuffer[i];
                    if (entry == 0)
                    {
                        numZeros++;
                        if (firstZeroIndex < 0)
                            firstZeroIndex = i;
                    }
                }

                // if (numZeros > 0)
                // {
                //     if (numZeros != readBufferSize)
                //     {
                //         Debug.Log($"Buffer length: {readBuffer.Length}, found zeros: {numZeros}, first zero index: {firstZeroIndex}");
                //     }
                //     else
                //     {
                //         Debug.Log("Frame with zeroes");
                //     }
                //     // FrameBufferEndPos = (int)(CurrentClipPos + 0.1f * OutSampleRate);
                // }

                if (Utility.IsError(readResult))
                {
                    Debug.LogWarning(
                        $"{nameof(PlaybackComponent)} AudioReadData failed with error code {readResult}");
                }
                else
                {
                    // Debug.Log($"Readresult: {readResult}, readBuffer length: {readBuffer.Length}, FrameBuffer End Pos: {FrameBufferEndPos}, OutputSampleRade: {AudioSettings.outputSampleRate}");
                    if (numZeros != readBufferSize)
                    {
                        for (int i = 0; i < readBufferSize; i++)
                        {
                            int writePosition = FrameBufferEndPos + i;
                            writePosition %= ClipSamples;
                            AsyncClipBuffer[writePosition] = readBuffer[i];
                        }

                        FrameBufferEndPos += readBufferSize;
                        FrameBufferEndPos %= ClipSamples;
                        lastFrameEntryTime = Time.time;
                    }
                }
            }
            else
            {
                Debug.Log("Can't read.");
            }
            
            if(Time.time - lastFrameEntryTime > 0.1f)
                FrameBufferEndPos = (int)(CurrentClipPos + 0.1f * OutSampleRate);
            
            
            
            int distanceToClipStart = GetBufferDistance(CurrentClipPos, FrameBufferEndPos);
            float audioBufferTime = distanceToClipStart * 1000.0f / OutSampleRate;
            Debug.Log($"Audio Buffer: {audioBufferTime } ms");

            if (audioBufferTime < 25.0f)
            {
                Debug.Log("Entered fast catch up");
                PlaybackSource.pitch = 0.8f;
            }else if (audioBufferTime < 50.0f)
            {
                PlaybackSource.pitch = 0.9f;
            }else if (audioBufferTime > 200.0f)
                PlaybackSource.pitch = 1.1f;
            else
            {
                PlaybackSource.pitch = 1.0f;
            }
            
            
            
            SpatialClip.SetData(AsyncClipBuffer, 0);

            PreviousClipPos = CurrentClipPos;
        }

        private float lastFrameEntryTime;
        private void OnEnable()
        {
            lastFrameEntryTime = Time.time;
            if (PlaybackMedia != null && PlaybackMedia.HasErrors)
                Debug.LogWarning(
                    $"{nameof(PlaybackComponent)} on {gameObject.name} had errors in {nameof(PlaybackStream)} and should be destroyed! {PlaybackMedia}");

            if (OverrideSampleRate)
                AudioSettings.outputSampleRate = (int)SampleRate;

            RedirectPlaybackAudio = true;
            if (OdinHandler.Config.VerboseDebug)
                Debug.Log(
                    $"## {nameof(PlaybackComponent)}.OnEnable AudioSettings: outputSampleRate {AudioSettings.outputSampleRate}, driverCapabilities {Enum.GetName(typeof(AudioSpeakerMode), AudioSettings.driverCapabilities)}, speakerMode {Enum.GetName(typeof(AudioSpeakerMode), AudioSettings.speakerMode)}");

            UnitySampleRate = AudioSettings.outputSampleRate;
            if (UnitySampleRate != (int)OdinHandler.Config.RemoteSampleRate)
            {
                if (OdinHandler.Config.Verbose)
                    Debug.LogWarning(
                        $"{nameof(PlaybackComponent)} AudioSettings.outputSampleRate ({AudioSettings.outputSampleRate}) does NOT match RemoteSampleRate ({OdinHandler.Config.RemoteSampleRate})! Using Resampler...");

                UseResampler = true;
                AudioSettings.GetDSPBufferSize(out int dspBufferSize, out int dspBufferCount);
                ResamplerCapacity = dspBufferSize * ((uint)OdinDefaults.RemoteSampleRate / UnitySampleRate) /
                                    (int)AudioSettings.speakerMode;
            }

            ClipSamples = OutSampleRate / 2;
            SpatialClip = AudioClip.Create("spatialClip", ClipSamples, 1, AudioSettings.outputSampleRate, false);
            ResetAudioClip();
            PlaybackSource.clip = SpatialClip;
            PlaybackSource.loop = true;
            PlaybackSource.Play();

            AudioFrameData = new float[960];
            AsyncClipBuffer = new float[ClipSamples];

            IsFrameBufferEmpty = true;
            FrameBufferEndPos = (int)(CurrentClipPos + 0.02f * OutSampleRate);
            FrameBufferEndPos %= ClipSamples;

            PreviousClipPos = PlaybackSource.timeSamples;


            // StartCoroutine(StreamReadRoutine());
        }


        private void OnDisable()
        {
            PlaybackSource.Stop();
            RedirectPlaybackAudio = false;
            AudioFrameData = null;
            ResampleBuffer = null;
        }

        private void OnDestroy()
        {
            _isDestroying = true;

            if (AutoDestroyAudioSource)
                Destroy(PlaybackSource);

            if (AutoDestroyMediaStream)
                OdinHandler.Instance.Client?
                    .Rooms[RoomName]?
                    .RemotePeers[PeerId]?
                    .Medias.Free(MediaStreamId);
        }


        private bool IsBetween(int value, int a, int b)
        {
            bool v1 = b > a && value >= a && value <= b;
            bool v2 = a > b && value >= a && value >= b;
            bool v3 = a > b && value <= a && value <= b;

            return v1 || v2 || v3;
        }

        private int GetBufferDistance(int a, int b)
        {
            int result = b - a;
            if (result < 0)
                result += ClipSamples;
            return result;
        }

        private void ResetAudioClip()
        {
            SpatialClip.SetData(new float[ClipSamples], 0);
        }
    }
}