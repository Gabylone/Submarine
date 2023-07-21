using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineCollider : MonoBehaviour
{
    public bool proximity = false;

    bool touchingWall = false;
    float timer = 0f;

    public int id;

    public LayerMask layerMask;

    public float radius = 50f;

    public Collider collider;

    private void Update()
    {
        if (proximity)
        {
            RaycastHit hit;

        }

        if (touchingWall)
        {
            timer -= Time.deltaTime;

            if (timer < 0f)
            {
                touchingWall = false;
                Submarine.Instance.ExitCollision(id);
            }
        }
    }



    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ocean Walls")
        {
            if (proximity)
            {
                if (!touchingWall)
                {
                    Submarine.Instance.ApproachCollision(id);
                }

                touchingWall = true;
                timer = 0.5f;

            }
            else
            {
                Submarine.Instance.Crash();
            }
        }
    }

    
    private void OnDrawGizmos()
    {
        /*if (proximity)
        {
            RaycastHit hit;

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, radius);

            if ( Physics.Raycast( transform.position, transform.up, out hit, radius, layerMask)
        }*/
    }
}
