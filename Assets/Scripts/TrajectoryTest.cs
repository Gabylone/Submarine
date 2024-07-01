using UnityEngine;

public class TrajectoryTest : MonoBehaviour {
    public int pointAmount = 10;

    public float speed = 0f;

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        for (int i = 0; i < pointAmount; i++) {
            // speed
            // distance
            // time 
        }
    }
}
