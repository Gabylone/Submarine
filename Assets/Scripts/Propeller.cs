using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : MonoBehaviour {

	[SerializeField]
	private Sub_Move subMove;

	public float rotSpeed = 10f;

	public float minSpeed = 3f;

	public float minVolume = 0.3f;
	public float maxVolume = 0.45f;

	public float minPitch = 0.3f;
	public float maxPitch = 0.6f;

	private AudioSource audioSource;


	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {

		transform.Rotate ( Vector3.forward * (minSpeed + (rotSpeed * subMove.CurrentSpeedStep * Time.deltaTime) ) );

		float f = subMove.currentMoveSpeed / subMove.maxMoveSpeed;

		audioSource.volume = Mathf.Lerp ( minVolume , maxVolume , f );
		audioSource.pitch = Mathf.Lerp ( minPitch , maxPitch , f );
	}
}
