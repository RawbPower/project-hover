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
    public float angularWobbleAmplitude, angularWobbleFrequency;

    private float linearVelocity, angularVelocity, verticalVelocity, wobbleVelocity;
    private float midVelocity = 0, midWobble = 0;
    private float linearAcceleration, angularAcceleration, verticalAcceleration = 0, wobbleAcceleration = 0;
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
        verticalVelocity = verticalWobbleAmplitude*verticalWobbleFrequency;
        wobbleVelocity = angularWobbleAmplitude * angularWobbleFrequency;
        // Get the position of thrusters from COM
        mainThruster = this.transform.GetChild(0).GetChild(1);
        rightThruster = this.transform.GetChild(0).GetChild(2);
        leftThruster = this.transform.GetChild(0).GetChild(3);
        thrusterDistance = Vector3.Distance(this.transform.position, rightThruster.position);
    }

    // Update is called once per frame
    void FixedUpdate()
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

        // Calculate vertical motion (Velocity Verlet method)
        midVelocity = verticalVelocity + verticalAcceleration*(Time.deltaTime/2);
        this.transform.GetChild(0).position += (midVelocity * transform.up * Time.deltaTime);
        // Prevent any build up of small errors
        if (Mathf.Abs(transform.GetChild(0).localPosition.y) > verticalWobbleAmplitude)
            this.transform.GetChild(0).localPosition = new Vector3(transform.GetChild(0).localPosition.x, verticalWobbleAmplitude * Mathf.Sign(transform.GetChild(0).localPosition.y), transform.GetChild(0).localPosition.z);
        verticalAcceleration = -gravity;
        verticalAcceleration += gravity - verticalWobbleAmplitude * verticalWobbleFrequency * verticalWobbleFrequency * Mathf.Sin(verticalWobbleFrequency*Time.time);
        verticalVelocity = midVelocity + verticalAcceleration * (Time.deltaTime / 2);

        // Calculate angular wobble
        midWobble = wobbleVelocity + wobbleAcceleration * (Time.deltaTime / 2);
        this.transform.GetChild(0).RotateAround(transform.position, transform.forward, midWobble * Time.deltaTime);
        // Prevent any build up of small errors
        Debug.Log(Mathf.Abs(transform.GetChild(0).localEulerAngles.z));
        if (transform.GetChild(0).localEulerAngles.z > angularWobbleAmplitude && transform.GetChild(0).localEulerAngles.z < 180.0f)
            this.transform.GetChild(0).localEulerAngles = new Vector3(transform.GetChild(0).localEulerAngles.x, transform.GetChild(0).localEulerAngles.y, angularWobbleAmplitude);
        else if (transform.GetChild(0).localEulerAngles.z < 360.0f - angularWobbleAmplitude && transform.GetChild(0).localEulerAngles.z > 180.0f)
            this.transform.GetChild(0).localEulerAngles = new Vector3(transform.GetChild(0).localEulerAngles.x, transform.GetChild(0).localEulerAngles.y, 360.0f - angularWobbleAmplitude);
        wobbleAcceleration = - angularWobbleAmplitude * angularWobbleFrequency * angularWobbleFrequency * Mathf.Sin(angularWobbleFrequency * Time.time);
        wobbleVelocity = midWobble + wobbleAcceleration * (Time.deltaTime / 2);

        // Update position
        transform.RotateAround(transform.position, transform.up, angularVelocity*Time.deltaTime);
        transform.position += (linearVelocity * transform.forward * Time.deltaTime);
        //Debug.Log("A: " + verticalAcceleration + " - V: " + verticalVelocity + " - X: " + this.transform.GetChild(0).position);
        //Debug.Log("A: " + wobbleAcceleration + " - V: " + wobbleVelocity + " - X: " + this.transform.GetChild(0).localEulerAngles.z);
    }
}
