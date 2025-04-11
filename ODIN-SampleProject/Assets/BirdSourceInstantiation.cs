using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSourceInstantiation : MonoBehaviour
{
    public GameObject birdPrefab;

    [Range(1, 100)]
    public int numberOfBirds = 10;

    List<GameObject> birds = new List<GameObject>();


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
        for (int i = 0; i < numberOfBirds; i++)
        {
            GameObject bird = Instantiate(birdPrefab);

            bird.transform.position = transform.position + new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            birds.Add(bird);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }


}
