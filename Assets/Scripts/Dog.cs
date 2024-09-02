using UnityEngine;

public class Dog : MonoBehaviour {
    Animator anim;

    public float speed = 0f;
    public float maxSpeed = 1f;

    public float rotSpeed = 10f;

    public float distanceToStop = 1f;

    public float acceleration = 1f;
    public float decceleration = 5f;

    // Start is called before the first frame update
    void Start() {
        anim = GetComponent<Animator>();
    }

}
