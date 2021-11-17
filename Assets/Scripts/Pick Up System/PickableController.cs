using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PickableController : MonoBehaviour {

	int pickableCount = 0;
	Pickable leftHandPickable;
	Pickable rightHandPickable;

	Humanoid humanoid;

	void Start () {
		humanoid = GetComponent<Humanoid> ();
	}

	public void AddPickable (Pickable pickable) {

		if ( pickable.CarryType == Pickable.CarryTypes.TwoHanded ) {
			RightHandPickable = pickable;
			LeftHandPickable = pickable;
		}

		if ( RightHandPickable == null ) {
			RightHandPickable = pickable;
			return;
		}

		if ( LeftHandPickable == null ) {
			LeftHandPickable = pickable;
			return;
		}

	}

	public void RemovePickable (Pickable pickable) {

		if ( pickable.CarryType == Pickable.CarryTypes.TwoHanded ) {
			RightHandPickable = null;
			LeftHandPickable = null;
			return;
		}

		if ( RightHandPickable != null ) {
			RightHandPickable = null;
			return;
		}

		if ( LeftHandPickable != null ) {
			LeftHandPickable = null;
			return;
		}

	}

	public bool FullHands {
		get {
			return RightHandPickable != null && LeftHandPickable != null;
		}
	}

	public bool EmptyHands {
		get {
			return RightHandPickable == null && LeftHandPickable == null;
		}
	}

	public Pickable LeftHandPickable {
		get {
			return leftHandPickable;
		}
		set {
			leftHandPickable = value;
		}
	}

	public Pickable RightHandPickable {
		get {
			return rightHandPickable;
		}
		set {
			rightHandPickable = value;
		}
	}
}
