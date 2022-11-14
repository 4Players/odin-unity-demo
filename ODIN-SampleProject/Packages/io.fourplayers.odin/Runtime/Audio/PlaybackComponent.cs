using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private float[] AsyncClipBuffer;
        
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


        private void OnEnable()
        {
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

            ClipSamples = OutSampleRate * 3;
            SpatialClip = AudioClip.Create("spatialClip", ClipSamples, 1, AudioSettings.outputSampleRate, false);
            ResetAudioClip();
            PlaybackSource.clip = SpatialClip;
            PlaybackSource.loop = true;
            PlaybackSource.Play();

            AudioFrameData = new float[960];
            AsyncClipBuffer = new float[ClipSamples];
            
            IsFrameBufferEmpty = true;
            FrameBufferEndPos = (int)(CurrentClipPos + 0.1f * OutSampleRate);
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

        private void FixedUpdate()
        {
            bool canRead = !(_isDestroying || PlaybackMedia == null || PlaybackMedia.HasErrors ||
                             RedirectPlaybackAudio == false);

            if (canRead)
            {
                int readBufferSize = Mathf.FloorToInt(Time.fixedUnscaledDeltaTime * OutSampleRate);
                float[] readBuffer = new float[readBufferSize];
                uint readResult = PlaybackMedia.AudioReadData(readBuffer);
                
                
                int numZeros = 0;
                for (var i = 0; i < readBuffer.Length; i++)
                {
                    float entry = readBuffer[i];
                    if (Mathf.Approximately(0,entry))
                    {
                        numZeros++;
                    }
                }
                
                
                if (numZeros > 0 && numZeros != readBufferSize)
                {
                    if (numZeros != readBufferSize)
                    {
                        Debug.Log($"Buffer length: {readBuffer.Length}, found zeros: {numZeros}");
                    }
                    
                    FrameBufferEndPos = (int)(CurrentClipPos + 0.02f * OutSampleRate);
                }
                
                

                if (Utility.IsError(readResult))
                    Debug.LogWarning(
                        $"{nameof(PlaybackComponent)} AudioReadData failed with error code {readResult}");
                else
                {

                    // Debug.Log($"Readresult: {readResult}, readBuffer length: {readBuffer.Length}, FrameBuffer End Pos: {FrameBufferEndPos}, OutputSampleRade: {AudioSettings.outputSampleRate}");
  
                    for (int i = 0; i < readBuffer.Length; i++)
                    {
                        int writePosition = FrameBufferEndPos + i;
                        writePosition %= ClipSamples;
                        AsyncClipBuffer[writePosition] = readBuffer[i];
                    }

                    FrameBufferEndPos += readBufferSize;
                    FrameBufferEndPos %= ClipSamples;
                }

               
            }

            SpatialClip.SetData(AsyncClipBuffer, 0);

            PreviousClipPos = CurrentClipPos;
        }


        // private void FixedUpdate()
        // {
        //    
        //     // let's check if our frame buffer is empty. This is the case e.g. when the current clip sample position
        //     // went beyond our Frame Buffer end position -->  Previous.... FrameBufferEndPos ... CurrentClipPos
        //     // Checking this ensures that this happened during the last audio update
        //     if (!IsFrameBufferEmpty && IsBetween(FrameBufferEndPos, PreviousClipPos, CurrentClipPos))
        //     {
        //         IsFrameBufferEmpty = true;
        //         Debug.Log("Reset Frame Buffer");
        //     }
        //
        //     // string log =
        //     //     $"Has Errors: {PlaybackMedia.HasErrors} Active: {PlaybackMedia.IsActive} Invalid: {PlaybackMedia.IsInvalid} Muted: {PlaybackMedia.IsMuted} Paused: {PlaybackMedia.IsPaused}";
        //     // Debug.Log(log);
        //
        //     bool canRead = !(_isDestroying || PlaybackMedia == null || PlaybackMedia.HasErrors ||
        //                      PlaybackMedia.IsMuted ||
        //                      RedirectPlaybackAudio == false);
        //
        //     if (canRead)
        //         ReadAudioFrames();
        //     else
        //         Debug.Log("Not reading audio stream data for some reason.");
        //
        //     // we're dynamically cleaning up behind ourselves. Use the last clip position and the current position
        //     int cleanUpLength = GetBufferDistance(PreviousClipPos, CurrentClipPos);
        //     if (cleanUpLength > 0)
        //     {
        //         // but let's actually use two times the clean up length, to have some overlapping in the cleaning from one 
        //         // frame to another - otherwise there could sometimes be residual samples left in the clip, leading to noises.
        //         int startPos = PreviousClipPos - cleanUpLength;
        //         while (startPos < 0)
        //             startPos += ClipSamples;
        //         
        //         SpatialClip.SetData(new float[cleanUpLength * 2], startPos);
        //     }
        //
        //     // Debug.Log($"Clean up Length is: {cleanUpLength}");
        //     PreviousClipPos = CurrentClipPos;
        //
        // }
        
        
        
        
        // /// <summary>
        // ///     We're looking at this in terms of Audio Frames - each time we call PlaybackMedia.AudioReadData, the resulting
        // ///     data is one Audio Frame. We store audio frames in the Spatial Audio Clip, which is then used by Unity to play back
        // ///     the Odin Media stream.
        // ///     To avoid stuttering, we use an audio frame buffer. When first receiving an audio frame, we write the
        // ///     received frame data into the clip with an offset (e.g. 60ms). When we receive the next audio frames from the ODIN
        // ///     servers,
        // ///     we concatenate those frames into the position given by FrameBufferEndPos.
        // /// </summary>
        // private void ReadAudioFrames()
        // {
        //     // Debug.Log($"Frame: {Time.renderedFrameCount}, Audio Data Length: {audioDataLength}, Time: {Time.time}");
        //     // read stream data
        //     uint readResult = PlaybackMedia.AudioReadData(AudioFrameData);
        //     // If we get an error, log and abort
        //     if (Utility.IsError(readResult))
        //         Debug.LogWarning(
        //             $"{nameof(PlaybackComponent)} AudioReadData failed with error code {readResult}");
        //
        //     bool containsData = false;
        //     foreach (float value in AudioFrameData)
        //         if (value != 0)
        //             containsData = true;
        //
        //     while (containsData)
        //     {
        //         // if this is the first audio frame we received since the stream last stopped sending, start buffering audio frames
        //         if (IsFrameBufferEmpty)
        //         {
        //             // int secureBufferOffset = (int) Mathf.Max(InitialBufferOffset, Time.fixedDeltaTime * 3.0f);
        //             // Wait for InitialBufferOffset seconds before writing the first audio frame
        //             FrameBufferEndPos = CurrentClipPos + (int)(InitialBufferOffset * OutSampleRate);
        //             FrameBufferEndPos %= ClipSamples;
        //
        //             IsFrameBufferEmpty = false;
        //         }
        //
        //         // Debug.Log($"Writing data to position {FrameBufferEndPos}, new last pos will be {(FrameBufferEndPos + StreamData.Length) % ClipSamples}");
        //
        //         // Write the audio frame data into the audio clip and update frame buffer end position.
        //         SpatialClip.SetData(AudioFrameData, FrameBufferEndPos);
        //         FrameBufferEndPos += AudioFrameData.Length;
        //         FrameBufferEndPos %= ClipSamples;
        //         
        //         readResult = PlaybackMedia.AudioReadData(AudioFrameData);
        //         // If we get an error, log and abort
        //         if (Utility.IsError(readResult))
        //             Debug.LogWarning(
        //                 $"{nameof(PlaybackComponent)} AudioReadData failed with error code {readResult}");
        //
        //         containsData = false;
        //         foreach (float value in AudioFrameData)
        //             if (value != 0)
        //                 containsData = true;
        //     }
        // }

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