using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubmarineInput : MonoBehaviour {

	public static SubmarineInput Instance;

	public delegate void GetAction ( Action action );
	public GetAction getAction;

	[SerializeField]
	private Action[] actions;

	private InputField inputField;

	public AudioClip rightAudioClip;
	public AudioClip wrongAudioClip;
	private AudioSource audioSource;

	public AudioClip[] typeAudioClips;


	private int[] numbers = new int[9] {1,2,3,4,5,6,7,8,9};

		// feedback
	bool feedbackActive = false;
	public float feedbackDuration = 1f;
	float timer = 0f;

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		inputField = GetComponent<InputField> ();

		inputField.Select();
		inputField.ActivateInputField();

		audioSource = GetComponent<AudioSource> ();
	}

	public void OnValueEdit () {
		audioSource.clip = typeAudioClips [Random.Range (0, typeAudioClips.Length)];
		audioSource.Play ();
	}

	public void OnEndEdit () {

		string inputText = inputField.text.ToLower ();

		inputField.text = "";
		inputField.Select();
		inputField.ActivateInputField();


		foreach ( Action action in actions ) {

			string phrase = System.Array.Find (action.associatedPhrases, x => inputText.Contains (x.ToLower()));

			if ( phrase != null ) {
				
				inputText = inputText.Replace ( phrase , "" );

				int tmpAmount = 0;
				System.Array.Find (numbers, x => int.TryParse (inputText, out tmpAmount));

				if ( tmpAmount > 0 ) {
					action.amount = tmpAmount;
				}

				if ( getAction != null )
					getAction (action);

				Feedback_RightAnswer ();

				return;
			}

		}

		Feedback_WrongAnswer ();

	}

	#region feedback
	void Update () {
		if ( feedbackActive ) {

			timer += Time.deltaTime;

			if ( timer >= feedbackDuration ) {
				feedbackActive = false;
				inputField.placeholder.color = Color.green;
			}

		}
	}
	void Feedback_RightAnswer () {

		inputField.placeholder.color = Color.blue;

		audioSource.clip = rightAudioClip;
		audioSource.Play ();

		feedbackActive = true;

		timer = 0f;

	}

	void Feedback_WrongAnswer() {

		inputField.placeholder.color = Color.red;

		audioSource.clip = wrongAudioClip;
		audioSource.Play ();

		feedbackActive = true;

		timer = 0f;

	}
	#endregion

	public Action[] Actions {
		get {
			return actions;
		}
	}
}



[System.Serializable]
public class Action {

	public string[] associatedPhrases;

	public ActionType actionType;

	public int amount = 0;

}

public enum ActionType {
	
	Rise,
	Dive,

	Accelerate,
	Deccelerate,

	Stop,

	TurnRight,
	TurnLeft,

	TurnDown,
	TurnUp,

	StopTurn,
	StopMoving,
	Stabilize,

	DisplayHelp,
	HideHelp,

}