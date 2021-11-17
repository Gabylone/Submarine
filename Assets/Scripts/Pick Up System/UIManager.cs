using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

	public static UIManager Instance;

	[SerializeField]
	private Transform[] canvasTransforms;

	void Awake () {
		Instance = this;
	}

	public void Place ( RectTransform recTransform, Vector3 worldPos ) {

		Vector3 v = Camera.main.WorldToViewportPoint (worldPos);

		recTransform.anchorMin = new Vector2 (v.x , v.y);
		recTransform.anchorMax = new Vector2 (v.x , v.y);

	}

	public GameObject CreateElement (GameObject prefab, int canvasID = 0) {

		if (canvasID>= canvasTransforms.Length ) {
			canvasID = 0;
			Debug.Log ("canvas id superior to canvas array " + canvasID    );
		}

		GameObject go = Instantiate (prefab) as GameObject;
		go.transform.SetParent (canvasTransforms[canvasID]);
		go.transform.localScale = Vector3.one;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;

		go.SetActive (false);

		return go;
	}
}
