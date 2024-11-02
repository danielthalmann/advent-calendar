using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;

    public float verticalOffset = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 t = new Vector3(target.transform.position.x, target.transform.position.y + verticalOffset, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, t, Time.deltaTime * 10);
    }
}
