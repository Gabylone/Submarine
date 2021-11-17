using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour {

	public float positionSpeed = 10f;
	public float rotationSpeed = 50f;

	Vector3 initDecal = Vector3.zero;
	Vector3 dirToSub = Vector3.zero;

	public Transform target;

	// Use this for initialization
	void Start () {

		initDecal = Sub.Instance.transform.position - transform.position;

	}

	void Update () {

		transform.forward = Vector3.MoveTowards ( transform.forward , target.forward , rotationSpeed * Time.deltaTime );
		transform.position = target.position;

//		if ( Input.GetKey(KeyCode.RightArrow) ) {
//			transform.Rotate ( Vector3.up * rotationSpeed * Time.deltaTime , Space.World);
//		}
//
//		if ( Input.GetKey(KeyCode.LeftArrow) ) {
//			transform.Rotate ( Vector3.up * -rotationSpeed * Time.deltaTime, Space.World);
//		}

	}
}
