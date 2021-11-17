using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stoplights : MonoBehaviour {

	public float rotSpeed = 1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;

		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		Vector3 targetDir = transform.parent.forward;

		if ( Physics.Raycast (ray , out hit ) ) {

			targetDir = (hit.point - transform.position); 

		}

		transform.forward = Vector3.MoveTowards( transform.forward , targetDir, rotSpeed * Time.deltaTime );
	}
}
