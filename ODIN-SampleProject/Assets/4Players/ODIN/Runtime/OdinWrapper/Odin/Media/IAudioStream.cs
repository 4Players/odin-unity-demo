using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OdinNative.Odin.Media
{
    interface IAudioStream
    {
        int GetMediaId();
        ulong GetPeerId();
        void AudioPushData(float[] buffer);
        Task AudioPushDataTask(float[] buffer, CancellationToken cancellationToken);
        void AudioPushDataAsync(float[] buffer);
        int AudioReadData(float[] buffer);
        Task<int> AudioReadDataTask(float[] buffer, CancellationToken cancellationToken);
        Task<int> AudioReadDataAsync(float[] buffer);
    }
}
