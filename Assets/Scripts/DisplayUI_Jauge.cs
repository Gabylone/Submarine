using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayUI_Jauge : MonoBehaviour
{
    public Submarine.Value value;

    public RectTransform target_RectTransform;
    public RectTransform current_RectTransform;

    public float speed = 1.0f;

    public float maxY = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float targetLerp = Submarine.Instance.GetLerp(value);
        //target_RectTransform.anchoredPosition = Vector3.Lerp(target_RectTransform.anchoredPosition, Vector3.up * targetLerp * maxY, speed * Time.deltaTime);
        target_RectTransform.anchoredPosition = Vector3.up * targetLerp * maxY;

        if ( current_RectTransform != null )
        {
            float currentLerp = Submarine.Instance.currentTiltAngle / Submarine.Instance.maxTiltSpeed;
            //current_RectTransform.anchoredPosition = Vector3.MoveTowards(current_RectTransform.anchoredPosition, Vector3.up * currentLerp * maxY, speed * Time.deltaTime);
            current_RectTransform.anchoredPosition = Vector3.up * currentLerp * maxY;
        }

    }
}