using UnityEngine;

namespace ODIN_Sample.Scripts.Runtime.FirstPerson
{
    public class MouseRotate : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            float yaw = Input.GetAxis("Mouse X");
            float pitch = Input.GetAxis("Mouse Y");
        
        
        }
    }
}
