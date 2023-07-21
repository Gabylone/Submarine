using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class SpeedDisplay : MonoBehaviour
{
    public Transform _transform;
    public float maxAngle = 250f;

    public Text uiText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float lerp = Submarine.Instance.moveSpeed / Submarine.Instance.maxMoveSpeed;

        _transform.localEulerAngles = Vector3.right * lerp * maxAngle;
        Color c = Color.Lerp( Color.green,Color.red,lerp );
        _transform.GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor",c);

        uiText.text = "" + Mathf.Round(Submarine.Instance.moveSpeed) + "\n" + "m/s";
    }
}
