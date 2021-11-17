using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub_Dive : MonoBehaviour {

	public static Sub_Dive Instance;

	public delegate void OnDiveStepChange (int newDiveStep,int maxDiveStep);
	public OnDiveStepChange onDiveStepChange;

	[SerializeField]
	private Transform targetTransform;

	[Header("dive angle")]
	public int currStep = 5;
	public int maxSteps = 10;
	public float currSpeed = 0f;
	public float targetSpeed = 0f;

	[SerializeField]
	public float acceleration = 5f;

	[SerializeField]
	private float maxSpeed = 20f;

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		SubmarineInput.Instance.getAction += HandleGetAction;
	}
	
	// Update is called once per frame
	void Update () {
		UdpateSpeed ();
	}

	#region dive
	public int DiveStep {
		get {

			return currStep;

		}
		set {

			currStep = Mathf.Clamp (value, 0, maxSteps);

			if (onDiveStepChange != null)
				onDiveStepChange (currStep,maxSteps);

			UdpateSpeed ();
		}
	}
	void UdpateSpeed ()
	{

		float lerp = (float)DiveStep / (maxSteps);

		targetSpeed = Mathf.Lerp ( -maxSpeed , maxSpeed, lerp );

		currSpeed = Mathf.MoveTowards ( currSpeed , targetSpeed , acceleration * Time.deltaTime);

		targetTransform.Translate ( Vector3.up * currSpeed * Time.deltaTime , Space.World);

	}
	#endregion

	#region main get action event 
	void HandleGetAction (Action action)
	{

		switch (action.actionType) {
		case ActionType.Rise:
			DiveStep += action.amount;
			break;
		case ActionType.Dive:
			DiveStep -= action.amount;
			break;
		case ActionType.Stop:
			DiveStep = 5;
			break;
		case ActionType.Stabilize:
			DiveStep = 5;
			break;
		default:
			break;
		}
	}
	#endregion
}
