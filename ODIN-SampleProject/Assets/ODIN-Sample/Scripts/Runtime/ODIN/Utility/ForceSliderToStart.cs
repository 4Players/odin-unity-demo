using UnityEngine;
using UnityEngine.UI;

namespace ODIN_Sample.Scripts.Runtime.ODIN.Utility
{
    /// <summary>
    /// Utility script for resetting the slider position to the start.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ForceSliderToStart : MonoBehaviour
    {
        // Start is called before the first frame update
        void OnEnable()
        {
            ScrollRect scrollRect = GetComponent<ScrollRect>();
            if (scrollRect)
            {
                scrollRect.verticalNormalizedPosition = 1.0f;
            }
        }

    
    }
}
