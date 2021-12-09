using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class ToggleRadioBehaviour : MonoBehaviour
    {
        [SerializeField] private string toggleButton = "Toggle";

        [SerializeField] private AudioSource toggleSource;
        [SerializeField] private Renderer feedbackMesh;
        [SerializeField] private int feedbackMaterialIndex = 1;
        
        [ColorUsage(true, true)]
        [SerializeField] private Color radioOffColor = Color.red;

        public UnityEvent<bool> onRadioToggled;

        private Color _originalColor;
        
        private static readonly int EmissionColorPropertyId = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            Assert.IsNotNull(toggleSource, "Missing reference to toggleSource.");
            Assert.IsNotNull(feedbackMesh, "Missing reference to feedback mesh.");
            Assert.IsTrue(feedbackMesh.materials.Length > feedbackMaterialIndex,
                "Invalid feedbackMaterialIndex: feedbackMesh does not have that many material slots.");

            _originalColor = feedbackMesh.materials[feedbackMaterialIndex].GetColor(EmissionColorPropertyId);
            UpdateFeedbackColor();

        }

        private void Update()
        {
            if (Input.GetButtonDown(toggleButton))
            {
                ToggleRadio();
            }
        }

        public bool IsRadioActive()
        {
            return toggleSource.gameObject.activeSelf;
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
                feedbackMesh.materials[feedbackMaterialIndex].SetColor(EmissionColorPropertyId, _originalColor);
            }
            else
            {
                feedbackMesh.materials[feedbackMaterialIndex].SetColor(EmissionColorPropertyId, radioOffColor);
            }
        }
    }
}