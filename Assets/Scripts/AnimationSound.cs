using UnityEngine;

public class AnimationSound : MonoBehaviour {
    public AudioSource footStepSource;
    public AudioClip[] footStepClips;
    public void PlayFootStep() {
        footStepSource.clip = footStepClips[Random.Range(0, footStepClips.Length)];
        footStepSource.Play();
    }
}
