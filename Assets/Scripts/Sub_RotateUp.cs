using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub_RotateUp : MonoBehaviour {

	public static Sub_RotateUp Instance;

	[SerializeField]
	private Transform targetTransform;

	[Header("dive angle")]
	int angleStep = 5;
	int maxAngleStep = 10;
	public float currAngle = 0f;
	public float targetAngle = 0f;

	[SerializeField]
	public float speedToTargetAngle = 5f;

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		SubmarineInput.Instance.getAction += HandleGetAction;
	}

	// Update is called once per frame
	void Update () {
		UpdateDiveAngle ();
	}

	#region dive
	public int AngleStep {
		get {

			return angleStep;

		}
		set {

			angleStep = Mathf.Clamp (value, -89, 89);

			UpdateDiveAngle ();
		}
	}
	void UpdateDiveAngle ()
	{

		float lerp = (float)AngleStep / (maxAngleStep);

		targetAngle = Mathf.Lerp ( -90f , 90f , lerp );

		currAngle = Mathf.MoveTowards ( currAngle , targetAngle , speedToTargetAngle * Time.deltaTime);

		targetTransform.localEulerAngles = new Vector3 (currAngle, 0f , 0f);

	}
	#endregion

	#region main get action event 
	void HandleGetAction (Action action)
	{
		switch (action.actionType) {
		case ActionType.TurnUp:
			AngleStep -= action.amount;
			break;
		case ActionType.TurnDown:
			AngleStep += action.amount;
			break;
		case ActionType.Stop:
			AngleStep = 5;
			break;
		default:
			break;
		}
	}
	#endregion
}
