using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnDisplay : MonoBehaviour
{
    public Transform _transform;

    public float maxPos = 0.42f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float lerp = Submarine.Instance.turnSpeed / Submarine.Instance.maxTurnSpeed;
        _transform.localPosition = Vector3.right * lerp * maxPos;
    }
}
