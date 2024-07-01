using UnityEngine;

public class MoveTest : MonoBehaviour {
    public float speed = 1f;


    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        dir = Camera.main.transform.TransformDirection(dir);
        dir.y = 0f;
        dir.Normalize();
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }
}
