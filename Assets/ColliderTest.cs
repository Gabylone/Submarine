using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTest : MonoBehaviour
{
    public LayerMask LayerMask;
    public float radius = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnDrawGizmos()
    {
        var colliders = Physics.OverlapSphere(transform.position, radius, LayerMask);
        if ( colliders.Length == 0) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, radius);
            return;
        }
        foreach (var collider in colliders) {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawLine(transform.position, collider.ClosestPoint(transform.position));
        }
    }
}
