using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    void Awake()
    {
        // Check if an instance already exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
        }
    }

    public void PlayMusic()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (!audio.isPlaying)
        {
            audio.Play();
        }
    }

    public void StopMusic()
    {
        GetComponent<AudioSource>().Stop();
    }
}