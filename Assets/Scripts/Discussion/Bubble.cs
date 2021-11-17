using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Bubble : MonoBehaviour {

	private Image image;
	private Text text;
	private RectTransform rectTransform;
	bool visible = false;

	public int index = 0;

	public void Init () {
		image = GetComponent<Image> ();
		text = GetComponentInChildren<Text> ();
		rectTransform = GetComponent<RectTransform> ();

		Visible = false;
	}

	public Image Image {
		get {
			return image;
		}
		set {
			image = value;
		}
	}

	public Text Text {
		get {
			return text;
		}
		set {
			text = value;
		}
	}

	public bool Visible {
		get {
			return visible;
		}
		set {
			visible = value;

			gameObject.SetActive (value);
		}
	}

	public RectTransform RectTransform {
		get {
			return rectTransform;
		}
		set {
			rectTransform = value;
		}
	}
}
