using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Display_DiveSpeed : MonoBehaviour {

	[SerializeField]
	private Sub_Dive subDive;

	[SerializeField]
	private Text targetText;

	// Use this for initialization
	void Start () {
		SubmarineInput.Instance.getAction += HandleGetAction;
		Clear ();
		Display ();
	}
	void Clear ()
	{
		targetText.text = "";
	}

	void Update () {
		string str = Mathf.RoundToInt (subDive.currSpeed) + "-m/s";
		targetText.text = str;
//		if ( !Type.typing ) {
//			
//		}
	}
	
	void Display ()
	{
		return;
		string str = Mathf.RoundToInt (subDive.currSpeed) + "-m/s";

		TextToDisplay textToDisplay = new TextToDisplay(
			str,
			targetText,
			0.01f
		);

		Type.DisplayText(textToDisplay);
	}

	void HandleGetAction (Action action)
	{

		switch (action.actionType) {
		case ActionType.Rise:
		case ActionType.Dive:
		case ActionType.Stabilize:
		case ActionType.Stop:
			Display ();
			break;
		default:
			break;
		}
	}
}
