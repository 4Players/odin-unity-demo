using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class ToggleViews : MonoBehaviour
    {
        [SerializeField] private SampleViewState currentViewState;
        [SerializeField] private string toggleViewButtonName;

        [SerializeField] private List<ViewSettings> viewSettingsList;

        private void Awake()
        {
            Assert.IsNotNull(currentViewState, "Missing view state reference.");
        }

        private void Start()
        {
            UpdateViewBehaviourStatus();
        }

        private void Update()
        {
            if (Input.GetButtonDown(toggleViewButtonName))
            {
                currentViewState.SwitchToNextState();

                UpdateViewBehaviourStatus();
            }
        }

        private void UpdateViewBehaviourStatus()
        {
            foreach (ViewSettings setting in viewSettingsList)
            {
                bool isViewActive = setting.viewState == currentViewState.State;
                setting.viewBehaviours.SetActive(isViewActive);
            }
        }

        [Serializable]
        public struct ViewSettings
        {
            public SampleViewState.ViewState viewState;
            public GameObject viewBehaviours;
        }
    }
}
