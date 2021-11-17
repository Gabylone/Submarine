using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDiveAngle : MonoBehaviour {

	[SerializeField]
	private Sub_RotateUp subRotation;

	[SerializeField]
	private Text pointerText;

	[SerializeField]
	private Text currentAngleText;

	void Start () {
		SubmarineInput.Instance.getAction += HandleGetAction;

		Clear ();

		Display ();
	}

	void Clear ()
	{
		pointerText.text = "";
	}

	void Update () {

//		bool over = subDive.currentDiveAngle > subDive.targetDiveAngle - 1;

		if ( Mathf.Approximately (subRotation.currAngle , subRotation.targetAngle) || Type.typing) {
			currentAngleText.gameObject.SetActive (false);
		} else {
			currentAngleText.gameObject.SetActive (true);
			currentAngleText.rectTransform.eulerAngles = new Vector3 (0,0,-subRotation.currAngle);
			//
		}

	}

	void HandleGetAction (Action action)
	{

		switch (action.actionType) {
		case ActionType.TurnUp:
		case ActionType.TurnDown:
		case ActionType.Stabilize:
		case ActionType.Stop:
			Display ();
			break;
		default:
			break;
		}
	}

	void Display ()
	{
		Clear ();	

		pointerText.rectTransform.eulerAngles = new Vector3 (0,0,-subRotation.targetAngle);

		string str = "---> " + Mathf.RoundToInt(subRotation.targetAngle) + "°";

		TextToDisplay textToDisplay = new TextToDisplay (
			str,
			pointerText,
			0.2f
		);

		Type.DisplayText (textToDisplay);
	}
}
