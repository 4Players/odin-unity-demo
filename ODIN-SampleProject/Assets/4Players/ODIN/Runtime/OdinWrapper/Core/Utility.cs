using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdinNative.Core
{
    public static class Utility
    {
        /// <summary>
        /// Representative ErrorCode for Ok.
        /// </summary>
        public const uint OK = 0;

        public static int RateToSamples(MediaSampleRate sampleRate = MediaSampleRate.Hz48000, int milliseconds = 20)
        {
            return ((int)sampleRate / 1000) * milliseconds;
        }

        /// <summary>
        /// Determines if the specified error code identifies an actual error.
        /// </summary>
        /// <param name="error">error code</param>
        /// <returns>true if error</returns>
        public static bool IsError(int error)
        {
            return ((UInt32)error >> 29) > 0;
        }

#if UNITY_STANDALONE || UNITY_EDITOR || ENABLE_IL2CPP || ENABLE_MONO
        [Obsolete("Future versions of Unity are expected to always throw exceptions and not have Assertions.Assert._raiseExceptions https://docs.unity3d.com/ScriptReference/Assertions.Assert.html")]
#endif
        internal static void Throw(Exception e, bool assertion = false)
        {
#if !UNITY_STANDALONE && !UNITY_EDITOR && !ENABLE_IL2CPP && !ENABLE_MONO
            if (assertion)
                Debug.WriteLine(e.ToString());
            else
                throw e;
#else
            if (assertion)
                UnityEngine.Debug.LogAssertion(e);
            else
                UnityEngine.Debug.LogException(e);
#endif
        }

        [Conditional("DEBUG"), Conditional("UNITY_ASSERTIONS"), Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        internal static void Assert(bool condition, string message)
        {
            if (condition) return;
#pragma warning disable CS0618 // Type or member is obsolete
            Utility.Throw(new Odin.OdinWrapperException(message), true);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public class RollingAverage
        {
            private readonly int pow;
            private readonly float?[] values;
            private int nextIndex = -1;

            public RollingAverage(int maxFactor = 16, float defaultValue = 0)
            {
                pow = maxFactor - 1;
                if (maxFactor == 0 || (maxFactor & pow) != 0)
                {
                    throw new ArgumentException("Must be power of two", nameof(maxFactor));
                }
                values = new float?[maxFactor];
                for(int i = 0; i < maxFactor; i++)
                    values[i] = defaultValue;
            }

            public void Update(float newValue)
            {
                var index = System.Threading.Interlocked.Increment(ref nextIndex) & pow;
                values[index] = newValue;
            }

            public double GetAverage()
            {
                return values.TakeWhile(x => x.HasValue)
                    .Select(x => x ?? 0)
                    .DefaultIfEmpty(0)
                    .Average();
            }
        }
    }
}
