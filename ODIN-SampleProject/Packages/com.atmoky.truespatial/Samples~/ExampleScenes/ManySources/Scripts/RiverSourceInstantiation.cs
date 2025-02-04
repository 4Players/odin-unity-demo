using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverSourceInstantiation : MonoBehaviour
{
    public GameObject riverPrefab;

    [Range(1, 100)]
    public int numberOfRiverSounds = 10;


    List<GameObject> riverSounds = new List<GameObject>();


    [Range(-20, 20)]
    public float minX = -10;
    [Range(-20, 20)]
    public float maxX = 10;
    [Range(-20, 20)]
    public float minY = -10;
    [Range(-20, 20)]
    public float maxY = 10;
    [Range(-20, 20)]
    public float minZ = -10;
    [Range(-20, 20)]
    public float maxZ = 10;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfRiverSounds; i++)
        {
            GameObject riverSound = Instantiate(riverPrefab);

            riverSound.transform.position = transform.position + new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            riverSounds.Add(riverSound);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
