using UnityEngine;

public class LightControl : MonoBehaviour {
    float offsetSpeedX;
    float offSetSpeedY;
    public float maxSpeed;
    public float minSpeed;

    Renderer rend;

    Vector2 currentOffset;

    // Start is called before the first frame update
    void Start() {
        rend = GetComponent<Renderer>();
        offsetSpeedX = Random.Range(-maxSpeed, maxSpeed);
        offSetSpeedY = Random.Range(minSpeed, maxSpeed);

        currentOffset = new Vector2(Random.value, Random.Range(-0.19f, 0));

    }

    // Update is called once per frame
    void Update() {
        currentOffset += new Vector2(offsetSpeedX, currentOffset.y) * Time.deltaTime;

        rend.material.mainTextureOffset = currentOffset;
    }
}
