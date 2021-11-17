using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayHelp : MonoBehaviour {

	[SerializeField]
	private Text targetText;

	void Start () {
		SubmarineInput.Instance.getAction += HandleGetAction;

		Hide ();
	}
	
	// Update is called once per frame
	void Hide () {
		targetText.text = "";
	}

	void Display() {

		Hide ();

		string str = "your goal : find the red ball\n\n";

		foreach ( Action action in SubmarineInput.Instance.Actions ) {

			str += action.actionType.ToString ().ToUpper() + " :  ";

			foreach ( string phrase in action.associatedPhrases ) {
				str += phrase + ", ";
			}

			str += "\n";
		}

		TextToDisplay textToDisplay = new TextToDisplay (
			str,
			targetText,
			0.01f
		);

		Type.DisplayText (textToDisplay);
	}

	void HandleGetAction (Action action)
	{
		switch (action.actionType) {
		case ActionType.DisplayHelp:
			Display ();
			break;
		case ActionType.HideHelp:
			Hide ();
			break;
		default:
			break;
		}
	}
}
