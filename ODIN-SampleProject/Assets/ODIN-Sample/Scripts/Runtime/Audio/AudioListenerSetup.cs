using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ODIN_Sample.Scripts.Runtime.Audio
{
    /// <summary>
    /// Used by the ODIN sample audio system to setup the 
    /// </summary>
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class AudioListenerSetup : MonoBehaviour
    {
        [SerializeField] private float detectionRange = 100.0f;
        
        private void Awake()
        {
            SphereCollider audioSourceDetector = GetComponent<SphereCollider>();
            Assert.IsNotNull(audioSourceDetector);
            audioSourceDetector.isTrigger = true;
            audioSourceDetector.radius = detectionRange;

            Rigidbody detectorRigidBody = GetComponent<Rigidbody>();
            Assert.IsNotNull(detectorRigidBody);
            detectorRigidBody.isKinematic = true;
            detectorRigidBody.useGravity = false;
        }
    }
}