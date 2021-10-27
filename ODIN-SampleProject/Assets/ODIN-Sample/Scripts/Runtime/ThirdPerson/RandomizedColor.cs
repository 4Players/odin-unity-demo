using System;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace ODIN_Sample.Scripts.Runtime.ThirdPerson
{
    [RequireComponent(typeof(Renderer))]
    public class RandomizedColor : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            Assert.IsNotNull(_renderer);

            if (null != _renderer)
            {
                _renderer.material.color = Random.ColorHSV();
            }
        }
        
        
    }
}
