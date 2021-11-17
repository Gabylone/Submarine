using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayTurnSpeed : MonoBehaviour {

	[SerializeField]
	private Sub_Turn subTurn;

	[SerializeField]
	private Text baseText;
	[SerializeField]
	private Text currentSpeedText;

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
		case ActionType.TurnLeft:
		case ActionType.TurnRight:
		case ActionType.StopTurn:
		case ActionType.Stop:
			Display ();
			break;
		default:
			break;
		}
	}

	void Display () {
		
		string str = "currTurn:";

		for (int i = 0; i <= subTurn.maxTurnSteps; i++) {

			if (i == subTurn.CurrentTurnStep) {
				str += string.Format("<color=red>{0}</color>","I");
//				str += "I";
			} else {
				str += '-';
			}

		}

		TextToDisplay textToDisplay = new TextToDisplay (
			str,
			baseText,
			0.03f
		);

		Type.DisplayText (textToDisplay);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
