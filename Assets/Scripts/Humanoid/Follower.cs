using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;

public class Follower : Humanoid {

		// action
	[SerializeField]
	private float actionRate_Min = 5f;
	[SerializeField]
	private float actionRate_Max = 15f;
	private float currentActionRate = 1f;

	private float action_ChanceJumping = 0.1f;

	private float actionTimer = 0f;

	[SerializeField]
	private float distanceToStop = 1.5f;

	NavMeshAgent navMeshAgent;
	[SerializeField]
	Transform followTarget;

	bool nearTarget = false;

	// Use this for initialization
	public override void Start()
	{
		base.Start ();

		navMeshAgent = GetComponent<NavMeshAgent> ();

	}

	// Update is called once per frame
	public override void Update () {
		base.Update ();
	}

	float deltaSpeed = 0f;
	Vector3 previousPoint = Vector3.zero;

	#region movement
	public override void Moving_Start ()
	{
		base.Moving_Start ();

	}
	public override void Moving_Update()
	{
		base.Moving_Update ();

		if ( FollowTarget != null && navMeshAgent.enabled )
			GoToPoint ( FollowTarget );
		
//		ActionUpdate ();
	}

	public void GoToPoint (Transform point) {

		Vector3 direction = point.position - GetTransform.position;
		direction.y = 0f;

		float distance = Vector3.Distance (point.position, GetTransform.position);

		TargetSpeed = nearTarget ? 0 : MoveSpeed;

		if (nearTarget) {

			if ( distance > distanceToStop + 0.1f) {
				nearTarget = false;
			}

		} else {

			if (distance < distanceToStop) {
				nearTarget = true;

				if ( FollowTarget.GetComponentInChildren<Interactable>() != null ) {
					FollowTarget.GetComponentInChildren<Interactable> ().Interact(this);
				}
			}

		}

		CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, TargetSpeed, Acceleration * Time.deltaTime);

		navMeshAgent.speed = CurrentSpeed;
		navMeshAgent.SetDestination (point.position);

		UpdateMoveAnimation ();

	}

	public Transform FollowTarget {
		get {
			return followTarget;
		}
		set {
			followTarget = value;
		}
	}
	#endregion

	#region stop
	public override void Stop_Start ()
	{
		base.Stop_Start ();

		NavMeshAgent.speed = 0f;
	}
	#endregion

	#region action
	private void ActionUpdate () {

		actionTimer += Time.deltaTime;

		if ( actionTimer > currentActionRate ) {
			
			actionTimer = 0f;

			currentActionRate = Random.Range ( actionRate_Min , actionRate_Max );
//			currentActionRate = 1f;

			StartAction ();
		}

	}
	private void StartAction () {
		ChangeState (States.Jump);
	}
	#endregion

	public override void Stop ()
	{
		base.Stop ();

		navMeshAgent.speed = 0f;

		CurrentSpeed = 0f;
	}

	public UnityEngine.AI.NavMeshAgent NavMeshAgent {
		get {
			return navMeshAgent;
		}
		set {
			navMeshAgent = value;
		}
	}
}
