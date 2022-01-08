using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Interactable : MonoBehaviour
{
    public bool interacting = false;

    public IKTrigger ikTrigger;
    private Interactable_Trigger Interactable_Trigger;

    public Transform player_anchor;

    public bool disableMovements = true;

    private float timer = 0f;

    public virtual void Start()
    {
        Interactable_Trigger = GetComponentInChildren<Interactable_Trigger>();
    }

    public virtual void Update()
    {
        if (interacting)
        {
            Interact_Update();
        }
    }

    private void LateUpdate()
    {
        if (interacting)
        {
            Interact_LateUpdate();
        }
    }

    public virtual void Interact_Start()
    {
        Interactable_Trigger.Disable();

        interacting = true;

        DisableOutline();

        timer = 0f;

        if ( player_anchor != null)
        {
            Player.Instance.GetTransform.DOMove(player_anchor.position, 0.5f);
            Player.Instance.Body.DORotateQuaternion(player_anchor.rotation, 0.5f);
        }

        CheckIKs();

        if ( disableMovements)
        {
            Player.Instance.DisableMovement();
        }
    }

    public virtual void Interact_Update()
    {
        if ( timer >=  0.2f && Input.GetKeyDown(KeyCode.I))
        {
            Interact_Exit();
        }

        timer += Time.deltaTime;
    }

    public virtual void Interact_LateUpdate()
    {

    }

    public virtual void Interact_Exit()
    {
        interacting = false;
        Interactable_Trigger.Enable();
    }

    public void EnableOutline()
    {
        foreach (var item in GetComponentsInChildren<Transform>())
        {
            item.gameObject.layer = 3;
        }
    }

    public void DisableOutline()
    {
        foreach (var item in GetComponentsInChildren<Transform>())
        {
            item.gameObject.layer = 0;
        }
    }

    private void OnDrawGizmos()
    {
        if ( player_anchor != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player_anchor.position, 0.3f);
        }

        if ( ikTrigger!= null)
        {
            Gizmos.color = Color.red;
            if (ikTrigger.rightHand_Target != null)
            {
                Gizmos.DrawSphere(ikTrigger.rightHand_Target.position, 0.1f);
            }

            if (ikTrigger.rightFoot_Target != null)
            {
                Gizmos.DrawSphere(ikTrigger.rightFoot_Target.position, 0.1f);
            }

            if (ikTrigger.leftFoot_Target != null)
            {
                Gizmos.DrawSphere(ikTrigger.leftFoot_Target.position, 0.1f);
            }

            if (ikTrigger.leftHand_Target != null)
            {
                Gizmos.DrawSphere(ikTrigger.leftHand_Target.position, 0.1f);
            }

            if (ikTrigger.head_Target != null)
            {
                Gizmos.DrawSphere(ikTrigger.head_Target.position, 0.1f);
            }
        }
    }

    void CheckIKs()
    {
        if (ikTrigger != null)
        {
            if (ikTrigger.rightHand_Target != null)
            {
                IKManager.Instance.SetTarget(IKManager.IKParam.Type.RightHand, ikTrigger.rightHand_Target);
            }

            if (ikTrigger.rightFoot_Target != null)
            {
                IKManager.Instance.SetTarget(IKManager.IKParam.Type.RightFoot, ikTrigger.rightFoot_Target);
            }

            if (ikTrigger.leftFoot_Target != null)
            {
                IKManager.Instance.SetTarget(IKManager.IKParam.Type.LeftFoot, ikTrigger.leftFoot_Target);
            }

            if (ikTrigger.leftHand_Target != null)
            {
                IKManager.Instance.SetTarget(IKManager.IKParam.Type.LeftHand, ikTrigger.leftHand_Target);
            }

        }
    }
}
