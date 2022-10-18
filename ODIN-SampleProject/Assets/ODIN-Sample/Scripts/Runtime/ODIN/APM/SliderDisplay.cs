using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Slider = UnityEngine.UI.Slider;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    /// <summary>
    /// Updates a display text based on a slider value.
    /// </summary>
    [ExecuteInEditMode]
    public class SliderDisplay : MonoBehaviour
    {
        /// <summary>
        /// Reference to the display text.
        /// </summary>
        [SerializeField] private TMP_Text displayText;
        /// <summary>
        /// Reference to the slider.
        /// </summary>
        [SerializeField] private Slider slider;

        private void Awake()
        {
            Assert.IsNotNull(displayText);
            Assert.IsNotNull(slider);
        }

        private void OnEnable()
        {
            OnSliderValueChanged(slider.value);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float newValue)
        {
            displayText.text = newValue.ToString("0.00");
        }
    }
}