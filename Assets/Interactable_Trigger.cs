using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Trigger : MonoBehaviour
{
    public bool inside = false;

    public Interactable linkedInteractable;

    private BoxCollider BoxCollider;

    private void Start()
    {
        BoxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if ( inside && Input.GetKeyDown(KeyCode.I))
        {
            if (!linkedInteractable.interacting)
            {
                linkedInteractable.Interact_Start();
            }
        }
    }

    public void Enable()
    {
        linkedInteractable.EnableOutline();

        /*if (ikTrigger != null)
        {
            if (ikTrigger.head_Target != null)
            {
                IKManager.Instance.SetTarget(IKManager.IKParam.Type.Head, ikTrigger.head_Target);
            }
        }*/

        inside = true;
    }

    public void Disable()
    {
        linkedInteractable.DisableOutline();

        IKManager.Instance.Stop(IKManager.IKParam.Type.Head);
        inside = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Enable();
    }

    private void OnTriggerExit(Collider other)
    {
        Disable();
    }
}
