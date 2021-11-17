using UnityEngine;
using System.Collections;

public class IKManager : MonoBehaviour {

	[Header ("leaning params")]
	[SerializeField]
	private bool enableLean = true;
	[SerializeField]
	private Transform backTransform;
	[SerializeField]
	private float backArchSpeed = 1f;

	private float initBackAngle = 90f;
	private float backAngle = 0f;
	private bool leaning = false;

	public float yToLean = 0.5f;

	[Header("IK Controls")]
	[SerializeField]
	private IKControl leftArmIK;
	[SerializeField]
	private IKControl rightArmIK;

    public Transform target;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (target == null)
            {
                ApplyIK(Camera.main.transform);
            }
            else
            {
                RemoveIK();
            }
        }
    }

    // Update is called once per frame
    void LateUpdate () {
		
       

		if ( enableLean )
			LeanToBall ();
	}

	#region leaning
	public float BackAngle {
		get {
			return backAngle;
		}
		set {
			backAngle = Mathf.Clamp (value , 0f, 90f);
		}
	}

	float currentDis = 0f;

	void LeanToBall () {

		if (leaning) {

			BackAngle += backArchSpeed * Time.deltaTime;

			backTransform.localEulerAngles = new Vector3 (0f, initBackAngle - BackAngle, 0f);

		} else if (BackAngle > 0) {
			BackAngle -= backArchSpeed * Time.deltaTime;

			backTransform.localEulerAngles = new Vector3 (0f, initBackAngle - backAngle, 0f);
		}

	}
	#endregion

	public void ApplyIK (Transform _target) {

        target = _target;

        leftArmIK.SetTarget(target);
        rightArmIK.SetTarget(target);

    }

	public void RemoveIK () {

        target = null;

		Leaning = false;

        LeftArmIK.RemoveTarget();
        rightArmIK.RemoveTarget();

    }

	public IKControl LeftArmIK {
		get {
			return leftArmIK;
		}
	}

	public IKControl RightArmIK {
		get {
			return rightArmIK;
		}
	}

	public bool Leaning {
		get {
			return leaning;
		}
		set {
			leaning = value;
		}
	}

}
