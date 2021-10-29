using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [RequireComponent(typeof(Collider))]
    public class AudioObstacle : MonoBehaviour
    {
        public AudioObstacleData audioObstacleData;

        private void Awake()
        {
            Assert.IsNotNull(audioObstacleData);
        }
    }
}