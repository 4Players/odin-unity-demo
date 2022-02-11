using OdinNative.Core;
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
    /// Representation of a read only audio stream
    /// </summary>
    public class PlaybackStream : MediaStream
    {
        internal PlaybackStream(ushort id, OdinMediaConfig config, StreamHandle stream) 
            : base(id, config, stream)
        { }

        internal PlaybackStream(ushort id, StreamHandle stream)
            : base(id, new OdinMediaConfig(OdinDefaults.RemoteSampleRate, OdinDefaults.RemoteChannels), stream) // Defaults match server config
        { }

        public override void AudioPushData(float[] buffer)
        {
            throw new OdinWrapperException("Remote streams are always readonly!",
                new NotSupportedException("AudioPushData, AudioPushDataTask and AudioPushDataAsync are not supported!"));
        }

        public override Task AudioPushDataTask(float[] buffer, CancellationToken cancellationToken)
        {
            throw new OdinWrapperException("Remote streams are always readonly!",
                new NotSupportedException("AudioPushData, AudioPushDataTask and AudioPushDataAsync are not supported!"));
        }

        public override void AudioPushDataAsync(float[] buffer)
        {
            throw new OdinWrapperException("Remote streams are always readonly!",
                new NotSupportedException("AudioPushData, AudioPushDataTask and AudioPushDataAsync are not supported!"));
        }
    }
}
