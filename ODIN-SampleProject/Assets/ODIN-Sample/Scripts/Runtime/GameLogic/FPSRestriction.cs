using System;
using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Restricts the game fps to the <see cref="targetFPS"/>. For testing purposes only.
    /// </summary>
    public class FPSRestriction : MonoBehaviour
    {
        /// <summary>
        /// The target fps.
        /// </summary>
        [SerializeField] private int targetFPS = 15;

        private void OnEnable()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFPS;
        }
    }
}