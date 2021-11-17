using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDepth : MonoBehaviour {

	[SerializeField]
	private Transform targetTransform;

	[SerializeField]
	private Text targetText;

	int stepAmount = 10;

	int unitsBetweenSteps = 100;

	int subCurrentStep = 0;

	void Start () {
		
//		SubmarineInput.Instance.getAction += HandleGetAction;

		Clear ();

		Display ();
	}

	void Clear ()
	{
		targetText.text = "";
	}

	void Update () {

			// if sub is below current step
		if ( (targetTransform.position.y) < -(int)( (subCurrentStep+1) *unitsBetweenSteps) ) {
			print ("sub is below");
			subCurrentStep++;
			Display ();
		}
//
		if ( (targetTransform.position.y) > -(int)(subCurrentStep*unitsBetweenSteps) ) {
			print ("sub is above");
			subCurrentStep--;
			Display ();
		}

		if ( Type.typing == false ) {
			QuickDisplay ();
		}

	}

	string getDepthText () {
		string str = "";

		bool placedSub = false;

		for (int stepIndex = 0; stepIndex < stepAmount; stepIndex++) {

			if ( (-targetTransform.position.y) < (int)(stepIndex*unitsBetweenSteps) && !placedSub) {
				string subPosText = "o(" + (int)targetTransform.position.y + ")\n";
				str += string.Format("<color=white>{0}</color>",subPosText);

				placedSub = true;

			}

			str += (int)(stepIndex*unitsBetweenSteps) + "----|\n";



		}

		return str;
	}

	void QuickDisplay ()
	{
		targetText.text = getDepthText ();
	}

	void Display ()
	{
		/*TextToDisplay textToDisplay = new TextToDisplay(
			getDepthText(),
			targetText,0.0,
			0.01f
		);

		Type.DisplayText(textToDisplay);*/
	}
}
