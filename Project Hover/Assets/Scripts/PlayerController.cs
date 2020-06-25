using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float centerAcceleration;
    public float sideAcceleration;
    public float linearDrag;
    public float angularDrag;
    public float linearThreshold = 0.1f;
    public float angularThreshold = 0.1f;

    public float gravity = 10f;
    public float verticalWobbleAmplitude, verticalWobbleFrequency;

    private float linearVelocity, angularVelocity, verticalVelocity;
    private float linearAcceleration, angularAcceleration, verticalAcceleration;
    private float thrusterDistance;

    private Transform mainThruster;
    private Transform rightThruster;
    private Transform leftThruster;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the velocities
        linearVelocity = 0;
        angularVelocity = 0;
        verticalVelocity = 0;
        // Get the position of thrusters from COM
        mainThruster = this.transform.GetChild(0).GetChild(1);
        rightThruster = this.transform.GetChild(0).GetChild(2);
        leftThruster = this.transform.GetChild(0).GetChild(3);
        thrusterDistance = Vector3.Distance(this.transform.position, rightThruster.position);
        Debug.Log(rightThruster.name);
    }

    // Update is called once per frame
    void Update()
    {
        // Make flame appear
        mainThruster.GetChild(0).gameObject.SetActive(Input.GetButton("Fire1"));
        rightThruster.GetChild(0).gameObject.SetActive(Input.GetButton("LeftBurst"));
        leftThruster.GetChild(0).gameObject.SetActive(Input.GetButton("RightBurst"));
        // Calculate acceleration
        linearAcceleration = (centerAcceleration) * System.Convert.ToSingle(Input.GetButton("Fire1"));
        linearAcceleration += sideAcceleration * (System.Convert.ToSingle(Input.GetButton("RightBurst")) + System.Convert.ToSingle(Input.GetButton("LeftBurst")));
        linearAcceleration -= linearDrag * linearVelocity;
        // Calculate angular acceleration
        angularAcceleration = (sideAcceleration/thrusterDistance) * (System.Convert.ToSingle(Input.GetButton("RightBurst")) - System.Convert.ToSingle(Input.GetButton("LeftBurst"))) * Mathf.Rad2Deg;
        //Debug.Log(angularAcceleration);
        angularAcceleration -= angularDrag * angularVelocity;
        //Debug.Log(angularAcceleration);
        // Update Velocities
        linearVelocity += linearAcceleration * Time.deltaTime;
        angularVelocity += angularAcceleration * Time.deltaTime;
        //Debug.LogError(angularVelocity);
        if (linearVelocity < linearThreshold)
            linearVelocity = 0;
        if (Mathf.Abs(angularVelocity) < angularThreshold)
            angularVelocity = 0;

        // Calculate vertical motion
        verticalAcceleration = -gravity;
        verticalAcceleration += gravity + verticalWobbleAmplitude*gravity*Mathf.Cos(verticalWobbleFrequency*Time.time);
        Debug.Log(verticalAcceleration);
        verticalVelocity += verticalAcceleration * Time.deltaTime;
        // Update position
        transform.RotateAround(transform.position, transform.up, angularVelocity*Time.deltaTime);
        transform.position += (linearVelocity * transform.forward * Time.deltaTime);
        this.transform.GetChild(0).position += (verticalVelocity * transform.up * Time.deltaTime);
    }
}
