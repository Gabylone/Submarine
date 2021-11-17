using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiveSound : MonoBehaviour {

	[SerializeField]
	private AudioClip[] crankClips;

	private AudioSource mainSource;

	public float minPitch = 0.7f;
	public float maxPitch = 1.5f;

	public float minVol = 0.4f;
	public float maxVol = 0.7f;

	// Use this for initialization
	void Start () {
		mainSource = GetComponent<AudioSource> ();

		Sub_Dive.Instance.onDiveStepChange += HandleOnDiveStepChange;
	}

	void HandleOnDiveStepChange (int newDiveStep,int maxDiveSteps )
	{

		float lerp = newDiveStep / maxDiveSteps;

		float pitch = Mathf.Lerp ( minPitch , maxPitch , lerp);
		float vol = Mathf.Lerp ( maxVol , minVol , lerp);

		mainSource.pitch = pitch;
		mainSource.volume = vol;

		mainSource.clip = crankClips [Random.Range (0, crankClips.Length)];
		mainSource.Play ();

	}

}
