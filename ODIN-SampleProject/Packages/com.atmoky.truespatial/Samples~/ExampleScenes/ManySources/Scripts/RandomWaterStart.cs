using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomWaterStart : MonoBehaviour
{
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        int randomClip = Random.Range(1, 4);
        audioSource.clip = Resources.Load<AudioClip>("RiverSounds/river" + randomClip);
        audioSource.time = Random.Range(0, audioSource.clip.length);
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
