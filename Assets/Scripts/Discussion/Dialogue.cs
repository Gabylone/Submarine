using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Dialogue : MonoBehaviour {

	bool speaking = false;

	[Header("Bubble")]
	[SerializeField]
	private GameObject bubblePrefab;
	private Bubble bubble;

	[SerializeField]
	private float duration = 1f;
	float timer = 0f;

	[SerializeField]
	private Transform anchor;

	// Use this for initialization
	void Start () {
		CreateBubble ();
	}

	void LateUpdate () {

		if ( speaking ) {

			if (timer > delay) {

				UIManager.Instance.Place (bubble.RectTransform, anchor.position);

				bubble.Visible = true;

				if (timer > (duration+delay)) {
					Exit ();
				}

			}

			timer += Time.deltaTime;

		}

	}

	private void CreateBubble () {
		bubble = UIManager.Instance.CreateElement (bubblePrefab, 1).GetComponent<Bubble>();
		bubble.Init ();
	}

	float delay = 0f;

	public void Speak ( string phrase , float _delay ) {

		if (speaking)
			return;

		bubble.Text.text = phrase;

		speaking = true;

		delay = _delay;

		timer = 0f;

	}

	public void Speak ( string phrase ) {
		Speak (phrase, 0f);
	}

	private void Exit () {

		speaking = false;

		bubble.Visible = false;
	}

	public Transform Anchor {
		get {
			return anchor;
		}
		set {
			anchor = value;
		}
	}

	public bool Speaking {
		get {
			return speaking;
		}
	}

	public float Duration {
		get {
			return duration;
		}
	}
}
