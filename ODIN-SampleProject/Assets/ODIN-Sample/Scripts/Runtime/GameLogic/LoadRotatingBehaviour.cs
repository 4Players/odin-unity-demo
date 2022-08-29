using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.GameLogic
{
    /// <summary>
    /// Simple script for animating a rotating load icon
    /// </summary>
    public class LoadRotatingBehaviour : MonoBehaviour
    {
        // The max speed added on top of the minimum speed
        [SerializeField] private float speed = 0.8f;
        // the sinus wave scaling
        [FormerlySerializedAs("dilation")] [SerializeField] private float waveScale = 1.5f;
        [SerializeField] private float minimumSpeed = 60.0f;

        private void Update()
        {
            float angle = Mathf.Sin(Time.time * waveScale) * speed ;
            angle = Mathf.Abs(angle) + minimumSpeed * Time.deltaTime;
            transform.Rotate(Vector3.forward, angle);
        }
    }
}