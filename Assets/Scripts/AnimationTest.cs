using UnityEngine;

public class AnimationTest : MonoBehaviour {
    public GameObject prefab;
    public GameObject copy;

    public Transform parent;

    public Animator animator;

    public Transform origin;
    public Transform target;

    // Start is called before the first frame update
    void LateUpdate() {
        /*if (copy != null)
        {
            Destroy(copy);
        }

        copy = Instantiate(prefab, parent);
        copy.transform.rotation = parent.rotation;
        copy.transform.position += Vector3.right * 1f;*/



        Transform[] bones = origin.GetComponentsInChildren<Transform>();
        int a = 0;

        foreach (var item in target.GetComponentsInChildren<Transform>()) {
            item.localPosition = bones[a].localPosition;
            item.localRotation = bones[a].localRotation;

            ++a;
        }

    }
}
