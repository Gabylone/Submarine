using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Type : MonoBehaviour {

	public static bool typing = false;
	static int charIndex = 1;

	static float timer = 0f;

	[SerializeField]
	AudioClip[] _typeAudioClips;
	static AudioClip[] typeAudioClips;
	static AudioSource audioSource;

	static List<TextToDisplay> textsToDisplay = new List<TextToDisplay>();

	TextToDisplay currTextToDisplay {
		get {
			return textsToDisplay [0];
//			return textsToDisplay [textsToDisplay.Count-1];
		}
	}

	// Use this for initialization
	void Start () {

		typeAudioClips = _typeAudioClips;
		audioSource = GetComponent<AudioSource> ();
	}

	public static void DisplayText (TextToDisplay textToDisplay)
	{
		textsToDisplay.Add (textToDisplay);

		if ( !typing )
		DisplayCurrentText ();
	}

	static void DisplayCurrentText () {
		typing = true;

		timer = 0f;

		charIndex = 1;
	}

	void CheckForRichText ()
	{




	}

	void DisplayChar ()
	{
		string str = currTextToDisplay.phrase.Remove ( charIndex );

		if ( str.EndsWith("<") ) {

			string targetString = "</color>";

			int targetIndex = currTextToDisplay.phrase.IndexOf (targetString, charIndex);

			if ( targetIndex >= currTextToDisplay.phrase.Length ) {
				print ("cret : " + targetIndex + " / " + currTextToDisplay.phrase.Length);
			}

//			charIndex = targetIndex + targetString.Length;
			charIndex = targetIndex + targetString.Length-1;

			currTextToDisplay.targetText.text = currTextToDisplay.phrase.Remove ( charIndex );

		}

		currTextToDisplay.targetText.text = str;

		timer = 0f;

		charIndex++;

		if ( charIndex == currTextToDisplay.phrase.Length) {
			EndDisplay ();
		}

			// PLAY SOUND
		audioSource.clip = typeAudioClips [Random.Range (0, typeAudioClips.Length)];
		audioSource.Play ();
	}

	void EndDisplay ()
	{
		typing = false;
		currTextToDisplay.targetText.text = currTextToDisplay.phrase;

		textsToDisplay.Remove (currTextToDisplay);

		if ( textsToDisplay.Count > 0 ) {
			DisplayCurrentText ();
		}
	}

	// Update is called once per frame
	void Update () {

		if ( typing ) {

			if ( timer >= currTextToDisplay.timeBtwLetters ) {

				DisplayChar ();
			}

			timer += Time.deltaTime;

		}
	}


}

public struct TextToDisplay {

	public string phrase;

	public Text targetText;

	public float timeBtwLetters;

		public TextToDisplay (
		string str,
		Text targetText,
		float timeBtwLetters
	)
	{
		this.phrase = str;
		this.targetText = targetText;
		this.timeBtwLetters = timeBtwLetters;
	}


}
