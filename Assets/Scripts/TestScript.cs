using UnityEngine;

[ExecuteInEditMode]
public class TestScript : MonoBehaviour {
    public bool growBuildings = false;

    public Transform[] list;
    public float lenght = 1f;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.P) && growBuildings) {
            GameObject go_parent = new GameObject();
            go_parent.name = "New Buildings";
            foreach (var item in list) {
                GameObject go = Instantiate(item.gameObject, go_parent.transform);
                go.transform.position = item.transform.position + item.transform.up * lenght;

            }

            growBuildings = false;
        }
    }
}
