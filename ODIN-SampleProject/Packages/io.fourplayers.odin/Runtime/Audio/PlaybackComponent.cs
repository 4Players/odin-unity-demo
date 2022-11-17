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
        private const float MinBufferSize = 0.02f;

        /// <summary>
        ///     The maximum audio buffer size - if we go above this, reset the audio buffer. Will lead to a bit of noise, but
        ///     reset the audio lag.
        /// </summary>
        private const float MaxBufferSize = 2f * TargetBufferSize;

        /// <summary>
        ///     The target audio buffer size in seconds.
        /// </summary>
        private const float TargetBufferSize = 0.1f;

        /// <summary>
        ///     The maximum divergence in seconds from the <see cref="TargetBufferSize" /> before starting to adjust the pitch.
        /// </summary>
        private const float TargetBufferTolerance = 0.015f;

        /// <summary>
        ///     The maximum pitch change available to move the audio buffer size back towards the <see cref="TargetBufferSize" />.
        /// </summary>
        private const float TargetSizePitchAdjustment = 0.025f;

        /// <summary>
        ///     The maximum amount of zero frames in seconds we wait before resetting the current audio buffer. Uses
        ///     the <see cref="lastPlaybackUpdateTime" /> to determine if we have hit this value.
        /// </summary>
        private const float MaxFrameLossTime = 0.2f;

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

        /// <summary>
        ///     Represents the audio clip buffer used for Unity Playback. The Spatial Clip Data is set to this data every frame.
        ///     Could potentially also be filled asynchronously, if implementation is changed to async.
        /// </summary>
        private float[] _clipBuffer;

        private bool _isDestroying;
        private long _MediaStreamId;
        private ulong _PeerId;

        /// <summary>
        ///     Buffer used to read data from the media stream.
        /// </summary>
        private float[] _readBuffer;

        private string _RoomName;

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
        private float lastPlaybackUpdateTime;

        private PlaybackStream PlaybackMedia;

        internal bool RedirectPlaybackAudio = true;
        private float[] ResampleBuffer;

        private double ResamplerCapacity;

        private AudioClip SpatialClip;
        private float SpatialClipSilenceScale = 1000f;
        private int UnitySampleRate;
        private bool UseResampler;


        /// <summary>
        ///     Number of Samples in the <see cref="SpatialClip" /> used for playback.
        /// </summary>
        private int ClipSamples => SpatialClip.samples;

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

        /// <summary>
        ///     The position in samples of the current playback audio source. Used to determine the current size of the
        ///     audio buffer.
        /// </summary>
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
            bool canRead = !(_isDestroying || PlaybackMedia == null || PlaybackMedia.HasErrors ||
                             RedirectPlaybackAudio == false);
            if (canRead)
            {
                // readBufferSize is based on the fixed unscaled delta time - we want to read "one frame" from the media stream
                int readBufferSize = Mathf.FloorToInt(Time.fixedUnscaledDeltaTime * OutSampleRate);
                if (null == _readBuffer || _readBuffer.Length != readBufferSize)
                    _readBuffer = new float[readBufferSize];

                // read the audio frame from the input data
                uint readResult = PlaybackMedia.AudioReadData(_readBuffer, readBufferSize);
                if (Utility.IsError(readResult))
                {
                    Debug.LogWarning(
                        $"{nameof(PlaybackComponent)} AudioReadData failed with error code {readResult}");
                }
                else
                {
                    // sometimes we get "zero frames" from the media stream - meaning the _readbuffer is filled entirely with zeroes
                    // We want to avoid pushing those zero frames into the play back clip buffer
                    int numZeros = 0;
                    foreach (var entry in _readBuffer)
                        // a float comparison with exactly 0 is slow, but in this case we have to do it, approximations
                        // will not work, because there is potentially data with very low values in the read buffer
                        if (entry == 0)
                            numZeros++;

                    // Only read the data, if there is data in the _readBuffer
                    if (numZeros != readBufferSize)
                    {
                        // write the data into the _clipBuffer.
                        for (int i = 0; i < readBufferSize; i++)
                        {
                            int writePosition = FrameBufferEndPos + i;
                            writePosition %= ClipSamples;
                            _clipBuffer[writePosition] = _readBuffer[i];
                        }

                        // Update the buffer end position
                        FrameBufferEndPos += readBufferSize;
                        FrameBufferEndPos %= ClipSamples;
                        // Update the last time we wrote into the playback clip buffer
                        lastPlaybackUpdateTime = Time.time;
                    }
                }
            }


            int distanceToClipStart = GetBufferDistance(CurrentClipPos, FrameBufferEndPos);
            // The size / duration of the current audio buffer.
            float audioBufferSize = (float)distanceToClipStart / OutSampleRate;

            // Reset the frame buffering, if we haven't received an audio frame for a certain amount of time
            bool shouldResetFrameBuffer = Time.time - lastPlaybackUpdateTime > MaxFrameLossTime;
            shouldResetFrameBuffer |=
                audioBufferSize <
                MinBufferSize; // This is a fixed value - anything below this will lead to audio issues
            shouldResetFrameBuffer |= audioBufferSize > MaxBufferSize;
            if (shouldResetFrameBuffer)
            {
                FrameBufferEndPos = GetTargetFrameBufferEndPosition();
            }

            // We'll adjust the playback source pitch to try and keep the audio buffer size close to the target
            float targetPitch = 1.0f;
            // if the audio buffer size is below the threshold, lower the pitch to allow the media stream input to catch up
            if (audioBufferSize < TargetBufferSize - TargetBufferTolerance)
                targetPitch = 1.0f - TargetSizePitchAdjustment;
            // if the audio buffer size is above the threshold, increase the pitch to allow the clip playback to catch up
            else if (audioBufferSize > TargetBufferSize + TargetBufferTolerance)
                targetPitch = 1.0f + TargetSizePitchAdjustment;

            // Interpolate the pitch over a few frames to avoid sudden pitch jumps.
            float pitch = PlaybackSource.pitch;
            pitch += (targetPitch - pitch) * 0.1f;
            PlaybackSource.pitch = pitch;

            // we also need to clean up any already played data from the clip buffer. Otherwise the playback will loop
            // once no new data is inserted
            int cleanUpCount = GetBufferDistance(FrameBufferEndPos, CurrentClipPos);
            for (int i = 0; i < cleanUpCount; i++)
            {
                int cleanUpIndex = (FrameBufferEndPos + i) % ClipSamples;
                _clipBuffer[cleanUpIndex] = 0.0f;
            }

            // Debug.Log($"Audio Buffer: {audioBufferSize * 1000.0f} ms, Pitch: {pitch}, fixed delta time: {Time.fixedUnscaledDeltaTime}");
            // finally insert the read data into the spatial clip.
            SpatialClip.SetData(_clipBuffer, 0);
        }

        private void OnEnable()
        {
            lastPlaybackUpdateTime = Time.time;
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

            int clipSamples = (int)(OutSampleRate * 3.0f * TargetBufferSize);
            SpatialClip = AudioClip.Create("spatialClip", clipSamples, 1, AudioSettings.outputSampleRate, false);
            ResetAudioClip();
            PlaybackSource.clip = SpatialClip;
            PlaybackSource.loop = true;
            PlaybackSource.Play();

            _clipBuffer = new float[ClipSamples];

            IsFrameBufferEmpty = true;
            FrameBufferEndPos = GetTargetFrameBufferEndPosition();
            FrameBufferEndPos %= ClipSamples;
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


        /// <summary>
        /// Returns the targeted frame buffer end position in time samples. The End position is located <see cref="TargetBufferSize"/> ms
        /// in front of the current playback clip position.
        /// </summary>
        /// <returns>The targeted frame buffer end position in time samples</returns>
        private int GetTargetFrameBufferEndPosition()
        {
            return (int)(CurrentClipPos + TargetBufferSize * OutSampleRate);
        }


        private bool IsBetween(int value, int a, int b)
        {
            bool v1 = b > a && value >= a && value <= b;
            bool v2 = a > b && value >= a && value >= b;
            bool v3 = a > b && value <= a && value <= b;

            return v1 || v2 || v3;
        }

        /// <summary>
        /// The distance (in time samples) between two time samples on the current playback clip.
        /// </summary>
        /// <param name="a">First time sample</param>
        /// <param name="b">Second time sample</param>
        /// <returns>Distance (in time samples) between two time samples</returns>
        private int GetBufferDistance(int a, int b)
        {
            int result = b - a;
            if (result < 0)
                result += ClipSamples;
            return result;
        }

        /// <summary>
        /// Resets the data in the <see cref="SpatialClip"/>.
        /// </summary>
        private void ResetAudioClip()
        {
            SpatialClip.SetData(new float[ClipSamples], 0);
        }
    }
}