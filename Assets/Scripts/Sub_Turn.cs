using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub_Turn : MonoBehaviour {

	public static Sub_Turn Instance;

	public delegate void OnTurnStepChange (int newStep);
	OnTurnStepChange onTurnStepChange;

	[SerializeField]
	Transform targetTransform;

	AudioSource audioSource;

	// TURN
	[Header("turn angle")]
	private int currentTurnStep = 5;
	public int maxTurnSteps = 10;
	public float currentTurnSpeed = 0f;
	public float targetTurnSpeed = 0f;

	[SerializeField]
	public float turnAcceleration = 5f;

	[SerializeField]
	private float maxTurnSpeed = 20f;

	void Start () {
		SubmarineInput.Instance.getAction += HandleGetAction;

		audioSource = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateTurn ();
	}

	#region turn
	public int CurrentTurnStep {
		get {
			return currentTurnStep;
		}
		set {
			currentTurnStep = Mathf.Clamp (value, 0, 10);

			if ( onTurnStepChange != null ) {
				onTurnStepChange (currentTurnStep);
			}
		}
	}
	void UpdateTurn () {

		float lerp = (float)CurrentTurnStep / 10f;

		targetTurnSpeed = Mathf.Lerp ( -maxTurnSpeed , maxTurnSpeed , lerp );

		currentTurnSpeed = Mathf.MoveTowards ( currentTurnSpeed , targetTurnSpeed , turnAcceleration * Time.deltaTime);

		targetTransform.Rotate ( Vector3.up * currentTurnSpeed * Time.deltaTime , Space.World);
	}
	#endregion

	#region main get action event 
	void HandleGetAction (Action action)
	{

		switch (action.actionType) {
		case ActionType.Stop:
			CurrentTurnStep = 5;
			break;
		case ActionType.TurnRight:
			CurrentTurnStep += action.amount;
			break;
		case ActionType.TurnLeft:
			CurrentTurnStep -= action.amount;
			break;
		case ActionType.StopTurn:
			CurrentTurnStep = 5;
			break;
		default:
			break;
		}
	}
	#endregion
}
