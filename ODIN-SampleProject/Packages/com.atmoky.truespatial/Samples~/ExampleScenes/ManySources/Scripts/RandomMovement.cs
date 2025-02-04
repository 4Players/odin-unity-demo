using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
    private Vector3 direction = new(0, 0, 0);

    // Update is called once per frame
    void Update()
    {
        float strength = 0.01f;
        float range = 10.0f;

        float x = Random.Range(-range, range);
        float y = Random.Range(-range, range);
        float z = Random.Range(-range, range);

        var target = new Vector3(x, y, z);

        // check if we are too far away from home
        if (transform.localPosition.magnitude > 2.0f)
        {
            target = -transform.localPosition;
        }

        direction = (1.0f - strength) * direction + strength * target;
        transform.localPosition += Time.deltaTime * direction;
    }
}
