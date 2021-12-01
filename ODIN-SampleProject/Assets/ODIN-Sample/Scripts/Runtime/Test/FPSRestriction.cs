using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.Test
{
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