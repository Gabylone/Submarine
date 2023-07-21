using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whale : MonoBehaviour
{
    public Transform _transform;

    public float speed = 1.0f;

    public float rotSpeed = 10f;

    public Transform target;

    public Transform[] bones;

    public float lerpSpeed = 1f;

    public float lerpSpeed2 = 0.5f;

    public float boneDecal = 1f;
    public float moveSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {

        Vector3 dir = target.position - _transform.position;
        dir.Normalize();

        Quaternion rot = Quaternion.LookRotation(-bones[1].forward, dir);
        bones[0].rotation = Quaternion.RotateTowards(bones[0].rotation, rot, rotSpeed * Time.deltaTime);

        for (int i = bones.Length-1; i > 0; i--)
        {
            bones[i].up = Vector3.MoveTowards(bones[i].up, -dir, lerpSpeed2 * Time.deltaTime * (i*3f));
        }
        /*for (int i = 1; i < bones.Length; i++)
        {
            Vector3 dir2 = bones[i - 1].position - bones[i].position;
            dir2.Normalize();
            Vector3 pos = bones[i - 1].position - dir * boneDecal;
            bones[i].position = Vector3.Lerp(bones[i].position , pos, moveSpeed * Time.deltaTime);
            bones[i].up = Vector3.Lerp(bones[i].up, -dir2, lerpSpeed2 * Time.deltaTime );
        }*/

        _transform.Translate(dir * speed * Time.deltaTime);


    }
}
