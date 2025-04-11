using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class RoomEnterScript : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioMixer audioMixer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
        if (other.gameObject.tag == "Player")
        {
            audioSource.reverbZoneMix = 1.0f;
            Debug.Log("reverbZoneMix = 1");
        }
    }

    void OnTriggerExit(Collider collision)
    {
        Debug.Log("Exited");
        if (collision.gameObject.tag == "Player")
        {
            audioSource.reverbZoneMix = 0.0f;
            Debug.Log("reverbZoneMix = 0");
        }
    }
}
