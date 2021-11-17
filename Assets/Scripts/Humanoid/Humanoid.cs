using UnityEngine;
using System.Collections;

public class Humanoid : MonoBehaviour {

	PickableController pickableController;

	[Header("Invert Kinematics")]
	[SerializeField]
	private IKManager ikManager;

	// properties
	[Header("Components")]
	[SerializeField]
	private Transform _bodyTransform;
	private Transform _transform;
	[SerializeField]
	private Animator animator;
	private Collider _collider;
	private Dialogue _dialogue;

	[Header("State Machine")]
	[SerializeField]
	private States startState = States.Moving;
	private States previousState;
	private States currentState;

	public delegate void UpdateState();
	UpdateState updateState;

	public enum States
	{
		None,
		Moving,
		Sleeping,
		Jump,
		Stop
	}

	private float timeInState = 0f;

	// lerps
	Vector3 lerpInitialPos;
	Vector3 lerpInitalRot;

	[Header("Movement")]
	// movement
	[SerializeField]
	private float rotationSpeed = 50f;
	[SerializeField]
	private float moveSpeed = 2f;
	[SerializeField]
	private float runSpeed = 4f;
	private float targetSpeed = 0f;

	float currentSpeed = 0f;
	[SerializeField]
	private float acceleration = 1f;
	[SerializeField]
	private float decceleration = 1f;

	// Use this for initialization
	public virtual void Start()
	{
		_transform = this.transform;
		ikManager = GetComponentInChildren<IKManager> ();
		pickableController = GetComponent<PickableController> ();
		Dialogue = GetComponent<Dialogue> ();
		Collider = GetComponent<Collider> ();

		ChangeState (startState);

	}

	// Update is called once per frame
	public virtual void Update ()
	{
		if (updateState != null)
		{
			updateState();
			timeInState += Time.deltaTime;
		}


	}

	#region movement
	public virtual void Moving_Start()
	{
		//
	}
	public virtual void Moving_Update()
	{


	}
	public virtual void Moving_Exit()
	{
		//
	}
	public void UpdateMoveAnimation () {
		GetAnimator.SetFloat ("move", CurrentSpeed / RunSpeed);
	}
	#endregion

	#region movement
	private float stop_Duration = 1f;

	public virtual void Stop () {
		Stop (0);
	}

	public virtual void Stop (float dur) {

		if (dur == 0)
			timeInState = -1f;
		else
			timeInState = 0f;

		stop_Duration = dur;
		ChangeState (States.Stop);

		GetAnimator.SetFloat ("move", 0f);
	}

	public virtual void Stop_Start()
	{
		
	}
	public virtual void Stop_Update()
	{
		if (TimeInState >= stop_Duration )
			ChangeState (States.Moving);
	}
	public virtual void Stop_Exit()
	{
		//
	}

	#endregion

	#region state machine
	public void ChangeState(States newState)
	{
		previousState = currentState;
		currentState = newState;

		lerpInitalRot = BodyTransform.forward;
		lerpInitialPos = GetTransform.position;

		timeInState = 0f;

		ExitState();
		EnterState();
	}
	private void EnterState()
	{
		switch (currentState)
		{
		case States.None:
			updateState = null;
			break;
		case States.Moving:
			updateState = Moving_Update;
			Moving_Start();
			break;
		case States.Stop:
			updateState = Stop_Update;
			Stop_Start();
			break;

		}
	}
	private void ExitState()
	{
		switch (previousState)
		{
		case States.None:

			break;
		case States.Moving:
			Moving_Exit();
			break;
		case States.Stop:
			Stop_Exit ();
			break;
		}
	}

	public void Deactivate () {
		updateState = null;
	}
	#endregion

	#region properties
	public Transform GetTransform
	{
		get
		{
			return _transform;
		}
	}
	public Animator GetAnimator
	{
		get
		{
			return animator;
		}
	}
	public Transform BodyTransform
	{
		get
		{
			return _bodyTransform;
		}
	}
	public States CurrentState
	{
		get
		{
			return currentState;
		}
		set
		{
			currentState = value;
		}
	}

	public float TimeInState {
		get {
			return timeInState;
		}
		set {
			timeInState = value;
		}
	}

	public float MoveSpeed {
		get {
			return moveSpeed;
		}
	}

	public float RotationSpeed {
		get {
			return rotationSpeed;
		}
	}

	public float Acceleration {
		get {
			return acceleration;
		}
	}

	public float Decceleration {
		get {
			return decceleration;
		}
	}
	#endregion

	public IKManager IkManager {
		get {
			return ikManager;
		}
	}

	public float RunSpeed {
		get {
			return runSpeed;
		}
	}

	public Collider Collider {
		get {
			return _collider;
		}
		set {
			_collider = value;
		}
	}

	public PickableController PickableManager {
		get {
			return pickableController;
		}
		set {
			pickableController = value;
		}
	}

	#region dialogue
	public Dialogue Dialogue {
		get {
			return _dialogue;
		}
		set {
			_dialogue = value;
		}
	}
	#endregion

	public States StartState {
		get {
			return startState;
		}
		set {
			startState = value;
		}
	}

	public float CurrentSpeed {
		get {
			return currentSpeed;
		}
		set {
			currentSpeed = value;
		}
	}

	public float TargetSpeed {
		get {
			return targetSpeed;
		}
		set {
			targetSpeed = value;
		}
	}

}
