using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class InverseKinematics : MonoBehaviour {

	public Transform upperArm_Transition;
	public Transform forearm_Transition;
	public Transform hand_Transition;

	public Transform upperArm_Virtual;
	public Transform forearm_Virtual;
	public Transform hand_Virtual;

	public Transform elbow;
	public Transform target;

	public Transform upperArm;
	public Transform forearm;
	public Transform hand;

	[Space(20)]
	public Vector3 uppperArm_OffsetRotation;
	public Vector3 forearm_OffsetRotation;
	public Vector3 hand_OffsetRotation;
	[Space(20)]
	public bool handMatchesTargetRotation = true;
	[Space(20)]
	public bool debug;

	float angle;
	float upperArm_Length;
	float forearm_Length;
	float arm_Length;
	float targetDistance;
	float adyacent;

	[Range(0f,1f)]
	public float weight = 1f;

	public float speed = 1f;

	public bool executeInEditMode = true;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void LateUpdate () {
		/*if (!executeInEditMode && !Application.isPlaying)
		{
			return;
		}

        float targetWeight = target != null ? 1f : 0f;
		weight = Mathf.Lerp(weight, targetWeight, speed * Time.deltaTime);

		UpdateVirtualIKs();

		UpdateTransitionIKs();

		UpdateIKs();*/

	}

	public void SetTarget(Transform _target)
    {
        target = _target;
    }

	public void Stop()
    {
		target = null;
    }

	void UpdateIKs()
    {
		/*upperArm.localPosition = Vector3.Lerp(upperArm.localPosition, upperArm_Virtual.localPosition, weight);
		forearm.localPosition = Vector3.Lerp(forearm.localPosition, forearm_Virtual.localPosition, weight);
		hand.localPosition = Vector3.Lerp(hand.localPosition, hand_Virtual.localPosition, weight);

		upperArm.localRotation = Quaternion.Lerp(upperArm.localRotation, upperArm_Virtual.localRotation, weight);
		forearm.localRotation = Quaternion.Lerp(forearm.localRotation, forearm_Virtual.localRotation, weight);
		hand.localRotation = Quaternion.Lerp(hand.localRotation, hand_Virtual.localRotation, weight);*/

		float f = weight;

		upperArm.localPosition = Vector3.Lerp(upperArm.localPosition, upperArm_Transition.localPosition, f);
		forearm.localPosition = Vector3.Lerp(forearm.localPosition, forearm_Transition.localPosition, f);
		hand.localPosition = Vector3.Lerp(hand.localPosition, hand_Transition.localPosition, f);

		upperArm.localRotation = Quaternion.Lerp(upperArm.localRotation, upperArm_Transition.localRotation, f);
		forearm.localRotation = Quaternion.Lerp(forearm.localRotation, forearm_Transition.localRotation, f);
		hand.localRotation = Quaternion.Lerp(hand.localRotation, hand_Transition.localRotation, f);
	}

	void UpdateTransitionIKs()
	{
		if (target == null)
		{
			return;
		}

		float f = 1f;
		// ça n'a pas de sens ça
		//float f = speed * Time.deltaTime;

		upperArm_Transition.localPosition = Vector3.Lerp(upperArm_Transition.localPosition, upperArm_Virtual.localPosition, f);
		forearm_Transition.localPosition = Vector3.Lerp(forearm_Transition.localPosition, forearm_Virtual.localPosition, f);
		hand_Transition.localPosition = Vector3.Lerp(hand_Transition.localPosition, hand_Virtual.localPosition, f);

		upperArm_Transition.localRotation = Quaternion.Lerp(upperArm_Transition.localRotation, upperArm_Virtual.localRotation, f);
		forearm_Transition.localRotation = Quaternion.Lerp(forearm_Transition.localRotation, forearm_Virtual.localRotation, f);
		hand_Transition.localRotation = Quaternion.Lerp(hand_Transition.localRotation, hand_Virtual.localRotation, f);
	}

	void UpdateVirtualIKs()
    {
		if (target == null)
		{
			return;
		}

		upperArm_Virtual.LookAt(target, elbow.position - upperArm_Virtual.position);

		upperArm_Virtual.Rotate(uppperArm_OffsetRotation);

		Vector3 cross = Vector3.Cross(elbow.position - upperArm_Virtual.position, forearm_Virtual.position - upperArm_Virtual.position);

		upperArm_Length = Vector3.Distance(upperArm_Virtual.position, forearm_Virtual.position);
		forearm_Length = Vector3.Distance(forearm_Virtual.position, hand_Virtual.position);
		arm_Length = upperArm_Length + forearm_Length;
		targetDistance = Vector3.Distance(upperArm_Virtual.position, target.position);
		targetDistance = Mathf.Min(targetDistance, arm_Length - arm_Length * 0.001f);

		adyacent = ((upperArm_Length * upperArm_Length) - (forearm_Length * forearm_Length) + (targetDistance * targetDistance)) / (2 * targetDistance);

		angle = Mathf.Acos(adyacent / upperArm_Length) * Mathf.Rad2Deg;

		upperArm_Virtual.RotateAround(upperArm_Virtual.position, cross, -angle);

		forearm_Virtual.LookAt(target, cross);
		forearm_Virtual.Rotate(forearm_OffsetRotation);

		if (handMatchesTargetRotation)
		{
			hand_Virtual.rotation = target.rotation;
			hand_Virtual.Rotate(hand_OffsetRotation);
		}
	}

	void OnDrawGizmos(){
		if (debug) {

			if ( target == null)
            {
				return;
            }

			Gizmos.color = Color.green;
			Gizmos.DrawLine(upperArm_Virtual.position, forearm_Virtual.position);
			Gizmos.DrawLine(forearm_Virtual.position, hand_Virtual.position);

			Gizmos.color = Color.blue;
			float decal = 0.015f;
			Gizmos.DrawLine(upperArm_Transition.position+Vector3.up*decal, forearm_Transition.position + Vector3.up * decal);
			Gizmos.DrawLine(forearm_Transition.position + Vector3.up * decal, hand_Transition.position + Vector3.up * decal);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(upperArm_Virtual.position, target.position);
			Gizmos.DrawSphere(target.position, 0.1f);
		}
	}

}
