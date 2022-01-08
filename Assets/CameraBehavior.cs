using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    private Transform _transform;
    public Transform target;

    public float lerpSpeed = 1f;

    public Vector3 decal;

    // Start is called before the first frame update
    void Start()
    {
        _transform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = target.position + (Player.Instance.Body.forward * decal.z);
        Vector3 dir = targetPos - _transform.position;

        _transform.forward = Vector3.Lerp(_transform.forward , dir.normalized, lerpSpeed * Time.deltaTime);
    }
}
