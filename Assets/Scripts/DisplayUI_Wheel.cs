using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayUI_Wheel : MonoBehaviour
{
    public Submarine.Value value;

    public float maxAngle = 90f;
    public RectTransform anchor;

    public float lerpSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float lerp = Submarine.Instance.GetLerp(value);

        Vector3 eulerAngles = Vector3.forward * lerp * maxAngle;

        Quaternion rot = Quaternion.Euler(eulerAngles);
        anchor.localRotation = Quaternion.Lerp(anchor.localRotation, rot, lerpSpeed * Time.deltaTime);

    }
}
