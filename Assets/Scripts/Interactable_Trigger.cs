using UnityEngine;

public class Interactable_Trigger : MonoBehaviour {
    public Interactable linkedInteractable;

    private float timer = 0f;

    bool added = false;

    private void Update() {
        if (linkedInteractable.selected) {
            if (timer >= 0) {
                timer -= Time.deltaTime;
            } else {
                linkedInteractable.Deselect();
                InteractableManager.Instance.RemoveInteractable(linkedInteractable);
                added = false;
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (linkedInteractable.interacting || InteractableManager.Instance.interacting) {
            if (added) {
                linkedInteractable.Deselect();
                InteractableManager.Instance.RemoveInteractable(linkedInteractable);
                added = false;
            }
            return;
        }

        Player player = other.GetComponent<Player>();

        if (player != null) {
            timer = 0.1f;

            if (!added) {
                InteractableManager.Instance.AddInteractable(linkedInteractable);
                added = true;
            }

            /*if ( !linkedInteractable.selected)
            {
                linkedInteractable.Select();
            }*/

        }
    }
}
