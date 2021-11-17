using UnityEngine;
using System.Collections;

public class Discussion : Interactable {

	private Humanoid linkedHuman;
	private Humanoid otherHuman;

	public override void Start ()
	{
		base.Start ();
		linkedHuman = GetComponentInParent<Humanoid> ();
	}

	public override void Interact (Humanoid humanoid)
	{
		base.Interact (humanoid);

		otherHuman = humanoid;

		otherHuman.Stop (100);
		linkedHuman.Stop (100);

		otherHuman.GetAnimator.SetLookAtPosition (linkedHuman.Dialogue.Anchor.position);
		linkedHuman.GetAnimator.SetLookAtPosition (otherHuman.Dialogue.Anchor.position);

		otherHuman.GetAnimator.SetLookAtWeight (1);
		linkedHuman.GetAnimator.SetLookAtWeight (1);
		TurnToEachOther ();

		Available = false;
	}

	private void TurnToEachOther () {
		//

		linkedHuman.GetTransform.forward = (otherHuman.GetTransform.position - linkedHuman.GetTransform.position);
		otherHuman.BodyTransform.forward = -(otherHuman.GetTransform.position - linkedHuman.GetTransform.position);

	}

	public override void Update ()
	{
		base.Update ();

		if (Available == false) {
			if (Timer >= linkedHuman.Dialogue.Duration * 2)
				Available = true;
		}
	}

	#region question
	#endregion

}
