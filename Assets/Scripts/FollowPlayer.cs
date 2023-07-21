using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public bool followPos = false;

    Vector3 distanceToPlayer;

    public float moveSpeed = 1f;
    public float rotSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        distanceToPlayer = -(Player.Instance.GetTransform.position - transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (followPos)
        {
            transform.position = Vector3.Lerp(transform.position, Player.Instance.GetTransform.position + distanceToPlayer, moveSpeed * Time.deltaTime);
        }

        Vector3 dir = Player.Instance.transform.position - transform.position;
        transform.forward = Vector3.Lerp(transform.forward, dir.normalized, rotSpeed * Time.deltaTime);
    }
}
