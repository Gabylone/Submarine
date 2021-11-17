using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractionTrigger : MonoBehaviour {

	RectTransform feedbackRect;

	bool inside = false;

	Interactable linkedInteractable = null;

	float lerp = 0f;
	private float lerpSpeed = 8.4f;

	void Start () {

		LinkedInteractable = GetComponentInParent<Interactable> ();
		if ( LinkedInteractable == null ) {

			Debug.LogError ("no linked interactable in " + name);

		}

	}

	void OnTriggerEnter ( Collider other ) {

		InteractionManager interactionManager = other.GetComponent<InteractionManager>();

		if (interactionManager != null) {
			Enter (interactionManager);
			return;
		}


	}

	void OnTriggerExit ( Collider other ) {
		if (other.tag == "Player") {
			Exit (other.GetComponent<InteractionManager>());
		}
	}

	private void Enter (InteractionManager interactionManager) {

		if (LinkedInteractable.Available == false)
			return;

		interactionManager.InteractionTriggers.Add (this);
		inside = true;

	}

	public void Exit (InteractionManager interactionManager) {

		interactionManager.InteractionTriggers.Remove (this);
		lerp = 0f;
		inside = false;
	}

	public Interactable LinkedInteractable {
		get {
			return linkedInteractable;
		}
		set {
			linkedInteractable = value;
		}
	}
}
