namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    ///     Contains the effect data like Volume, CutoffFrequency and LowpassResonanceQ.
    /// </summary>
    public struct AudioEffectData
    {
        /// <summary>
        ///     The Volume that should be applied to the audio source, Range [0,...]
        /// </summary>
        public float Volume;

        /// <summary>
        ///     The lowpass cutofffrequency, Range [0,22000]
        /// </summary>
        public float CutoffFrequency;

        /// <summary>
        ///     The Lowpass Resonance Q that should be applied to the audio source. Simulates the sound transferance rate through a
        ///     material.
        ///     Range [0,...]
        /// </summary>
        public float LowpassResonanceQ;

        /// <summary>
        ///     Returns false, if the effect will not change the perceived audio from the source. E.g. if the volume is set to 1.0f
        ///     or
        ///     if the cutofffrequency is above or equal to 22000 Hz.
        /// </summary>
        public bool IsEffectAudible =>
            Volume < 0.99f || Volume > 1.01f ||
            CutoffFrequency < 22000 ||
            LowpassResonanceQ < 0.99f ||
            LowpassResonanceQ > 1.01f;


        /// <summary>
        ///     Returns a default object, containing effect values that are not perceivable.
        /// </summary>
        public static AudioEffectData Default =>
            new()
            {
                Volume = 1.0f,
                CutoffFrequency = 22000.0f,
                LowpassResonanceQ = 1.0f
            };
    }
}