using System;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Scriptable Object, used as a reference to the sample project's current view state.  A view state is defined as
    /// the view (camera) position of a player, e.g. First Person or Third Person.
    /// </summary>
    [CreateAssetMenu(fileName = "SampleViewState", menuName = "Odin-Sample/ViewState", order = 0)]
    public class SampleViewState : ScriptableObject
    {
        /// <summary>
        /// The current View State.
        /// </summary>
        [SerializeField] private ViewState currentViewState = ViewState.ThirdPerson;

        /// <summary>
        /// Called when the view state is changed.
        /// </summary>
        public Action<ViewState> OnSwitchedToNewState;
        
        /// <summary>
        /// Returns the current View State.
        /// </summary>
        public ViewState State => currentViewState;

        /// <summary>
        /// Switch to the next <see cref="ViewState"/>. Will iterate through states based on the order of entries in
        /// the <see cref="ViewState"/> <c>enum</c> definition.
        /// </summary>
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

        /// <summary>
        /// Enum containing all possible view states. A view state is defined as the view position of a player, e.g. First
        /// Person or Third Person.
        /// </summary>
        public enum ViewState
        {
            FirstPerson,
            ThirdPerson
        }
        
    }
}