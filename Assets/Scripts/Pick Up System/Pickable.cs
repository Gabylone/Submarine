using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Pickable : Interactable {

	[SerializeField]
	private string pickableName = "";

	public enum PickableStates {
		Pickable,
		Unpickable,

		Carried,
		Thrown,
		Dropped
	}
	PickableStates pickableState = PickableStates.Pickable;

	Humanoid carrier;

	[Header("Physics Params")]
	[SerializeField]
	private float force = 500f;
	[SerializeField]
	private float torque = 250f;
	[SerializeField]
	private float angleToStand = 0.8f;
	[SerializeField]
	private RigidbodyConstraints initContraints = RigidbodyConstraints.FreezeAll;

	// Components
	private Rigidbody rigidbody;
	private Transform initParent;
	private Collider collider;
	Transform target;

	[Header("Lerp Params")]
//	[SerializeField]
	private float lerpDuration = 0.5f;

	private bool lerping = false;
	float timer = 0f;
	Vector3 lerp_InitPos = Vector3.zero;
	Vector3 lerp_InitRot = Vector3.zero;

	[Header("Animation Params")]
	[SerializeField]
	private float weightSpeed_Throwing = 1f;
	[SerializeField]
	private float weightSpeed_Carrying = 1f;
	[SerializeField]
	private CarryTypes carryType = CarryTypes.OneHanded;


	public enum CarryTypes {
		OneHanded,
		TwoHanded
	}
	private float ikTimer = 0f;

	public virtual void Start () {

		base.Start ();

		rigidbody = GetComponent<Rigidbody> ();
		collider = GetComponentInChildren<Collider> ();
		initParent = transform.parent;

		pickableState = PickableStates.Pickable;


		Constrained = true;
	}

	public virtual void Update () {

		if (lerping)
			LerpToHand ();

		SetWeightTimer ();

	}

	IKControl ikControl;

	#region pick up
	public override void Interact (Humanoid humanoid)
	{
		base.Interact (humanoid);

		PickUp (humanoid);
	}
	public virtual void PickUp (Humanoid _hum) {

		if (PickableState == PickableStates.Carried)
			return;

		// set humanoid
		carrier = _hum;
		target = _hum.GetTransform;

			// set state
		pickableState = PickableStates.Carried;
		Available = false;

			// choose hand
		if ( Carrier.PickableManager.RightHandPickable != null ) {
			ikControl = Carrier.IkManager.LeftArmIK;
		} else {
			ikControl = Carrier.IkManager.RightArmIK;
		}

			// set pickable

		carrier.GetComponent<PickableController> ().AddPickable(this);

			// set physics
		Constrained = true;
		Collider.enabled = false;

			// set lerp to parent
		lerping = true;
		timer = 0f;

		lerp_InitRot = transform.up;
		lerp_InitPos = transform.position;
	}

	public void Reset () {

		transform.rotation = Quaternion.LookRotation ( Vector3.forward , Vector3.up );

		Vector3 p = transform.position;
		p.y = 0f;
		transform.position = p;

		Constrained = true;
	}
	#endregion

	#region throw & drop
	public virtual void Throw ( Vector3 direction ) {

			// state
		pickableState = PickableStates.Thrown;

			// set anims
		if ( Carrier.PickableManager.LeftHandPickable != null)
			Carrier.GetAnimator.SetLayerWeight (1, 1);
		if ( Carrier.PickableManager.RightHandPickable != null)
			Carrier.GetAnimator.SetLayerWeight (2, 1);

		Carrier.GetAnimator.SetTrigger ("throw");

		rigidbody.AddTorque ( direction * torque );
		rigidbody.AddForce ( direction * force);

		Exit ();

	}

	public virtual void Drop () {

		pickableState = PickableStates.Dropped;
		Exit ();
	}

	public virtual void Exit () {

		if ( Carrier != null )
			Carrier.PickableManager.RemovePickable (this);

		Constrained = false;
		Collider.enabled = true;

		transform.parent = initParent;

	}
	#endregion

	#region animation
	void SetWeightTimer () {

		if (PickableState == PickableStates.Thrown || PickableState == PickableStates.Dropped) { 
			if (ikTimer > 0) {
				ikTimer -= weightSpeed_Throwing * Time.deltaTime;

				ApplyWeight ();
			}
		}

		if (PickableState == PickableStates.Carried && lerping == false) { 
			if (ikTimer < 1) {
				ikTimer += weightSpeed_Carrying * Time.deltaTime;

				ApplyWeight ();
			}
		}
	}

	void ApplyWeight () {
		if (carryType == CarryTypes.TwoHanded) {
			Carrier.GetAnimator.SetLayerWeight (1, ikTimer);
			Carrier.GetAnimator.SetLayerWeight (2, ikTimer);
		} else {
			
			if ( Carrier.PickableManager.LeftHandPickable != null) {
				Carrier.GetAnimator.SetLayerWeight (1, ikTimer);
			}

			if ( Carrier.PickableManager.RightHandPickable != null) {
				Carrier.GetAnimator.SetLayerWeight (2, ikTimer);
			}

		}
	}

	void LerpToHand () {
		transform.position = Vector3.Lerp (lerp_InitPos, ikControl.Hand.position, timer/lerpDuration);
		transform.up = Vector3.Lerp (lerp_InitRot, Vector3.up, timer/lerpDuration);

		timer += Time.deltaTime;

		if (timer >= lerpDuration) { 
			transform.SetParent (ikControl.Hand);
			carrier.IkManager.RemoveIK ();
			lerping = false;
		}
	}
	#endregion

	#region invert kinematics

	#endregion
	public Rigidbody Rigidbody {
		get {
			return rigidbody;
		}
	}
	public bool Constrained {
		get {
			if ( Rigidbody.constraints == initContraints )
				return true;
			else
				return false;
		}
		set {
			if (value == true) {
				if (Rigidbody == null)
					Debug.Log ("game" + gameObject.name);
				Rigidbody.constraints = initContraints;
			} else {
				Rigidbody.constraints = RigidbodyConstraints.None;
			}
		}
	}

	public PickableStates PickableState {
		get {
			return pickableState;
		}
		set {
			pickableState = value;
			Available = value != PickableStates.Carried;
		}
	}

	public Collider Collider{
		get {
			return collider;
		}
	}

	public bool Straight {
		get {
			return Vector3.Dot (transform.up, Vector3.up) > angleToStand;
		}
	}

	public Transform Target {
		get {
			return target;
		}
		set {
			target = value;
		}
	}

	public Humanoid Carrier {
		get {
			return carrier;
		}
		set {
			carrier = value;
		}
	}

	public Transform InitParent {
		get {
			return initParent;
		}
		set {
			initParent = null;
		}
	}

	public CarryTypes CarryType {
		get {
			return carryType;
		}
		set {
			carryType = value;
		}
	}

	public string PickableName {
		get {
			return pickableName;
		}
	}
}