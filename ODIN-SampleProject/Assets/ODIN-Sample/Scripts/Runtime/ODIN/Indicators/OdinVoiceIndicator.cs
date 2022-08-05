using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    ///     Changes Color of <see cref="indicationTarget"/> based on whether media is currently transmitting or not.
    /// </summary>
    public class OdinVoiceIndicator : OdinVoiceIndicatorBase
    {
        /// <summary>
        ///     This renderers color will be switched to <see cref="voiceOnColor" />, if the remote player is transmitting. The
        ///     color will return back to the original color of the main materials' initial color.
        /// </summary>
        [SerializeField] private Renderer indicationTarget;

        /// <summary>
        ///     The color the <see cref="indicationTarget" /> should display when the remote player is transmitting.
        /// </summary>
        [ColorUsage(true, true)] [SerializeField]
        private Color voiceOnColor = Color.green;

        private Color _originalColor;

        protected override void Awake()
        {
            base.Awake();
            if (null == indicationTarget)
                indicationTarget = GetComponent<Renderer>();
            Assert.IsNotNull(indicationTarget);

            _originalColor = indicationTarget.material.color;
        }

        protected override void UpdateFeedback(bool isVoiceOn)
        {
            if (null == indicationTarget)
                indicationTarget = GetComponent<Renderer>();
            if (isVoiceOn)
                indicationTarget.material.color = voiceOnColor;
            else
                indicationTarget.material.color = _originalColor;
        }
    }
}