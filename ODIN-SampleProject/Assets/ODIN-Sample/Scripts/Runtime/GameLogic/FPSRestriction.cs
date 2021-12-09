using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.GameLogic.Test
{
    /// <summary>
    /// Restricts the game fps to the <see cref="targetFPS"/>.
    /// </summary>
    public class FPSRestriction : MonoBehaviour
    {
        [SerializeField] private int targetFPS = 15;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFPS;
        }
    }
}