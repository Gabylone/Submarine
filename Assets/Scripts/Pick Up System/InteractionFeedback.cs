using UnityEngine;
using System.Collections;

public class InteractionFeedback : MonoBehaviour {

	private InteractionManager interactionManager;
	private PickableController piclabkeController;
	bool showFeedbacks = true;

	Humanoid humanoid;

	[SerializeField]
	private int feedbackAmount = 5;

	[SerializeField]
	private GameObject feedbackPrefab;
	[SerializeField]
	private GameObject[] feedbacksObjects;

	int closestID = 0;

	// Use this for initialization
	void Start () {
		
		interactionManager = GetComponent<InteractionManager> ();
		piclabkeController = GetComponent<PickableController> ();

		humanoid = GetComponent<Humanoid> ();

		feedbacksObjects = new GameObject[feedbackAmount];

		for( int i = 0; i < feedbackAmount ; ++i ) {

			feedbacksObjects [i] = UIManager.Instance.CreateElement (feedbackPrefab);

		}
	}
	// Update is called once per frame
	void Update () {
		
		if (interactionManager.InteractionTriggers.Count > 0) {

			if (piclabkeController.FullHands) {
				print ("hull hands");
				ShowFeedbacks = false;
				return;
			}

			if (!AllowFeedbacks) {
				print ("not allowed");
				return;
			}

			// demarquer le plus gros.
			InteractionTrigger closestTrigger = GetClosestTrigger ();
			feedbacksObjects [closestID].transform.localScale = Vector3.one * 2;

			UpdateFeedbackPositions ();

			if (Input.GetButton ("Action") && humanoid.TimeInState > 0.2f) {
				closestTrigger.LinkedInteractable.Interact (humanoid);

//				humanoid.IkManager.TargetInteractable = closestTrigger.LinkedInteractable;
//				humanoid.IkManager.ApplyIK ();
			}

			if (Input.GetButtonUp ("Action")) {
				if (piclabkeController.EmptyHands)
					humanoid.IkManager.RemoveIK ();
			}

		} else {
			ShowFeedbacks = false;
		}
	}

	#region feedback objects
	private void UpdateFeedbackPositions () {

		int a = 0;
		foreach ( GameObject feedback in feedbacksObjects ) {
			if ( a < interactionManager.InteractionTriggers.Count ) {
				UIManager.Instance.Place (feedback.GetComponent<RectTransform>(), interactionManager.InteractionTriggers[a].transform.position + Vector3.up * 0.5f);
			}

			feedback.SetActive (a < interactionManager.InteractionTriggers.Count);

			++a;
		}

	}

	public bool ShowFeedbacks {
		get {
			return showFeedbacks;
		}
		set {
			showFeedbacks = value;

			foreach (GameObject feedback in feedbacksObjects)
				feedback.SetActive (value);
		}
	}

	bool allowFeedbacks = true;

	public bool AllowFeedbacks {
		get {
			return allowFeedbacks;
		}
		set {
			allowFeedbacks = value;
		}
	}
	#endregion

	private InteractionTrigger GetClosestTrigger () {

		InteractionTrigger closestTrigger = interactionManager.InteractionTriggers [0];

		int a = 0;

		foreach ( InteractionTrigger trigger in interactionManager.InteractionTriggers ) {

			float newDot = Vector3.Dot ( humanoid.BodyTransform.forward , (trigger.transform.position - transform.position).normalized );
			float initDot = Vector3.Dot ( humanoid.BodyTransform.forward , (closestTrigger.transform.position - transform.position).normalized );

			if (newDot > initDot) {
				closestTrigger = trigger;
				closestID = a;
			} else {
				feedbacksObjects[a].transform.localScale = Vector3.one;
			}

			++a;

		}

		return closestTrigger;
	}


}
