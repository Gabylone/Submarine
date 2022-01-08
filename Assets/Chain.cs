using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chain : MonoBehaviour
{
    public GameObject linkPrefab;
    public GameObject lastObjPrefab;

    public GameObject base_Obj;

    public List<GameObject> links_objs = new List<GameObject>();

    GameObject dangler_obj;
    public float pushForce = 5f;

    public int amount = 10;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject instance = Instantiate(linkPrefab, transform) as GameObject;

            Vector3 v = new Vector3( 0 , -(instance.transform.localScale.y*2)  * i , 0 );
            instance.transform.position = transform.position + v;
            instance.name = "Link (" + i + ")";

            if (i == 0)
            {
                //instance.GetComponent<SpringJoint>().connectedBody = base_Obj.GetComponent<Rigidbody>();
                instance.GetComponent<SpringJoint>().connectedBody = base_Obj.GetComponent<Rigidbody>();
            }
            else
            {
                //instance.GetComponent<SpringJoint>().connectedBody = links_objs[links_objs.Count-1].GetComponent<Rigidbody>();
                instance.GetComponent<SpringJoint>().connectedBody = links_objs[links_objs.Count-1].GetComponentsInChildren<Rigidbody>()[1];
            }

            links_objs.Add(instance);
        }

        dangler_obj = Instantiate(lastObjPrefab, transform) as GameObject;

        Vector3 v1 = new Vector3(0, -(dangler_obj.transform.localScale.y * 2) * amount, 0);
        dangler_obj.transform.position = transform.position + v1;
        dangler_obj.name = "Dangler";

        dangler_obj.GetComponent<SpringJoint>().connectedBody = links_objs[links_objs.Count - 1].GetComponentsInChildren<Rigidbody>()[1];

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            dangler_obj.transform.Translate(Vector3.down * pushForce * Time.deltaTime);
        }
    }
}
