using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Will activate the targets, if the viewstate defined in <see cref="stateToActivateOn"/> is currently active.
    /// </summary>
    public class ActivateOnViewState : MonoBehaviour
    {
        /// <summary>
        /// The targets that will be actived, if <see cref="stateToActivateOn"/> is the current <see cref="viewState"/>
        /// </summary>
        [SerializeField] private GameObject[] targetsToActivate;

        /// <summary>
        /// State to activate on.
        /// </summary>
        [SerializeField] private SampleViewState.ViewState stateToActivateOn;

        /// <summary>
        /// Reference to Scriptable Object, that contains the current view state value.
        /// </summary>
        [SerializeField] private SampleViewState viewState;

        private void Awake()
        {
            Assert.IsNotNull(viewState);
        }

        private void Start()
        {
            UpdateOnViewStateSet(viewState.State);
        }

        private void OnEnable()
        {
            viewState.OnSwitchedToNewState += UpdateOnViewStateSet;
        }

        private void OnDisable()
        {
            viewState.OnSwitchedToNewState -= UpdateOnViewStateSet;
        }

        private void UpdateOnViewStateSet(SampleViewState.ViewState newState)
        {
            foreach (GameObject target in targetsToActivate) target.SetActive(newState == stateToActivateOn);
        }
    }
}