using OdinNative.Core;
using OdinNative.Odin;
using OdinNative.Odin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            .Medias[MediaId] as PlaybackStream;

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
        private ushort _MediaId;
        /// <summary>
        /// Media id for this playback. Change this value to pick a PlaybackStream by media id from peers Medias.
        /// </summary>
        /// <remarks>Invalid values will cause errors.</remarks>
        public ushort MediaId
        {
            get { return _MediaId; }
            set
            {
                _MediaId = value;
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

        internal bool RedirectPlaybackAudio = true;
        //State of InvokeRepeating
        private bool _RedirectingPlaybackAudio = false;
        private const float RedirectPlaybackDelay = 0.5f;
        private const float RedirectPlaybackInterval = 0.02f; // Min 2x CacheSize ms

        private Utility.RollingAverage avgFps;
        private float fps;
        
        private int AudioClipIndex;
        private readonly float[] ReadBuffer = new float[Utility.RateToSamples(MediaSampleRate.Hz48000, 20)]; // 48kHz * 20ms 
        private readonly int CacheMultiplier = 6; // 20ms * 6
        private int CacheSize; // 80ms * 48kHz

        /// <summary>
        /// Use set <see cref="Channels"/> on true, <see cref="OdinEditorConfig.RemoteChannels"/> on false
        /// </summary>
        public bool OverrideChannels;
        /// <summary>
        /// The playback <see cref="OdinNative.Core.MediaChannels"/>
        /// </summary>
        /// <remarks>Set value is ignored on 
        /// <see cref="UnityEngine.AudioClip"/> <see href="https://docs.unity3d.com/ScriptReference/AudioClip.html">(AudioClip)</see>
        /// creation if <see cref="OverrideChannels"/> is false</remarks>
        public MediaChannels Channels;

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
                if(PlaybackMedia == null)
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

            AudioClipIndex = 0;
            CacheSize = ReadBuffer.Length * CacheMultiplier;

            CreateClip();
        }

        private void CreateClip()
        {
            if (OverrideChannels == false) Channels = OdinHandler.Config.RemoteChannels;
            if (OverrideSampleRate == false) SampleRate = OdinHandler.Config.RemoteSampleRate;

            if (Channels != MediaChannels.Mono) Debug.LogWarning("Odin-Server assert Mono-Channel missmatch, continue anyway...");
            if (SampleRate != MediaSampleRate.Hz48000) Debug.LogWarning("Odin-Server assert 48k-SampleRate missmatch, continue anyway...");
            PlaybackSource.clip = AudioClip.Create($"({PlaybackSource.gameObject.name}) {nameof(PlaybackComponent)}",
                CacheSize,
                (int)Channels,
                (int)SampleRate, false);
        }

        void OnEnable()
        {
            if (PlaybackSource.isPlaying == false)
            {
                PlaybackSource.Play();
                float[] samples = new float[CacheSize];
                PlaybackSource.clip.SetData(samples, 0);
                AudioClipIndex = PlaybackSource.timeSamples;
            }

            RedirectPlaybackAudio = true;
        }

        void Reset()
        {
            OverrideChannels = false;
            Channels = OdinHandler.Config.RemoteChannels;

            OverrideSampleRate = false;
            SampleRate = OdinHandler.Config.RemoteSampleRate;
        }

        void Start()
        {
            avgFps = new Utility.RollingAverage(16, Application.targetFrameRate);
        }

        void Update()
        {
            fps = 1 / Time.unscaledDeltaTime;
            avgFps.Update(fps);

            CheckRedirectAudio();
        }

        private void CheckRedirectAudio()
        {
            if (RedirectPlaybackAudio && _RedirectingPlaybackAudio == false)
            {
                InvokeRepeating("Flush", RedirectPlaybackDelay, RedirectPlaybackInterval);
                _RedirectingPlaybackAudio = true;
                if (PlaybackSource.isPlaying == false)
                {
                    PlaybackSource.Play();
                    float[] samples = new float[CacheSize];
                    PlaybackSource.clip.SetData(samples, 0);
                    AudioClipIndex = PlaybackSource.timeSamples;
                }
            }
            else if (RedirectPlaybackAudio == false && _RedirectingPlaybackAudio)
            {
                CancelInvoke("Flush");
                _RedirectingPlaybackAudio = false;
                PlaybackSource.Stop();
                PlaybackSource.clip.SetData(new float[CacheSize], 0);
                AudioClipIndex = 0;
            }
        }

        private void RecalculateCacheByFps()
        {
            double fps = avgFps.GetAverage();
            int newCacheSize = Math.Max(ReadBuffer.Length * CacheMultiplier, ReadBuffer.Length * CacheMultiplier + (int)(ReadBuffer.Length * Math.Ceiling(2 - fps * RedirectPlaybackInterval)));
            // Unity calls on 5 or less fps flush ~twice
            if (fps < 5)
                newCacheSize = newCacheSize * 13; // 1500ms
            else if (fps < 12)
                newCacheSize = newCacheSize * 2; //  240ms

            if (CacheSize != newCacheSize)
            {
                CacheSize = newCacheSize;
                PlaybackSource.clip = AudioClip.Create($"({PlaybackSource.gameObject.name}) {nameof(PlaybackComponent)}",
                    CacheSize,
                    (int)Channels,
                    (int)SampleRate, false);
                PlaybackSource.Play();
                float[] samples = new float[CacheSize];
                PlaybackSource.clip.SetData(samples, 0);
                AudioClipIndex = PlaybackSource.timeSamples;
            }
        }

        private void Flush()
        {
            if (PlaybackMedia == null || PlaybackMedia.IsMuted || _RedirectingPlaybackAudio == false) return;

            int invalidated = PlaybackSource.timeSamples - AudioClipIndex;
            if (invalidated < 0)
            {
                // wrapped around
                invalidated += CacheSize;
            }

            int offset = AudioClipIndex;
            int end = AudioClipIndex + invalidated - ReadBuffer.Length;
            for (offset = AudioClipIndex; offset < end; offset += ReadBuffer.Length)
            {
                int n = PlaybackMedia.AudioReadData(ReadBuffer);
                PlaybackSource.clip.SetData(ReadBuffer, offset % CacheSize);
                if (n != ReadBuffer.Length)
                {
                    Debug.LogWarning("Playback failed getting samples from backend");
                }
            }
            AudioClipIndex = offset % CacheSize;
            if (invalidated >= 960)
                RecalculateCacheByFps();
        }

        void OnDisable()
        {
            CancelInvoke();
            PlaybackSource.Stop();
            AudioClipIndex = 0;
            RedirectPlaybackAudio = false;
        }

        private void OnDestroy()
        {
            if (AutoDestroyAudioSource)
                Destroy(PlaybackSource);

            OdinHandler.Instance.Client?
            .Rooms[RoomName]?
            .RemotePeers[PeerId]?
            .Medias.Free(MediaId);
        }
    }
}
