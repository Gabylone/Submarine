using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Wheel : Interactable
{
    public int index_left = 0;
    public int index_right = 3;
    public Transform[] anchors;

    public int indexRate = 2;

    private float angle = 0f;
    public float angleToStop = 0f;
    private float timer = 0f;
    public float timeBetweenTurns = 1f;

    bool turning = false;

    public Transform _targetTransform;
    public float rotateSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    public override void Interact_Start()
    {
        base.Interact_Start();
        ChangeAnchor(false);
    }

    public override void Interact_Update()
    {
        base.Interact_Update();

        Turn_Update();
    }

    public override void Interact_Exit()
    {
        base.Interact_Exit();
     
        Player.Instance.EnableMovement();

        IKManager.Instance.StopHands();
    }

    void Turn_Update()
    {
        if (turning)
        {
            if (Input.GetAxis("Horizontal") > -.1f
                &&
                Input.GetAxis("Horizontal") < .1f
                )
            {
                return;
            }

            if ( Input.GetAxis("Horizontal") < 0)
            {
                angle -= rotateSpeed * Time.deltaTime;
                _targetTransform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
            }
            else
            {
                angle += rotateSpeed * Time.deltaTime;
                _targetTransform.Rotate(-Vector3.forward * rotateSpeed * Time.deltaTime);
            }


            if (angle >= angleToStop || angle <= -angleToStop)
            {
                if (angle >= angleToStop )
                {
                    ChangeAnchor(false);
                }
                if (angle <= -angleToStop)
                {
                    ChangeAnchor(true);
                }

                timer = 0f;
                turning = false;
            }
        }
        else
        {
            timer += Time.deltaTime;

            if (timer >= timeBetweenTurns)
            {
                angle = 0f;
                turning = true;
            }
        }
    }

    void ChangeAnchor(bool left)
    {
        if (left)
        {
            index_left -= indexRate;
            index_right -= indexRate;

            if (index_right < 0)
            {
                index_right += anchors.Length;
            }

            if (index_left < 0)
            {
                index_left += anchors.Length;
            }
        }
        else
        {
            index_left += indexRate;
            index_right += indexRate;

            if (index_right >= anchors.Length)
            {
                index_right -= anchors.Length;
            }

            if (index_left >= anchors.Length)
            {
                index_left -= anchors.Length;
            }
        }

        

        IKManager.Instance.SetTarget(IKManager.IKParam.Type.LeftHand, anchors[index_left]);
        IKManager.Instance.SetTarget(IKManager.IKParam.Type.RightHand, anchors[index_right]);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player_anchor.position, 0.3f);

        Gizmos.color = Color.red;
        foreach (var anchor in anchors)
        {
            Gizmos.DrawWireSphere(anchor.position, 0.1f);
        }
    }
}
