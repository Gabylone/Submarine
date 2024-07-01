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

    // Update is called once per frame
    void Update() {
        Vector3 dirToPlayer = (Player.Instance.GetTransform.position - transform.position).normalized;
        Vector3 dir = Vector3.Lerp(transform.forward, dirToPlayer, rotSpeed * Time.deltaTime);

        transform.forward = dir;

        if (Vector3.Distance(Player.Instance.GetTransform.position, transform.position) < distanceToStop) {
            speed = Mathf.Lerp(speed, 0f, decceleration * Time.deltaTime);
        } else {
            speed = Mathf.Lerp(speed, maxSpeed, acceleration * Time.deltaTime);
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        anim.SetFloat("move", speed / maxSpeed);
    }
}
