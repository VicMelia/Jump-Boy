using UnityEngine;

public class MusicManager : MonoBehaviour
{
    AudioSource _musicSource;
    public static MusicManager Instance;

    private void Awake()
    {
        Instance = this;
        _musicSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMusic()
    {
        _musicSource.Play();
    }
    public void StopMusic()
    {
        _musicSource.Stop();
    }
}
