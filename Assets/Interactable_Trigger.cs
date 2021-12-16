using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Trigger : MonoBehaviour
{
    public bool inside = false;

    public Interactable linkedInteractable;

    BoxCollider BoxCollider;

    private void Start()
    {
        BoxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if ( inside && Input.GetKeyDown(KeyCode.I))
        {
            if (linkedInteractable.interacting)
            {
                BoxCollider.enabled = true;
                linkedInteractable.Interact_Exit();
            }
            else
            {
                BoxCollider.enabled = false;
                linkedInteractable.Exit();
                linkedInteractable.Interact_Start();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        linkedInteractable.Enter();

        inside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        linkedInteractable.Exit();

        IKManager.Instance.Stop(IKManager.IKParam.Type.Head);

        inside = false;
    }
}
