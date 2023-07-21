 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Door : MonoBehaviour
{
    private Vector3 initPos;

    public Vector3 targetPos;

    public float move_duration = 1f;

    float timer = 0f;

    public float duration = 4f;

    public bool opened = false;

    public delegate void OnSwitch();
    public OnSwitch onSwitch;

    public ButtonTrigger buttonTrigger1;
    public ButtonTrigger buttonTrigger2;

    private void Start()
    {
        initPos = transform.localPosition;

        buttonTrigger1.onTrigger += Switch;
        buttonTrigger2.onTrigger += Switch;
    }

    private void Update()
    {
        if (opened)
        {

            if (timer >= duration)
            {
                Switch();
            }

            timer += Time.deltaTime;
        }
    }

    public void Switch()
    {
        timer = 0f;

        opened = !opened;


        if (opened)
        {
            transform.DOLocalMove(initPos + targetPos, move_duration);
        }
        else
        {
            transform.DOLocalMove(initPos, move_duration);
        }

        if (onSwitch != null)
        {
            onSwitch();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        Gizmos.DrawWireCube(transform.position, Vector3.one);
        Gizmos.DrawWireCube(transform.position + targetPos, Vector3.one);
        Gizmos.DrawLine(transform.position, transform.position + targetPos);
    }
}
