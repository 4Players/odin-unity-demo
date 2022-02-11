using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OdinNative.Unity.Samples
{
    /// <summary>
    /// Demo Example where everything in #region Demo does not correlate with Odin
    /// </summary>
    [ExecuteInEditMode]
    public class DemoPeerDisplay : MonoBehaviour
    {
        #region Demo
        public bool TopDown;

        void Start()
        {
            gameObject.transform.rotation = Camera.main.transform.rotation;
        }

        void Update()
        {
            if (TopDown)
                gameObject.transform.rotation = Quaternion.LookRotation(gameObject.transform.position - Camera.main.transform.position);
            else
                gameObject.transform.rotation = Quaternion.Euler(Camera.main.transform.rotation.eulerAngles.x, 0.0f, gameObject.transform.parent.rotation.z * -1.0f);
        }
        #endregion Demo
    }
}