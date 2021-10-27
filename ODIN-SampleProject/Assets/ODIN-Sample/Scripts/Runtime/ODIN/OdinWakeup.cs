using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.ODIN
{
    public class OdinWakeup : MonoBehaviour
    {
        private void Awake()
        {
            var odinHandler = OdinHandler.Instance;
        }
    }
}
