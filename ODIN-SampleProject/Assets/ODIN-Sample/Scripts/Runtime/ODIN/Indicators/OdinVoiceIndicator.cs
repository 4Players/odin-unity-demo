using System.Collections;
using OdinNative.Odin.Peer;
using OdinNative.Odin.Room;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Odin.Indicators
{
    /// <summary>
    ///     Behaviour for displaying feedback on whether the remote player represented by the <see cref="adapter" /> script
    ///     is currently transmitting in the ODIN room with the name <see cref="odinRoomName" />, by changing the color of a
    ///     mesh.
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
            if (isVoiceOn)
                indicationTarget.material.color = voiceOnColor;
            else
                indicationTarget.material.color = _originalColor;
        }
    }
}