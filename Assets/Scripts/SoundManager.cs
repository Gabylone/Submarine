using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    public AudioSource audioSource;
    public AudioSource audioSource_Loop;

    private void Awake() {
        Instance = this;
    }

    public void Play(AudioClip audioClip, float pitch) {
        audioSource.clip = audioClip;
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    public void Play(AudioClip audioClip) {
        Play(audioClip, 1f);
    }

    public void Play(AudioClip[] audioClips) {
        Play(audioClips[Random.Range(0, audioClips.Length)]);
    }

    public void Play(AudioClip[] audioClips, float pitch) {
        Play(audioClips[Random.Range(0, audioClips.Length)], pitch);
    }


}
