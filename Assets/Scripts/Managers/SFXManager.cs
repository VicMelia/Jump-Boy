using UnityEngine;
using System.Collections;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }
    private float _fadeSpeed = 0.05f;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    private IEnumerator FadeVolume(AudioSource source, float start, float end)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * _fadeSpeed;
            source.volume = Mathf.Lerp(start, end, t);
            yield return null;
        }

        source.volume = end;
    }
}