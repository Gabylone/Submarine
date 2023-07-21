using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiveDisplay : MonoBehaviour
{
    public float maxHeight = 0.45f;

    public Transform _transform;
    public Text uiText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float lerp = Submarine.Instance.diveSpeed / Submarine.Instance.maxDiveSpeed;

        _transform.localPosition = Vector3.up * lerp * maxHeight;

        uiText.text = "" + Mathf.RoundToInt(Submarine.Instance.diveSpeed) + "\n" + "m/s";
    }
}
