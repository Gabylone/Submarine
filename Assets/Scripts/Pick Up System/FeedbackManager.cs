using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FeedbackManager : MonoBehaviour {

	public static FeedbackManager Instance;

	[SerializeField]
	private RectTransform feedbackTransform;

	float timer = 0f;

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (feedbackTransform.gameObject.activeSelf) {
			if (timer > 0) { 
				timer -= Time.deltaTime;
			} else {
				feedbackTransform.gameObject.SetActive (false);
			}
		}
	}

	public void Place ( Vector3 worldPos ) {

		Vector3 v = Camera.main.WorldToViewportPoint (worldPos);

		feedbackTransform.anchorMin = new Vector2 (v.x , v.y);
		feedbackTransform.anchorMax = new Vector2 (v.x , v.y);

		feedbackTransform.gameObject.SetActive (true);

		timer = 0.2f;

	}
}
