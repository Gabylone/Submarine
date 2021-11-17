using UnityEngine;
using System.Collections;

public class Interactable : MonoBehaviour {

	private bool available = true;

	float timer = 0f;

	private InteractionTrigger linkedTrigger;

	public virtual void Start () {
		linkedTrigger = GetComponentInChildren<InteractionTrigger> ();
	}

	public virtual void Interact (Humanoid humanoid) {
		
		if (!Available)
			return;

  		linkedTrigger.Exit (humanoid.GetComponent<InteractionManager> ());

		humanoid.TimeInState = 0f;
	}

	public virtual void Update () {
		timer += Time.deltaTime;
	}

	public bool Available {
		get {
			return available;
		}
		set {
			available = value;

			timer = 0f;
		}
	}

	public float Timer {
		get {
			return timer;
		}
		set {
			timer = value;
		}
	}
}