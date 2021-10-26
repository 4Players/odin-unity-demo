using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RotateToVelocity : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float rotationSpeed = 5.0f;
    
    private void Awake()
    {
        Assert.IsNotNull(characterController);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movementDirection = characterController.velocity;
        movementDirection.y = 0.0f;
        if (movementDirection.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            float lerpValue = Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, lerpValue);
        }
    }
}
