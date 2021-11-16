using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace ODIN_Sample.Scripts.Runtime.ThirdPerson
{
    public class ToggleRadioBehaviour : MonoBehaviour
    {
        [SerializeField] private string toggleButton = "Toggle";

        [SerializeField] private AudioSource toggleSource;
        [SerializeField] private Renderer feedbackMesh;
        [SerializeField] private int feedbackMaterialIndex = 1;
        [SerializeField] private Color radioOffColor = Color.red;

        public UnityEvent<bool> onRadioToggled;

        private Color _originalColor;

        private void Awake()
        {
            Assert.IsNotNull(toggleSource, "Missing reference to toggleSource.");
            Assert.IsNotNull(feedbackMesh, "Missing reference to feedback mesh.");
            Assert.IsTrue(feedbackMesh.materials.Length > feedbackMaterialIndex,
                "Invalid feedbackMaterialIndex: feedbackMesh does not have that many material slots.");
            
            _originalColor = feedbackMesh.materials[feedbackMaterialIndex].color;
            UpdateFeedbackColor();

        }

        private void Update()
        {
            if (Input.GetButtonDown(toggleButton))
            {
                ToggleRadio();
            }
        }

        public void ToggleRadio()
        {
            SetRadio(!toggleSource.gameObject.activeSelf);
            onRadioToggled.Invoke(toggleSource.gameObject.activeSelf);
        }

        public void SetRadio(bool newActive)
        {
            toggleSource.gameObject.SetActive(newActive);
            UpdateFeedbackColor();
        }

        private void UpdateFeedbackColor()
        {
            if (toggleSource.gameObject.activeSelf)
            {
                feedbackMesh.materials[feedbackMaterialIndex].color = _originalColor;
            }
            else
            {
                feedbackMesh.materials[feedbackMaterialIndex].color = radioOffColor;
            }
        }
    }
}