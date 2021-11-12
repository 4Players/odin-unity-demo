using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    [RequireComponent(typeof(Collider))]
    public class AudioObstacle : MonoBehaviour
    {
        [FormerlySerializedAs("data")] [FormerlySerializedAs("audioObstacleData")] public AudioObstacleSettings settings;

        private void Awake()
        {
            Assert.IsNotNull(settings);
        }
    }
}