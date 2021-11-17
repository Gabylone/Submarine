using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour {
	
	List<InteractionTrigger> interactionTriggers = new List<InteractionTrigger> ();

	Humanoid humanoid;

	void Start () {
		humanoid = GetComponent<Humanoid> ();
	}

	public List<InteractionTrigger> InteractionTriggers {
		get {
			return interactionTriggers;
		}
		set {
			interactionTriggers = value;
		}
	}
}
