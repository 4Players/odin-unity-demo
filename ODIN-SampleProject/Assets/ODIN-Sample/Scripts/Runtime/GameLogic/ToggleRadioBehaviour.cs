using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Behaviour for toggling the in-game radio - will essentially activate or deactivate the radio's playback Audio Source
    /// referenced as <see cref="toggleSource"/>.
    /// </summary>
    public class ToggleRadioBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Name of the button used for toggling the radio.
        /// </summary>
        [SerializeField] private string toggleButton = "Toggle";
        /// <summary>
        /// The radio's audio source.
        /// </summary>
        [SerializeField] private AudioSource toggleSource;
        /// <summary>
        /// The renderer used for feedback on the radio activity status. Will switch the renderers color to <see cref="radioOffColor"/>
        /// if off, or back to the original color if on.
        /// </summary>
        [SerializeField] private Renderer feedbackMesh;
        /// <summary>
        /// The material index to target when setting the <see cref="feedbackMesh"/> material color
        /// </summary>
        [SerializeField] private int feedbackMaterialIndex = 1;
        
        /// <summary>
        /// The color to display on the <see cref="feedbackMesh"/> when the radio is toggled off.
        /// </summary>
        [ColorUsage(true, true)]
        [SerializeField] private Color radioOffColor = Color.red;

        /// <summary>
        /// Called, when the radio active status is toggled. True, if the radio audio is playing, off otherwise.
        /// </summary>
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

        /// <summary>
        /// Returns whether the radio is active = is playing music/audio.
        /// </summary>
        /// <returns>True if playing, false otherwise.</returns>
        public bool IsRadioActive()
        {
            return toggleSource.gameObject.activeSelf;
        }

        /// <summary>
        /// Toggle the current radios active status.
        /// </summary>
        public void ToggleRadio()
        {
            SetRadio(!IsRadioActive());
            onRadioToggled.Invoke(IsRadioActive());
        }

        /// <summary>
        /// Directly set the radios new active status.
        /// </summary>
        /// <param name="newActive">Set to true, if the radio should play audio, false to mute it.</param>
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