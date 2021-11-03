using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CubeText : MonoBehaviour
{
    void Start()
    {
        gameObject.transform.rotation = Camera.main.transform.rotation;
    }

    void Update()
    {
        //gameObject.transform.rotation = Quaternion.LookRotation(gameObject.transform.position - Camera.main.transform.position);
    }
}
