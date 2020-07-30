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
    public float frontArea;
    public float angularDrag;
    public float sideLinearDrag;
    public float sideArea;
    public float linearThreshold = 0.1f;
    public float angularThreshold = 0.1f;
    public float strafeTime;

    public float mass = 100f;
    public float density = 1.225f;

    public float gravity = 10f;
    public float verticalWobbleAmplitude, verticalWobbleFrequency;
    public float angularWobbleAmplitude, angularWobbleFrequency;

    private float angularVelocity, verticalVelocity, wobbleVelocity;
    private Vector3 linearVelocity;
    private float midVelocity = 0, midWobble = 0;
    private float angularAcceleration, verticalAcceleration = 0, wobbleAcceleration = 0;
    private Vector3 linearAcceleration;
    private float thrusterDistance, thrusterAngle, brakeDistance, brakeAngle;

    private Transform mainThruster;
    private Transform rightThruster;
    private Transform leftThruster;
    private Transform rightBrake;
    private Transform leftBrake;
    private Transform rightStrafer;
    private Transform leftStrafer;
    private bool axisInUse;
    private float strafeTimer;
    private Vector3 strafeDir;
    private bool strafing;
    private bool reverse;
    private float strafeRatio;


    // Start is called before the first frame update
    void Start()
    {
        // Initialize the velocities
        axisInUse = false;
        strafing = false;
        linearVelocity = new Vector3(0, 0, 0);
        angularVelocity = 0;
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
        thrusterAngle = Vector3.Angle(rightThruster.position - this.transform.position, mainThruster.position - this.transform.position);
        brakeDistance = Vector3.Distance(this.transform.position, rightBrake.position);
        brakeAngle = Vector3.Angle(rightBrake.position - this.transform.position, mainThruster.position - this.transform.position);
        Debug.Log(thrusterAngle + " - " + brakeAngle);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Make flame appear
        linearAcceleration = new Vector3(0,0,0);
        angularAcceleration = 0;
        mainThruster.GetChild(0).gameObject.SetActive(Input.GetButton("Fire1"));
        mainThruster.GetChild(1).gameObject.SetActive(Input.GetButton("Fire2"));
        rightThruster.GetChild(0).gameObject.SetActive(Input.GetButton("LeftBurst"));
        leftThruster.GetChild(0).gameObject.SetActive(Input.GetButton("RightBurst"));
        rightBrake.gameObject.SetActive(Input.GetAxis("RightBrake") > 0);
        leftBrake.gameObject.SetActive(Input.GetAxis("LeftBrake") > 0);

        /*if (linearVelocity > 0)
        {
            reverse = false;
        } else if (Input.GetButtonDown("Fire2") && linearVelocity <= 0)
        {
            reverse = true;
        }*/

        // Calculate acceleration with strafe
     
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            if (axisInUse == false)
            {
                if (!strafing)
                {
                    strafing = true;
                    strafeRatio = Mathf.Abs(strafeShift - (linearDrag) * Vector3.Dot(linearVelocity, linearVelocity)) / (strafeShift + (linearDrag + 2 * sideLinearDrag) * Vector3.Dot(linearVelocity, linearVelocity));
                    //Debug.Log("Ratio - " + strafeRatio);
                    strafeDir = System.Math.Sign(Input.GetAxisRaw("Horizontal")) * Vector3.Cross(transform.up, transform.forward).normalized;
                }
                axisInUse = true;
            }
        }
        if (Input.GetAxisRaw("Horizontal") == 0)
        {
            axisInUse = false;
        }

        if (strafing) {
            if (strafeTimer < strafeTime)
            {
                /*horizontalAcceleration = strafeDir * strafeShift * (Mathf.PI * Mathf.PI)/(4*strafeTime*strafeTime) * Mathf.Sin((Mathf.PI * strafeTimer)/(2*strafeTime));
               leftStrafer.GetChild(0).GetChild(0).gameObject.SetActive(horizontalAcceleration > 0);
               leftStrafer.GetChild(1).GetChild(0).gameObject.SetActive(horizontalAcceleration > 0);
               rightStrafer.GetChild(0).GetChild(0).gameObject.SetActive(horizontalAcceleration < 0);
               rightStrafer.GetChild(1).GetChild(0).gameObject.SetActive(horizontalAcceleration < 0);
               linearAcceleration += horizontalAcceleration * Vector3.Cross(transform.up, transform.forward).normalized;*/
                linearAcceleration += strafeDir * strafeShift;
                strafeTimer += Time.deltaTime;
            }
            else if (strafeTimer > strafeTime && strafeTimer < (strafeTime + strafeTime))
            {
                linearAcceleration += -strafeDir * strafeShift;
                strafeTimer += Time.deltaTime;
            }
            else
            {
                strafing = false;
                strafeTimer = 0;
                //Debug.Log(Vector3.Cross(transform.up, transform.forward).normalized);
                //Debug.Log(linearVelocity);
                //Debug.Log(Vector3.Dot(linearVelocity, Vector3.Cross(transform.up, transform.forward)) * Vector3.Cross(transform.up, transform.forward).normalized);
                linearVelocity -= Vector3.Dot(linearVelocity, Vector3.Cross(transform.up, transform.forward))* Vector3.Cross(transform.up, transform.forward).normalized;     // This, not sure not really physical but may just be fixing error. Must check maths
                //Debug.Log(linearVelocity);
                strafeDir = new Vector3(0,0,0);
            }
        }

        //Debug.Log(linearAcceleration);
        if (!strafing)
        {
            linearAcceleration += (centerAcceleration) * System.Convert.ToSingle(Input.GetButton("Fire1")) * transform.forward;
            linearAcceleration += -reverseRatio * (centerAcceleration) * System.Convert.ToSingle(Input.GetButton("Fire2")) * transform.forward;
            linearAcceleration += sideAcceleration * (System.Convert.ToSingle(Input.GetButton("RightBurst")) + System.Convert.ToSingle(Input.GetButton("LeftBurst"))) * transform.forward;
            linearAcceleration -= 0.5f * (density/mass) * frontArea * linearDrag * Vector3.Dot(linearVelocity, linearVelocity) * linearVelocity.normalized;
            //Debug.Log(linearDrag * Vector3.Dot(linearVelocity, linearVelocity) * linearVelocity.normalized.x);
            //Debug.Log(linearDrag * Vector3.Dot(linearVelocity, linearVelocity) * linearVelocity.normalized);
            //linearAcceleration -= linearDrag * new Vector3(linearVelocity.x * linearVelocity.x, linearVelocity.y * linearVelocity.y, linearVelocity.z * linearVelocity.z);
            linearAcceleration -= 0.5f * (density / mass) * sideArea * sideLinearDrag * (System.Math.Sign(Input.GetAxis("RightBrake")) + System.Math.Sign(Input.GetAxis("LeftBrake"))) * Vector3.Dot(linearVelocity, linearVelocity) * linearVelocity.normalized;
        }

        //horizontalAcceleration -= horizontalDrag * horizontalVelocity * horizontalVelocity * Mathf.Sign(horizontalVelocity);

        // Calculate angular acceleration
        angularAcceleration = ((sideAcceleration*Mathf.Sin(thrusterAngle))/thrusterDistance) * (System.Convert.ToSingle(Input.GetButton("LeftBurst")) - System.Convert.ToSingle(Input.GetButton("RightBurst"))) * Mathf.Rad2Deg;
        angularAcceleration -= 0.5f * (density / mass) * sideArea * ((sideLinearDrag * Vector3.Dot(linearVelocity, linearVelocity) * (System.Math.Sign(Input.GetAxis("RightBrake")) - System.Math.Sign(Input.GetAxis("LeftBrake"))) * Mathf.Sin(brakeAngle))/brakeDistance) * Mathf.Rad2Deg;
        //Debug.Log(sideAngularDrag * angularVelocity * angularVelocity * (System.Math.Sign(Input.GetAxis("LeftBrake")) - System.Math.Sign(Input.GetAxis("RightBrake"))));

        // Might need to fixed the angular drag to something more realistic
        angularAcceleration -= angularDrag * angularVelocity * angularVelocity * Mathf.Sign(angularVelocity);
        //Debug.Log(angularAcceleration);

        //Debug.Log(linearAcceleration);
        // Update Velocities
        linearVelocity += linearAcceleration * Time.deltaTime;
        /*if (!reverse && linearVelocity < 0)
            linearVelocity = 0;*/
        angularVelocity += angularAcceleration * Time.deltaTime;

        //Debug.Log(angularVelocity);
        //Debug.LogError(angularVelocity);
        if (Mathf.Abs(linearVelocity.magnitude) < linearThreshold)
            linearVelocity = new Vector3(0,0,0);
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
        //Debug.Log(Mathf.Abs(transform.GetChild(0).localEulerAngles.z));
        if (transform.GetChild(0).localEulerAngles.z > angularWobbleAmplitude && transform.GetChild(0).localEulerAngles.z < 180.0f)
            this.transform.GetChild(0).localEulerAngles = new Vector3(transform.GetChild(0).localEulerAngles.x, transform.GetChild(0).localEulerAngles.y, angularWobbleAmplitude);
        else if (transform.GetChild(0).localEulerAngles.z < 360.0f - angularWobbleAmplitude && transform.GetChild(0).localEulerAngles.z > 180.0f)
            this.transform.GetChild(0).localEulerAngles = new Vector3(transform.GetChild(0).localEulerAngles.x, transform.GetChild(0).localEulerAngles.y, 360.0f - angularWobbleAmplitude);
        wobbleAcceleration = - angularWobbleAmplitude * angularWobbleFrequency * angularWobbleFrequency * Mathf.Sin(angularWobbleFrequency * Time.time);
        wobbleVelocity = midWobble + wobbleAcceleration * (Time.deltaTime / 2);

        // Update position
        transform.RotateAround(transform.position, transform.up, angularVelocity*Time.deltaTime);
        transform.position += (linearVelocity * Time.deltaTime);
        Debug.Log(transform.position);
        //Debug.Log("A: " + verticalAcceleration + " - V: " + verticalVelocity + " - X: " + this.transform.GetChild(0).position);
        //Debug.Log("A: " + wobbleAcceleration + " - V: " + wobbleVelocity + " - X: " + this.transform.GetChild(0).localEulerAngles.z);
    }
}
