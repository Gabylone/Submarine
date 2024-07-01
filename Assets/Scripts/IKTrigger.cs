using UnityEngine;

[System.Serializable]
public class IKTrigger {
    public Transform[] targets = new Transform[5];

    public Transform GetTarget(IKParam.Type type) {
        return targets[(int)type];
    }
}
