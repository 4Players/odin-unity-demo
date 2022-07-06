﻿using OdinNative.Core;
using OdinNative.Core.Handles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OdinNative.Odin.Media
{
    /// <summary>
    /// Base stream
    /// </summary>
    /// <remarks>Video is currently not supported</remarks>
    public abstract class MediaStream : IVideoStream, IAudioStream, IDisposable
    {
        /// <summary>
        /// Handle ID
        /// </summary>
        public long Id { get; internal set; }
        /// <summary>
        /// Internal ID of the media stream
        /// </summary>
        internal ushort MediaId => GetMediaId();
        internal ulong PeerId => GetPeerId();
        /// <summary>
        /// Audio config of the media stream
        /// </summary>
        public OdinMediaConfig MediaConfig { get; private set; }
        /// <summary>
        /// Control of async read and write tasks
        /// </summary>
        public CancellationTokenSource CancellationSource { get; internal set; }
        /// <summary>
        /// Indicates wether or not the media stream is muted
        /// </summary>
        /// <remarks>If true, no data will be read/pushed for the media handle</remarks>
        public bool IsMuted { get; set; }
        /// <summary>
        /// Indicates wether or not the media stream is active and sending/receiving data
        /// </summary>
        public bool IsActive { get; internal set; }

        private StreamHandle Handle;
        private ResamplerHandle ResamplerHandle;

        /// <summary>
        /// Base stream
        /// </summary>
        /// <param name="config">audio stream configuration</param>
        public MediaStream(OdinMediaConfig config)
            : this(config, OdinLibrary.Api.AudioStreamCreate(config))
        { }

        internal MediaStream(OdinMediaConfig config, StreamHandle handle)
        {
            Id = ((IntPtr)handle).ToInt64();
            MediaConfig = config;
            Handle = handle;
            CancellationSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Returns the media stream ID and updates <see cref="Id"/>.
        /// </summary>
        /// <returns>id value</returns>
        public ushort GetMediaId()
        {
            OdinLibrary.Api.MediaStreamMediaId(Handle, out ushort mediaId);
            return mediaId;
        }

        /// <summary>
        /// Returns the ID of the peer that own this media stream.
        /// </summary>
        /// <returns>id</returns>
        public ulong GetPeerId()
        {
            OdinLibrary.Api.MediaStreamPeerId(Handle, out ulong peerId);
            return peerId;
        }

        /// <summary>
        /// Sets <see cref="IsMuted"/>.
        /// </summary>
        /// <param name="mute">true for NOP or false to call ffi on read/write</param>
        public void SetMute(bool mute)
        {
            IsMuted = mute;
        }

        /// <summary>
        /// Toggles <see cref="IsMuted"/>.
        /// </summary>
        public void ToggleMute()
        {
            IsMuted = !IsMuted;
        }

        /// <summary>
        /// Adds this media stream to a room.
        /// </summary>
        /// <param name="roomHandle"><see cref="Room.Room"/> handle</param>
        /// <returns>true on success or false</returns>
        internal bool AddMediaToRoom(RoomHandle roomHandle)
        {
            return OdinLibrary.Api.RoomAddMedia(roomHandle, Handle) == Utility.OK;
        }

        /// <summary>
        /// Sends data to the audio stream. The data has to be interleaved [-1, 1] float data.
        /// </summary>
        /// <remarks>if <see cref="IsMuted"/> NOP</remarks>
        /// <param name="buffer">audio data</param>
        public virtual void AudioPushData(float[] buffer)
        {
            if (IsMuted) return;

            OdinLibrary.Api.AudioPushData(Handle, buffer, buffer.Length);
        }

        /// <summary>
        /// Sends data to the audio stream. The data has to be interleaved [-1, 1] float data.
        /// </summary>
        /// <remarks>if <see cref="IsMuted"/> NOP</remarks>
        /// <param name="buffer">audio data</param>
        /// <param name="length">bytes to write</param>
        public virtual void AudioPushData(float[] buffer, int length)
        {
            if (IsMuted) return;

            OdinLibrary.Api.AudioPushData(Handle, buffer, length);
        }

        /// <summary>
        /// Sends data to the audio stream. The data has to be interleaved [-1, 1] float data.
        /// </summary>
        /// <remarks>if <see cref="IsMuted"/> NOP</remarks>
        /// <param name="buffer">interleaved audio data</param>
        /// <param name="cancellationToken"></param>
        public virtual Task AudioPushDataTask(float[] buffer, CancellationToken cancellationToken)
        {
            if (IsMuted) return Task.CompletedTask;

            return Task.Factory.StartNew(() => {
                OdinLibrary.Api.AudioPushData(Handle, buffer, buffer.Length);
            }, cancellationToken);
        }

        /// <summary>
        /// Sends audio data using a custom <see cref="CancellationSource"/>.
        /// The data has to be interleaved [-1, 1] float data.
        /// </summary>
        /// <remarks>if <see cref="IsMuted"/> NOP</remarks>
        /// <param name="buffer">interleaved audio data</param>
        public virtual async void AudioPushDataAsync(float[] buffer)
        {
            await AudioPushDataTask(buffer, CancellationSource.Token);
        }

        /// <summary>
        /// Reads data from the audio stream.
        /// </summary>
        /// <remarks>if <see cref="IsMuted"/> NOP</remarks>
        /// <param name="buffer">buffer to write into</param>
        /// <returns>count of written bytes into buffer</returns>
        public virtual uint AudioReadData(float[] buffer)
        {
            if (IsMuted) return 0;

            return OdinLibrary.Api.AudioReadData(Handle, buffer, buffer.Length);
        }

        /// <summary>
        /// Reads data from the audio stream.
        /// </summary>
        /// <remarks>if <see cref="IsMuted"/> NOP</remarks>
        /// <param name="buffer">buffer to write into</param>
        /// <param name="length">bytes to read</param>
        /// <returns>count of written bytes into buffer</returns>
        public virtual uint AudioReadData(float[] buffer, int length)
        {
            if (IsMuted) return 0;

            return OdinLibrary.Api.AudioReadData(Handle, buffer, length);
        }

        /// <summary>
        /// Reads data from the audio stream.
        /// </summary>
        /// <remarks>if <see cref="IsMuted"/> NOP</remarks>
        /// <param name="buffer">buffer to write into</param>
        /// <param name="cancellationToken"></param>
        /// <returns>count of written bytes into buffer</returns>
        public virtual Task<uint> AudioReadDataTask(float[] buffer, CancellationToken cancellationToken)
        {
            if (IsMuted) return Task.FromResult<uint>(0);

            return Task.Factory.StartNew(() => {
                return OdinLibrary.Api.AudioReadData(Handle, buffer, buffer.Length);
            }, cancellationToken);
        }

        /// <summary>
        /// Read audio data using a custom custom <see cref="CancellationSource"/>.
        /// </summary>
        /// <param name="buffer">buffer to write into</param>
        /// <returns>count of written bytes into buffer</returns>
        public virtual async Task<uint> AudioReadDataAsync(float[] buffer)
        {
            return await AudioReadDataTask(buffer, CancellationSource.Token);
        }

        /// <summary>
        /// Get the number of samples available in the audio buffer.
        /// </summary>
        /// <returns>floats available to read with <see cref="AudioReadData(float[])"/></returns>
        public virtual uint AudioDataLength()
        {
            return OdinLibrary.Api.AudioDataLength(Handle);
        }

        /// <summary>
        /// Set a resampler and resamples a chunk of audio.
        /// </summary>
        /// <remarks>This is intended for situations where the audio output pipeline doesn't support 48kHz.</remarks>
        /// <param name="input">current buffer</param>
        /// <param name="outputSampleRate">resample samplerate</param>
        /// <param name="output">resampled buffer</param>
        /// <param name="capacity">size of resampled buffer</param>
        /// <remarks>The current buffer samplesrate is expected to be <see cref="OdinNative.Odin.OdinDefaults.RemoteSampleRate"/> with <see cref="OdinNative.Odin.OdinDefaults.RemoteChannels"/></remarks>
        /// <returns>true on success or false if missmatch of capacity i.e. errorcode</returns>
        public virtual bool AudioResample(float[] input, uint outputSampleRate, out float[] output, out int capacity)
        {
            if (ResamplerHandle == null)
                ResamplerHandle = OdinLibrary.Api.ResamplerCreate((uint)OdinDefaults.RemoteSampleRate, outputSampleRate, (short)OdinDefaults.RemoteChannels);
            else if (ResamplerHandle.ToRate != outputSampleRate)
            {
                ResamplerHandle.Dispose();
                ResamplerHandle = null;
                return AudioResample(input, outputSampleRate, out output, out capacity);
            }

            output = new float[((ResamplerHandle.ToRate / ResamplerHandle.FromRate) * input.Length) * ResamplerHandle.Channels];
            capacity = output.Length;
            uint ret = OdinLibrary.Api.ResamplerProcess(ResamplerHandle, input, input.Length, output, ref capacity);
            return ret == capacity;
        }

        /// <summary>
        /// Set a resampler and resamples a chunk of audio.
        /// </summary>
        /// <remarks>This is intended for situations where the audio output pipeline doesn't support 48kHz.</remarks>
        /// <param name="input">source buffer</param>
        /// <param name="outputSampleRate">to samplerate</param>
        /// <param name="output">target buffer</param>
        /// <param name="capacity">target capacity</param>
        /// <returns>sample count on success or errorcode on failure</returns>
        internal virtual uint AudioResample(float[] input, uint outputSampleRate, float[] output, int capacity)
        {
            if (ResamplerHandle == null)
                ResamplerHandle = OdinLibrary.Api.ResamplerCreate((uint)OdinDefaults.RemoteSampleRate, outputSampleRate, (short)OdinDefaults.RemoteChannels);
            else if (ResamplerHandle.ToRate != outputSampleRate)
            {
                ResamplerHandle.Dispose();
                ResamplerHandle = null;
                return AudioResample(input, outputSampleRate, output, capacity);
            }

            return OdinLibrary.Api.ResamplerProcess(ResamplerHandle, input, input.Length, output, ref capacity);
        }

        /// <summary>
        /// Cancel the custom <see cref="CancellationSource"/>.
        /// </summary>
        /// <returns>true if the token was canceled or false</returns>
        public bool Cancel()
        {
            if (CancellationSource.Token.CanBeCanceled)
            {
                CancellationSource.Cancel();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Debug
        /// </summary>
        /// <returns>info</returns>
        public override string ToString()
        {
            return $"{nameof(MediaStream)}: {nameof(Id)} {Id}, {nameof(MediaId)} {MediaId}, {nameof(PeerId)} {PeerId}, {nameof(IsMuted)} {IsMuted}\n\t- {nameof(MediaConfig)} {MediaConfig?.ToString()}";
        }

        private bool disposedValue;
        /// <summary>
        /// On dispose will free the stream and resampler
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CancellationSource?.Dispose();
                    MediaConfig = null;
                    Handle?.Close();
                    Handle = null;
                    ResamplerHandle?.Close();
                    ResamplerHandle = null;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Default deconstructor
        /// </summary>
        ~MediaStream()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// On dispose will free the stream and resampler
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
