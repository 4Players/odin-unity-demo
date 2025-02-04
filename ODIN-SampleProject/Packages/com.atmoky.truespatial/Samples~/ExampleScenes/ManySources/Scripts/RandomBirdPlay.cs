
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class RandomBirdPlay : MonoBehaviour
{

    AudioSource audioSource;

    [Range(0.0f, 1.0f)]
    public float PlayProbability = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.value < PlayProbability * Time.deltaTime && !audioSource.isPlaying)
        {
            int randomClip = Random.Range(1, 50);
            audioSource.clip = Resources.Load<AudioClip>("BirdSounds/bird_" + randomClip);
            if (audioSource.clip == null)
            {
                Debug.LogError("Could not load sound: " + "BirdSounds/bird_" + randomClip);
                return;
            }
            audioSource.Play();
        }
    }
}
