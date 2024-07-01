using UnityEngine;

public class MoveObject_Triggerable : Triggerable {
    public GameObject target;
    public string message;

    public override void Trigger() {
        base.Trigger();

        target.SendMessage(message);
    }
}
