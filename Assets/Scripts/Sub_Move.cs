using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub_Move : MonoBehaviour {

	public static Sub_Move Instance;

	[Header("Components")]
	[SerializeField]
	public Transform targetTransform;

	// MOVE
	[Header("move")]
	public float maxMoveSpeed = 10f;

	private int currentSpeedStep = 0;
	public int maxSpeedStep = 10;

	private float targetMoveSpeed = 0f;
	public float currentMoveSpeed = 0f;
	[SerializeField]
	private float moveAcceleration = 5f;

	void Start () {
		SubmarineInput.Instance.getAction += HandleGetAction;
	}

	// Update is called once per frame
	void Update () {

		UpdateMovement ();

	}

	#region movement
	public int CurrentSpeedStep {
		get {
			return currentSpeedStep;
		}
		set {

			currentSpeedStep = Mathf.Clamp(value , 0, maxSpeedStep);

			targetMoveSpeed = maxMoveSpeed * currentSpeedStep / maxSpeedStep;

		}
	}
	void UpdateMovement ()
	{
		currentMoveSpeed = Mathf.MoveTowards ( currentMoveSpeed , targetMoveSpeed , moveAcceleration * Time.deltaTime );

		transform.Translate ( targetTransform.forward * currentMoveSpeed * Time.deltaTime , Space.World);
	}
	#endregion

	#region main get action event 
	void HandleGetAction (Action action)
	{

		switch (action.actionType) {
		case ActionType.Accelerate:
			CurrentSpeedStep += action.amount;
			break;
		case ActionType.Deccelerate:
			CurrentSpeedStep -= action.amount;
			break;
		case ActionType.Stop:
			CurrentSpeedStep = 0;
			break;
		case ActionType.StopMoving:
			CurrentSpeedStep = 0;
			break;
		default:
			break;
		}
	}
	#endregion
}
