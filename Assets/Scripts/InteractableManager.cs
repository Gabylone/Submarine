using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InteractableManager : MonoBehaviour {
    public static InteractableManager Instance;

    private void Awake() {
        Instance = this;
    }

    public List<Interactable> Interactables = new List<Interactable>();

    public Interactable previous;
    public Interactable current;

    public bool interacting = false;

    bool someSelected = false;

    private void Update() {
        if (Interactables.Count > 0) {
            someSelected = true;
            Interactable closest = Interactables[0];

            for (int i = 1; i < Interactables.Count; i++) {
                float disToClosest = Vector3.Distance(Player.Instance.GetTransform.position, closest.GetTransform.position);
                float disToItem = Vector3.Distance(Player.Instance.GetTransform.position, Interactables[i].GetTransform.position);

                if (disToItem < disToClosest) {
                    closest = Interactables[i];
                }
            }

            if (!closest.selected) {
                if (previous != null) {
                    previous.Selected_Exit();
                }

                current = closest;
                previous = current;

                closest.Selected_Start();
            }
        } else {
            if (someSelected) {
                someSelected = false;
                IKManager.Instance.Stop(IKParam.Type.Head);
                IKManager.Instance.Stop(IKParam.Type.LeftHand);
                IKManager.Instance.Stop(IKParam.Type.RightHand);
            }
        }
    }

    public void AddInteractable(Interactable interactable) {
        Interactables.Add(interactable);
    }

    public void RemoveInteractable(Interactable interactable) {
        Interactables.Remove(interactable);
    }
}
