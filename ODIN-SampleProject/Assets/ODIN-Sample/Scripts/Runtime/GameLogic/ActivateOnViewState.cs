using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    public class ActivateOnViewState : MonoBehaviour
    {
        [SerializeField] private GameObject[] targetsToActivate;
        
        [SerializeField] private SampleViewState.ViewState stateToActivateOn;

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
            viewState.OnSwitchedToNewState += (UpdateOnViewStateSet);
        }
        
        private void OnDisable()
        {
            viewState.OnSwitchedToNewState -= (UpdateOnViewStateSet);
        }

        private void UpdateOnViewStateSet(SampleViewState.ViewState newState)
        {
            foreach (GameObject target in targetsToActivate)
            {
                target.SetActive(newState == stateToActivateOn);

            }

        }
    }
}