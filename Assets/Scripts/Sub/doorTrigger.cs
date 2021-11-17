using UnityEngine;
using System.Collections;

public class doorTrigger : MonoBehaviour {

	public int count = 0;

	Vector3 initPos;

	float timer = 0f;
	[SerializeField]
	private float duration = 1f;

	bool lerping = false;

	[SerializeField]
	private Animator doorAnimator;

	private bool canOpen = true;

	void OnTriggerEnter ( Collider other ) {

        Player player = other.GetComponent<Player>();

		if (player != null && canOpen) {
			++Count;

			if (count == 1)
				timer = 0f;
		}



		//	
	}

	void OnTriggerExit ( Collider other ) {

        Player player = other.GetComponent<Player>();

        if (player != null && canOpen)
        {
			--Count;

			if (count == 0)
				timer = 0f;
		}
	}


	public int Count {
		get {
			return count;
		}
		set {
			count = Mathf.Clamp (value, 0, 100);

			doorAnimator.SetBool ("Opened", count > 0);

			print ("count : " + count);

			if (count > 0)
				print ("opened");
		}
	}

	public bool CanOpen {
		get {
			return canOpen;
		}
		set {
			canOpen = value;

			if ( value == false )
				doorAnimator.SetBool ("Opened", false);
		}
	}
}
