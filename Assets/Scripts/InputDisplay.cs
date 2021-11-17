using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputDisplay : MonoBehaviour {

	private Text inputDisplay_Text;

	// Use this for initialization
	void Start () {

		inputDisplay_Text = GetComponent<Text> ();

		Clear ();

		SubmarineInput.Instance.getAction += HandleGetAction;
	}

	void Clear ()
	{
		inputDisplay_Text.text = "";
	}

	void HandleGetAction (Action action)
	{
		inputDisplay_Text.text = action.actionType.ToString ();
//		Type.DisplayText (new TextToDisplay(action.actionType.ToString() , inputDisplay_Text , 0.1f));
	}

}
