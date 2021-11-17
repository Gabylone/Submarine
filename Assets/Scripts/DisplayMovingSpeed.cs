using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayMovingSpeed : MonoBehaviour {
	
	[SerializeField]
	private Text baseText;

	[SerializeField]
	private Sub_Move subMovement;

	// Use this for initialization
	void Start () {
		SubmarineInput.Instance.getAction += HandleGetAction;

		Clear ();

		Display ();
	}

	void Clear ()
	{
		baseText.text = "";
	}

	void HandleGetAction (Action action)
	{
		switch (action.actionType) {

		case ActionType.Accelerate:
		case ActionType.Deccelerate:
		case ActionType.StopMoving:
		case ActionType.Stop:
			Display ();
			break;
		default:
			break;
		}
	}

	void Display () {

		string str = "currSpd:";

		for (int i = 0; i <= subMovement.maxSpeedStep; i++) {

			if (i == subMovement.CurrentSpeedStep) {
				str += string.Format ("<color=red>{0}</color>", subMovement.CurrentSpeedStep.ToString ());
			} else {
				

			}
			str += '_';
		}

		TextToDisplay textToDisplay = new TextToDisplay (
			str,
			baseText,
			0.05f
		);

		Type.DisplayText (textToDisplay);
	}
}
