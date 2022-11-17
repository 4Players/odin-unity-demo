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
        ///     THe minimum audio buffer size. I do not recommend lowering this, because values below 20ms lead to an extreme
        ///     amount of noise.
        /// </summary>
        private const float minBufferSize = 0.02f;

        /// <summary>
        ///     The maximum audio buffer size - if we go above this, reset the audio buffer. Will lead to a bit of noise, but
        ///     reset the audio lag.
        /// </summary>
        private const float maxBufferSize = 2f * targetBufferSize;

        /// <summary>
        ///     The target audio buffer size in seconds
        /// </summary>
        private const float targetBufferSize = 0.1f;

        private const float targetBufferTolerance = 0.015f;

        private const float targetSizePitchAdjustment = 0.025f;

        /// <summary>
        ///     The maximum amount of zero frames in seconds we wait before resetting the current audio buffer. Uses
        ///     the <see cref="lastFrameReadTime" /> to determine if we have hit this value.
        /// </summary>
        private const float maxFrameLossTime = 0.2f;

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
        private float[] ClipBuffer;


        private int ClipSamples => SpatialClip.samples;

        /// <summary>
        ///     The end position of the buffered stream audio frames inside the Spatial Audio Clip. We use this to append
        ///     a new Audio Frame from the Media Stream.
        /// </summary>
        private int FrameBufferEndPos;

        /// <summary>
        ///     Whether there are any audio frames stored in the Spatial Audio Clip.
        /// </summary>
        private bool IsFrameBufferEmpty = true;

        /// <summary>
        ///     The last time we read an ODIN audio frame into the output buffer.
        /// </summary>
        private float lastFrameReadTime;

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
            ResampleBuffer = null;
        }


        private void FixedUpdate()
        {
            // if(Time.unscaledDeltaTime < Time.fixedUnscaledDeltaTime)
            //     PlaybackSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            // else
            // {
            //     PlaybackSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
            // }
            
            bool canRead = !(_isDestroying || PlaybackMedia == null || PlaybackMedia.HasErrors ||
                             RedirectPlaybackAudio == false);

            int numZeros = 0;

            if (canRead)
            {
                int readBufferSize = Mathf.FloorToInt(Time.fixedUnscaledDeltaTime * OutSampleRate);
                // todo: avoid creating a float buffer every frame
                float[] readBuffer = new float[readBufferSize];

                uint readResult = PlaybackMedia.AudioReadData(readBuffer, readBufferSize);

                for (var i = 0; i < readBuffer.Length; i++)
                {
                    float entry = readBuffer[i];
                    if (entry == 0)
                    {
                        numZeros++;
                    }
                }

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
                            ClipBuffer[writePosition] = readBuffer[i];
                        }

                        FrameBufferEndPos += readBufferSize;
                        FrameBufferEndPos %= ClipSamples;
                        lastFrameReadTime = Time.time;
                    }
                }
            }

            int distanceToClipStart = GetBufferDistance(CurrentClipPos, FrameBufferEndPos);
            float audioBufferSize = (float)distanceToClipStart / OutSampleRate;

            // Reset the frame buffering, if we haven't received an audio frame for a certain amount of time
            bool shouldResetFrameBuffer = Time.time - lastFrameReadTime > maxFrameLossTime;
            shouldResetFrameBuffer |=
                audioBufferSize <
                minBufferSize; // This is a fixed value - anything below this will lead to audio issues
            shouldResetFrameBuffer |= audioBufferSize > maxBufferSize;
            if (shouldResetFrameBuffer)
                FrameBufferEndPos = GetTargetFrameBufferEndPosition();

            float targetPitch = 1.0f;
            if (audioBufferSize < targetBufferSize - targetBufferTolerance)
                targetPitch = 1.0f - targetSizePitchAdjustment;
            else if (audioBufferSize > targetBufferSize + targetBufferTolerance)
                targetPitch = 1.0f + targetSizePitchAdjustment;

            float pitch = PlaybackSource.pitch;
            pitch += (targetPitch - pitch) * 0.1f;
            PlaybackSource.pitch = pitch;


            Debug.Log($"Audio Buffer: {audioBufferSize * 1000.0f} ms, Pitch: {pitch}, fixed delta time: {Time.fixedUnscaledDeltaTime}");
            SpatialClip.SetData(ClipBuffer, 0);
            PreviousClipPos = CurrentClipPos;
        }

        private void OnEnable()
        {
            lastFrameReadTime = Time.time;
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

            int clipSamples = (int)(OutSampleRate * 3.0f * targetBufferSize);
            SpatialClip = AudioClip.Create("spatialClip", clipSamples, 1, AudioSettings.outputSampleRate, false);
            ResetAudioClip();
            PlaybackSource.clip = SpatialClip;
            PlaybackSource.loop = true;
            // PlaybackSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            PlaybackSource.Play();

            ClipBuffer = new float[ClipSamples];

            IsFrameBufferEmpty = true;
            FrameBufferEndPos = GetTargetFrameBufferEndPosition();
            FrameBufferEndPos %= ClipSamples;

            PreviousClipPos = PlaybackSource.timeSamples;
        }


        private void OnDisable()
        {
            PlaybackSource.Stop();
            RedirectPlaybackAudio = false;
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


        private int GetTargetFrameBufferEndPosition()
        {
            return (int)(CurrentClipPos + targetBufferSize * OutSampleRate);
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