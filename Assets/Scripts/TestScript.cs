using UnityEngine;

[ExecuteInEditMode]
public class TestScript : MonoBehaviour {

    public Transform target;
    public Animator animator;
    public float weight;
    public AvatarIKGoal goal;

    public void OnAnimatorIK(int layerIndex) {
        animator.SetIKPosition(goal, target.position);
        animator.SetIKPositionWeight(goal, weight);
    }


    // Start is called before the first frame update
    void Start() {

    }
}
