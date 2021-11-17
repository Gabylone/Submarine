using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub : MonoBehaviour {

	public static Sub Instance;

	public int health = 100;

	void Awake () {
		Instance = this;
	}
}