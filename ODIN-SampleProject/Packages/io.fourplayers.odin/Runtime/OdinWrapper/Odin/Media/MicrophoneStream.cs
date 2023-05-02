using OdinNative.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OdinNative.Odin.Media
{
    /// <summary>
    /// Representation of a write only audio stream
    /// </summary>
    public class MicrophoneStream : MediaStream
    {
        internal MicrophoneStream()
            : base(new OdinMediaConfig(OdinDefaults.RemoteSampleRate, OdinDefaults.RemoteChannels))  // Defaults match server config
        { }

        internal MicrophoneStream(OdinMediaConfig config)
            : base(config)
        { }

        /// <summary>
        /// AudioReadData and AudioReadDataAsync are not supported!
        /// </summary>
        /// <remarks>This will only work for remote/output streams.</remarks>
        /// <param name="stats">Audio stream statistics</param>
        /// <exception cref="OdinWrapperException"></exception>
        /// <returns>throws OdinWrapperException</returns>
        public override bool AudioStats(out Core.Imports.NativeBindings.OdinAudioStreamStats stats)
        {
            throw new OdinWrapperException("Microphone streams are always local! This will only work for remote/output streams.",
                new NotSupportedException("AudioStats, AudioReadData, AudioReadDataTask and AudioReadDataAsync are not supported!"));
        }
        /// <summary>
        /// AudioReadData is not supported!
        /// </summary>
        /// <remarks>Microphone streams are always writeonly</remarks>
        /// <param name="buffer"></param>
        /// <exception cref="OdinWrapperException"></exception>
        /// <returns>throws OdinWrapperException</returns>
        public override uint AudioReadData(float[] buffer)
        {
            throw new OdinWrapperException("Microphone streams are always writeonly!",
                new NotSupportedException("AudioStats, AudioReadData, AudioReadDataTask and AudioReadDataAsync are not supported!"));
        }
        /// <summary>
        /// AudioReadDataTask is not supported!
        /// </summary>
        /// <remarks>Microphone streams are always writeonly</remarks>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="OdinWrapperException"></exception>
        /// <returns>throws OdinWrapperException</returns>
        public override Task<uint> AudioReadDataTask(float[] buffer, CancellationToken cancellationToken)
        {
            throw new OdinWrapperException("Microphone streams are always writeonly!",
                new NotSupportedException("AudioStats, AudioReadData, AudioReadDataTask and AudioReadDataAsync are not supported!"));
        }
        /// <summary>
        /// AudioReadDataAsync is not supported!
        /// </summary>
        /// <remarks>Microphone streams are always writeonly</remarks>
        /// <param name="buffer"></param>
        /// <exception cref="OdinWrapperException"></exception>
        /// <returns>throws OdinWrapperException</returns>
        public override Task<uint> AudioReadDataAsync(float[] buffer)
        {
            throw new OdinWrapperException("Microphone streams are always writeonly!",
                new NotSupportedException("AudioStats, AudioReadData, AudioReadDataTask and AudioReadDataAsync are not supported!"));
        }
    }
}
