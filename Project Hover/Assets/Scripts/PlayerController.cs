using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float centerAcceleration;
    public float reverseRatio;
    public float sideAcceleration;
    public float strafeShift;
    public float linearDrag;
    public float angularDrag;
    public float horizontalDrag;
    public float sideLinearDrag;
    public float sideAngularDrag;
    public float linearThreshold = 0.1f;
    public float angularThreshold = 0.1f;
    public float strafeTime;

    public float gravity = 10f;
    public float verticalWobbleAmplitude, verticalWobbleFrequency;
    public float angularWobbleAmplitude, angularWobbleFrequency;

    private float linearVelocity, angularVelocity, verticalVelocity, wobbleVelocity, horizontalVelocity;
    private float midVelocity = 0, midWobble = 0;
    private float linearAcceleration, angularAcceleration, verticalAcceleration = 0, wobbleAcceleration = 0, horizontalAcceleration = 0;
    private float thrusterDistance, brakeDistance;

    private Transform mainThruster;
    private Transform rightThruster;
    private Transform leftThruster;
    private Transform rightBrake;
    private Transform leftBrake;
    private Transform rightStrafer;
    private Transform leftStrafer;
    private bool axisInUse;
    private float strafeTimer;
    private int strafeDir = 0;
    private bool strafing;
    private bool reverse;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the velocities
        axisInUse = false;
        strafing = false;
        linearVelocity = 0;
        angularVelocity = 0;
        horizontalVelocity = 0;
        verticalVelocity = verticalWobbleAmplitude*verticalWobbleFrequency;
        wobbleVelocity = angularWobbleAmplitude * angularWobbleFrequency;
        // Get the position of thrusters from COM
        mainThruster = this.transform.GetChild(0).GetChild(1);
        rightThruster = this.transform.GetChild(0).GetChild(2);
        leftThruster = this.transform.GetChild(0).GetChild(3);
        rightBrake = this.transform.GetChild(0).GetChild(4);
        leftBrake = this.transform.GetChild(0).GetChild(5);
        rightStrafer = this.transform.GetChild(0).GetChild(6);
        leftStrafer = this.transform.GetChild(0).GetChild(7);
        thrusterDistance = Vector3.Distance(this.transform.position, rightThruster.position);
        brakeDistance = Vector3.Distance(this.transform.position, rightBrake.position);
        Debug.Log(rightStrafer.name);
    }

    // Update is called once per frame
    void Update()
    {
        // Make flame appear
        mainThruster.GetChild(0).gameObject.SetActive(Input.GetButton("Fire1"));
        mainThruster.GetChild(1).gameObject.SetActive(Input.GetButton("Fire2"));
        rightThruster.GetChild(0).gameObject.SetActive(Input.GetButton("LeftBurst"));
        leftThruster.GetChild(0).gameObject.SetActive(Input.GetButton("RightBurst"));
        rightBrake.gameObject.SetActive(Input.GetAxis("RightBrake") > 0);
        leftBrake.gameObject.SetActive(Input.GetAxis("LeftBrake") > 0);

        if (linearVelocity > 0)
        {
            reverse = false;
        } else if (Input.GetButtonDown("Fire2") && linearVelocity <= 0)
        {
            reverse = true;
        }

            // Calculate acceleration
        linearAcceleration = (centerAcceleration) * System.Convert.ToSingle(Input.GetButton("Fire1"));
        linearAcceleration += - reverseRatio*(centerAcceleration) * System.Convert.ToSingle(Input.GetButton("Fire2"));
        linearAcceleration += sideAcceleration * (System.Convert.ToSingle(Input.GetButton("RightBurst")) + System.Convert.ToSingle(Input.GetButton("LeftBurst")));
        linearAcceleration -= linearDrag * linearVelocity * linearVelocity * Mathf.Sign(linearVelocity);
        linearAcceleration -= sideLinearDrag * (System.Math.Sign(Input.GetAxis("RightBrake")) + System.Math.Sign(Input.GetAxis("LeftBrake"))) * linearVelocity * linearVelocity * Mathf.Sign(linearVelocity);

        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            if (axisInUse == false)
            {
                if (!strafing)
                {
                    strafing = true;
                    strafeDir = System.Math.Sign(Input.GetAxisRaw("Horizontal"));
                }
                axisInUse = true;
            } else
            {
                horizontalAcceleration = 0;
            }
        }
        if (Input.GetAxisRaw("Horizontal") == 0)
        {
            horizontalAcceleration = 0;
            axisInUse = false;
        }

        if (strafing)
        {
            horizontalAcceleration = strafeDir * strafeShift * 4 * (Mathf.PI * Mathf.PI)/(strafeTime*strafeTime) * Mathf.Sin(2* Mathf.PI * strafeTimer/strafeTime);
            leftStrafer.GetChild(0).GetChild(0).gameObject.SetActive(horizontalAcceleration > 0);
            leftStrafer.GetChild(1).GetChild(0).gameObject.SetActive(horizontalAcceleration > 0);
            rightStrafer.GetChild(0).GetChild(0).gameObject.SetActive(horizontalAcceleration < 0);
            rightStrafer.GetChild(1).GetChild(0).gameObject.SetActive(horizontalAcceleration < 0);
            strafeTimer += Time.deltaTime;
            Debug.Log(horizontalAcceleration);
        } else
        {
            leftStrafer.GetChild(0).GetChild(0).gameObject.SetActive(false);
            leftStrafer.GetChild(1).GetChild(0).gameObject.SetActive(false);
            rightStrafer.GetChild(0).GetChild(0).gameObject.SetActive(false);
            rightStrafer.GetChild(1).GetChild(0).gameObject.SetActive(false);
        }

        if (strafeTimer > strafeTime)
        {
            strafing = false;
            strafeTimer = 0;
            strafeDir = 0;
            horizontalVelocity = 0;     // This, not sure not really physical but may just be fixing error. Must check maths
        }

        //horizontalAcceleration -= horizontalDrag * horizontalVelocity * horizontalVelocity * Mathf.Sign(horizontalVelocity);

        // Calculate angular acceleration
        angularAcceleration = (sideAcceleration/thrusterDistance) * (System.Convert.ToSingle(Input.GetButton("RightBurst")) - System.Convert.ToSingle(Input.GetButton("LeftBurst"))) * Mathf.Rad2Deg;
        angularAcceleration -= sideAngularDrag * (System.Math.Sign(Input.GetAxis("LeftBrake")) - System.Math.Sign(Input.GetAxis("RightBrake"))) * Mathf.Rad2Deg;
        //Debug.Log(System.Math.Sign(Input.GetAxis("RightBrake")) - System.Math.Sign(Input.GetAxis("LeftBrake")));

        // Might need to fixed the angular drag to something more realistic
        angularAcceleration -= angularDrag * angularVelocity * angularVelocity * Mathf.Sign(angularVelocity);
        //Debug.Log(angularAcceleration);

        // Update Velocities
        linearVelocity += linearAcceleration * Time.deltaTime;
        if (!reverse && linearVelocity < 0)
            linearVelocity = 0;
        angularVelocity += angularAcceleration * Time.deltaTime;
        horizontalVelocity += horizontalAcceleration * Time.deltaTime;

        //Debug.Log(angularVelocity);
        //Debug.LogError(angularVelocity);
        if (Mathf.Abs(linearVelocity) < linearThreshold)
            linearVelocity = 0;
        if (Mathf.Abs(angularVelocity) < angularThreshold)
            angularVelocity = 0;
        if (Mathf.Abs(horizontalVelocity) < linearThreshold)
            horizontalVelocity = 0;

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
        //Debug.Log(Mathf.Abs(transform.GetChild(0).localEulerAngles.z));
        if (transform.GetChild(0).localEulerAngles.z > angularWobbleAmplitude && transform.GetChild(0).localEulerAngles.z < 180.0f)
            this.transform.GetChild(0).localEulerAngles = new Vector3(transform.GetChild(0).localEulerAngles.x, transform.GetChild(0).localEulerAngles.y, angularWobbleAmplitude);
        else if (transform.GetChild(0).localEulerAngles.z < 360.0f - angularWobbleAmplitude && transform.GetChild(0).localEulerAngles.z > 180.0f)
            this.transform.GetChild(0).localEulerAngles = new Vector3(transform.GetChild(0).localEulerAngles.x, transform.GetChild(0).localEulerAngles.y, 360.0f - angularWobbleAmplitude);
        wobbleAcceleration = - angularWobbleAmplitude * angularWobbleFrequency * angularWobbleFrequency * Mathf.Sin(angularWobbleFrequency * Time.time);
        wobbleVelocity = midWobble + wobbleAcceleration * (Time.deltaTime / 2);

        // Update position
        transform.RotateAround(transform.position, transform.up, angularVelocity*Time.deltaTime);
        transform.position += (linearVelocity * transform.forward * Time.deltaTime);
        transform.position += (horizontalVelocity * Vector3.Cross(transform.up, transform.forward) * Time.deltaTime); 
        //Debug.Log("A: " + verticalAcceleration + " - V: " + verticalVelocity + " - X: " + this.transform.GetChild(0).position);
        //Debug.Log("A: " + wobbleAcceleration + " - V: " + wobbleVelocity + " - X: " + this.transform.GetChild(0).localEulerAngles.z);
    }
}
