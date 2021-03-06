using System;
using System.Collections.Generic;
using ODIN_Sample.Scripts.Runtime.Odin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Behaviour reacting to input for switching the current view state, e.g. for switching between First Person und
    /// Third Person View. View States and the Behaviours which should be activated should be set in
    /// <see cref="viewSettingsList"/>.
    /// </summary>
    public class ToggleViews : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Sample Project's current view state scriptable object reference.
        /// </summary>
        [SerializeField] private SampleViewState currentViewState;
        /// <summary>
        /// The button for toggling the view state.
        /// </summary>
        [SerializeField] private InputActionReference toggleViewButton;
        /// <summary>
        /// Settings for the view states between which we'll be switching.
        /// </summary>
        [SerializeField] private List<ViewSettings> viewSettingsList;

        private void Awake()
        {
            Assert.IsNotNull(currentViewState, "Missing view state reference.");
            Assert.IsNotNull(toggleViewButton);
            toggleViewButton.action.Enable();
        }

        private void OnEnable()
        {
            toggleViewButton.action.performed += ActionToggleView;
        }

        private void OnDisable()
        {
            toggleViewButton.action.performed -= ActionToggleView;
        }

        private void ActionToggleView(InputAction.CallbackContext context)
        {
            currentViewState.SwitchToNextState();

            UpdateViewBehaviourStatus();
        }

        private void Start()
        {
            UpdateViewBehaviourStatus();
        }

        private void UpdateViewBehaviourStatus()
        {
            foreach (ViewSettings setting in viewSettingsList)
            {
                bool isViewActive = setting.viewState == currentViewState.State;
                setting.viewBehaviours.SetActive(isViewActive);
            }
        }

        
        /// <summary>
        /// Serializable struct, containing Settings for switching through views.
        /// </summary>
        [Serializable]
        public struct ViewSettings
        {
            /// <summary>
            /// The View State to which we want to switch.
            /// </summary>
            public SampleViewState.ViewState viewState;
            /// <summary>
            /// The Game Object that should be activated, when switching to this <see cref="viewState"/>. The target
            /// object should contain behaviours required for the view state's game logic.
            /// </summary>
            public GameObject viewBehaviours;
        }
    }
}
