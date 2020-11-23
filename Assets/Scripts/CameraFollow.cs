using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float     speed = 0.1f;

    Vector3 baseOffset;

    // Start is called before the first frame update
    void Start()
    {
        baseOffset = target.position - transform.position;   
    }

    // Update is called once per frame
    void Update()
    {
        var delta = target.position - transform.position - baseOffset;

        var newPos = transform.position + speed * delta;

        transform.position = newPos;
    }
}
