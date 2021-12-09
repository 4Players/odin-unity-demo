using System;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    [CreateAssetMenu(fileName = "SampleViewState", menuName = "Odin-Sample/ViewState", order = 0)]
    public class SampleViewState : ScriptableObject
    {
        [SerializeField] private ViewState currentViewState = ViewState.ThirdPerson;

        public Action<ViewState> OnSwitchedToNewState;
        
        public ViewState State => currentViewState;

        public void SwitchToNextState()
        {
            ViewState[] values = (ViewState[]) Enum.GetValues(typeof(ViewState));
            int nextIndex = ((int)currentViewState + 1) % values.Length;
            SetState(values[nextIndex]);
        }

        public void SetState(ViewState newState)
        {
            currentViewState = newState;
            OnSwitchedToNewState?.Invoke(newState);
        }

        public enum ViewState
        {
            FirstPerson,
            ThirdPerson
        }
        
    }
}