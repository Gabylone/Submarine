using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverFeedback : MonoBehaviour
{
    private Lever lever;

    public Transform local_jauge_transform;
    public Transform local_pointer_transform;

    public bool global = true;
    public Submarine.Value value;
    public Transform global_jauge_transform;
    public Transform global_pointer_transform;
    public int global_jauge_way = 1;
    public float maxScale = 1f;

    public float speed = 1f;

    private void Start()
    {
        lever = GetComponentInParent<Lever>();

        if (!global)
        {
            global_jauge_transform.gameObject.SetActive(false);
            global_pointer_transform.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        float localLerp = lever.GetValue() / lever.valueMultiplier;

        Vector3 localScale = local_jauge_transform.localScale;
        localScale.y = localLerp * maxScale;

        local_jauge_transform.localScale = Vector3.Lerp(local_jauge_transform.localScale , localScale, speed * Time.deltaTime);
        local_pointer_transform.localPosition = Vector3.Lerp(local_pointer_transform.localPosition, Vector3.up * localLerp * maxScale, speed * Time.deltaTime);

        if (global)
        {
            float globalLerp = Submarine.Instance.GetLerp(value);
            Vector3 globalScale = global_jauge_transform.localScale;
            globalScale.y = globalLerp * maxScale;

            global_jauge_transform.localScale = Vector3.Lerp(global_jauge_transform.localScale , globalScale , speed * Time.deltaTime);

            global_pointer_transform.localPosition = Vector3.Lerp(global_pointer_transform.localPosition, Vector3.up * globalLerp * maxScale, speed * Time.deltaTime);
        }
    }
}
