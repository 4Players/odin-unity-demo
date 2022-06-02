using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Slider = UnityEngine.UI.Slider;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    [ExecuteInEditMode]
    public class SliderDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;
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