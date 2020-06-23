using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float maxSpeed = 10f;
    public float airDrag = 0.4f;
    public float linearAcceleration = 2f;
    public float rotSpeed = 60f;
    public float threshold = 0.1f;
    private float vel, rot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rot = rotSpeed * (System.Convert.ToSingle(Input.GetButton("RightBurst")) - System.Convert.ToSingle(Input.GetButton("LeftBurst")));
        vel += linearAcceleration * System.Convert.ToSingle(Input.GetButton("Fire1"));
        vel -= airDrag*vel;
        if (vel < threshold)
            vel = 0;
        Debug.Log(vel);
        transform.RotateAround(transform.position, transform.up, rot*Time.deltaTime);
        transform.position += (vel * transform.forward * Time.deltaTime);
    }
}
