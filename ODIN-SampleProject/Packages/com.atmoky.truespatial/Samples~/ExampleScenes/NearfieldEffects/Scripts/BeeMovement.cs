using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeMovement : MonoBehaviour
{

    [Range(0.5f, 10.0f)]
    public float XFrequency = 2.0f;

    [Range(0.5f, 10.0f)]
    public float YFrequency = 2.0f;

    [Range(0.5f, 10.0f)]
    public float ZFrequency = 1.0f;

    [Range(0.1f, 2.0f)]
    public float Radius = 1.0f;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float x = Radius * Mathf.Sin(Time.time * XFrequency);
        float y = Radius * Mathf.Sin(Time.time * YFrequency);
        float z = Radius * Mathf.Sin(Time.time * ZFrequency);


        transform.localPosition = new Vector3(x, y, z);
        // rotate the bee such that it faces the direction it is moving
        //transform.LookAt(transform.position + new Vector3(x, y, z));

        // roate the bee such that its up vector points in the direction it is moving
        //transform.forward = new Vector3(x, y, -z);

        // Rotate towards the movement direction
        Vector3 movementDirection = new Vector3(x, y, z).normalized;
        if (movementDirection == Vector3.zero)
        {
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(-movementDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 1f);
    }
}
