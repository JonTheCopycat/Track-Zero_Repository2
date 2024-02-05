using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarControl : MonoBehaviour
{
    public static float speedFactor = 0.75f;
    
    protected Rigidbody rb;
    protected Car car;
    protected CarGetter carGetter;
    //Collider collider;
    
    //timestamps
    float elapsedSinceBoost;
    float elapsedSinceQuickBoost;
    float countdownTimestamp;
    float extraTimestamp;

    //current car stats
    public float maxSpeed;
    public float handling;
    public float dHandling;
    public float acceleration;
    public float boostStrength;
    public float driftAcceleration;
    public float traction;
    public float oversteer;

    //miscellaneous
    public static readonly float slightDriftAngle = 1f;
    public static readonly float fullDriftAngle = 2.5f;
    static readonly bool maxSpeedUsed = true;
    public static bool normalGravity = false;
    private static int carsInARow;

    //traction
    float currentTraction;
    float driftAngle;
    float downforce;
    float rawDownforce;
    //full drift before hand
    //the start of a drift should not accelerate the player (into a wall)
    //but the end of a drift can be extended as the end drift angle is smaller than the start angle

    //all modifiers
    float terrainModifier;
    float rawTerrainModifier;
    float tractionModifier;
    float rawTractionModifier;
    float oversteerModifier;
    float rawOversteerModifier;
    float maxSpeedModifier;
    float accModifier;

    //physics and functional variables
    Quaternion rotationFromHit;
    Vector3 currentUp;
    Vector3 sideProjection;
    Vector3 frontProjection;
    Vector3 lastAngularVelocity;
    Vector3 lastVelocity;
    Coroutine BoostRoutine;
    Coroutine QuickBoostRoutine;
    float boostMeter;
    float doubleBoostMeter;
    float suspendDistance;
    int boostsReady;
    float oversteerVel;
    float handlingVel;
    float handlingAcc;
    float handlingLimiter;
    float driftAccLimiter;
    bool BST_running;
    float springAcceleration;
    float springVelocity;
    float springPosition;



    protected short status;
    public static short C_ENABLED = 0x1;
    public static short C_ONDIRT = 0x2;
    public static short C_ONICE = 0x4;
    public static short C_GROUNDED = 0x8;
    public static short C_BRAKING = 0x10;
    public static short C_SLIDING = 0x20;
    public static short C_DRIFTING = 0x40;
    public static short C_BSTING = 0x80;
    public static short C_DBSTING = 0x100;
    public static short C_BSTREADY = 0x200;
   
    public static short C_ONTURBO = 0x400;

    protected bool boostStart;


    RaycastHit raycastHit;

    //controls
    protected CarInputHandler inputHandler;
    //public ScreenManager screenManager;

    protected GameObject FinishLine;

    protected bool started = false;

    //collision tracking
    float lastCollisionStrength;
    float lastCollisionTime;

    public bool isBraking()
    {
        return (status & C_BRAKING) != 0;
    }

    public bool isSliding()
    {
        return (status & C_SLIDING) != 0;
    }

    public bool isDrifting()
    {
        return (status & C_DRIFTING) != 0;
    }

    public bool isBoostReady()
    {
        return (status & C_BSTREADY) != 0;
    }

    public bool isGrounded()
    {
        return (status & C_GROUNDED) != 0;
    }

    public bool isEnabled()
    {
        return (status & C_ENABLED) != 0;
    }

    public int GetBoostsReady()
    {
        return boostsReady;
    }

    public float GetBoostMeter()
    {
        return boostMeter;
    }

    public float GetDoubleBoostMeter()
    {
        return doubleBoostMeter;
    }

    public (float, float) GetBoostMeters()
    {
        return (boostMeter, doubleBoostMeter);
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    public float GetAngularVelocity()
    {
        return handlingVel + oversteerVel;
    }

    public float GetDriftAngle()
    {
        return driftAngle;
    }

    public float GetDownfoce()
    {
        return downforce;
    }

    public float GetFinalTraction()
    {
        return currentTraction * terrainModifier;
    }

    public bool isGoingForward()
    {
        return (Vector3.Project(rb.velocity, rb.rotation * Vector3.forward).normalized == (rb.rotation * Vector3.forward).normalized);
    }

    public bool isGoingInDirection(Vector3 direction)
    {
        return (Vector3.Project(rb.velocity, rb.rotation * direction).normalized == (rb.rotation * direction).normalized);
    }

    public bool isDrivingOnDirt()
    {
        return (status & C_ONDIRT) != 0;
    }

    public bool isDrivingOnIce()
    {
        return (status & C_ONICE) != 0;
    }

    public bool isDrivingOnTurbo()
    {
        return (status & C_ONTURBO) != 0;
    }

    public bool isBoosting()
    {
        return (status & C_BSTING) != 0;
    }

    public bool isDoubleBoosting()
    {
        return (status & C_DBSTING) != 0;
    }

    public bool isBoostStart()
    {
        return boostStart;
    }

    public static void SetCarsInARow(int newValue)
    {
        carsInARow = newValue;
    }

    public void SetMaxSpeedModifier(float mod)
    {
        maxSpeedModifier = mod;
    }

    public void SetAccModifier(float mod)
    {
        accModifier = mod;
    }

    public bool IsRotatingInAir()
    {
        return Vector3.Angle(currentUp, rb.rotation * Vector3.up) > 60;
    }

    /// <summary>
    /// returns the strength of the last collision, and then a timestamp for when it happened.
    /// </summary>
    /// <returns></returns>
    public (float, float) GetLastCollision()
    {
        return (lastCollisionStrength, lastCollisionTime);
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        FinishLine = GameObject.FindGameObjectWithTag("Finish");
        try
        {
            inputHandler = GetComponent<CarInputHandler>();
            carGetter = GetComponent<CarGetter>();
            //collider = GetComponent<Collider>();
        }
        catch
        {
            Debug.LogError(this.ToString() + " missing InputHandler or CarGetter");
            Debug.Break();
        }


    }

    public virtual void CustomStart()
    {
        //set up car
        car = carGetter.GetCar();
        if (car != null)
        {
            maxSpeed = car.GetMaxSpeed() * speedFactor;
            acceleration = car.GetAcceleration() * 1f * speedFactor;
            boostStrength = car.GetBoostStrength() * speedFactor;
            driftAcceleration = car.GetDriftAcceleration();
            traction = car.GetTraction();
            oversteer = car.GetOversteer();
        }
        else
        {
            Debug.Log("Using default car");
            //default settings. for now, this will be cars that I'm testing.
            maxSpeed = 300;
            handling = 2.5f;
            dHandling = 2.5f;
            acceleration = 250 / 2f;
            boostStrength = 50f;
            driftAcceleration = 50f;
            traction = 0.5f;
            oversteer = 0.5f;
        }

        boostMeter = 0;

        currentTraction = 0.2f;
        terrainModifier = 1;
        tractionModifier = 1;
        oversteerModifier = 0;
        downforce = 0;
        rawDownforce = 0;

        maxSpeedModifier = 0;
        accModifier = 0;

        lastAngularVelocity = rb.angularVelocity;
        lastVelocity = rb.velocity;
        springAcceleration = 0;
        springVelocity = 0;
        springPosition = 0;

        suspendDistance = 0.5f * transform.localScale.y;

        extraTimestamp = 0;
        currentUp = rb.rotation * Vector3.up;
        
        handlingVel = 0;
        handlingAcc = 0;
        handlingLimiter = 0;
        driftAccLimiter = 0;

        lastCollisionStrength = 0;
        lastCollisionTime = 0;

        //reset the car at the start
        BroadcastMessage("Reset");
        rb.useGravity = normalGravity;
        boostStart = false;
        status = 0x0;
        BST_running = false;

        //Time.timeScale = 0.5f;
        Debug.Log("CarControl");
        started = true;

    }

    private void FixedUpdate()
    {
        if (started)
        {
            CustomFixedUpdate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            //weaken handling as speed increases, publicaly (so that other scripts that rely on this can use it without problem)
            handling = car.GetHandling() * Mathf.LerpUnclamped(1.2f, 1f, rb.velocity.magnitude / (350 * speedFactor)) * speedFactor * 1.2f;
            dHandling = car.GetDHandling() * Mathf.LerpUnclamped(1.2f, 1f, rb.velocity.magnitude / (350 * speedFactor)) * speedFactor * 1.2f;
            
            CustomUpdate();
        }
        
    }

    protected virtual void CustomFixedUpdate()
    {
        //legacy raycasts for backup purposes

        //use raycasting as a backup for collisions.
        //Debug.DrawLine(rb.position, rb.position + (lastPosition - rb.position), Color.red, Time.deltaTime);
        //if (Physics.Raycast(new Ray(rb.position, lastPosition - rb.position), out raycastHit, (rb.position - lastPosition).magnitude))
        //{
        //    if (raycastHit.collider.isTrigger)
        //    {
        //        OnTriggerEnter(raycastHit.collider);
        //    }
        //}

        //my plain old raycast Physics.Raycast(new Ray(rb.position, rb.rotation * -Vector3.up), out raycastHit, 1f * transform.localScale.y)
        /** the box cast
         * Physics.BoxCast(
            rb.position + transform.up * 0.75f,
            new Vector3(carGetter.GetHitboxSize().x, 0.1f, carGetter.GetHitboxSize().y) * 0.4f,
            rb.rotation * Vector3.down,
            out hits,
            rb.rotation,
            (1.1f + 0.75f) * transform.localScale.y
            )
        */

        /**
         * Physics.BoxCastAll(
            rb.position + transform.up * 0.75f,
            new Vector3(carGetter.GetHitboxSize().x, 0.1f, carGetter.GetHitboxSize().y) * 0.45f,
            rb.rotation * Vector3.down, rb.rotation,
            (1.1f + 0.75f) * transform.localScale.y
            );
        */

        sideProjection = Vector3.Project(rb.velocity, rb.rotation * Vector3.right);
        frontProjection = Vector3.Project(rb.velocity, rb.rotation * Vector3.forward);

        //ground collision and orientation for hovering
        //Debug.DrawRay(rb.position, rb.rotation * -Vector3.up, Color.red, 3f);
        //raycast hit = ground hit
        if ((status & C_ENABLED) != 0)
        {

            TractionControl(Time.fixedDeltaTime);

            //fake physics to stay on the track
            RaycastHit[] hits = new RaycastHit[5];
            RaycastHit temp;
            Vector3 currentPoint; // to be used to keep this readable despite the math that's about to happen
            //it will be relative
            //center
            Physics.Raycast(
                new Ray(rb.position + transform.up * suspendDistance, -currentUp), out temp, (0.75f + suspendDistance));
            hits[0] = new RaycastHit();

            //every corner/wheel

            currentPoint = rb.rotation * new Vector3(carGetter.GetHitboxSize().x, 0, carGetter.GetHitboxSize().y);
            Physics.Raycast(
                new Ray(rb.position + transform.up * suspendDistance + currentPoint * 0.5f, Vector3.RotateTowards(-currentUp, currentPoint.normalized, 10 * Mathf.Deg2Rad, 0)), 
                out temp, (0.75f + suspendDistance));
            hits[1] = temp;

            currentPoint = rb.rotation * new Vector3(-carGetter.GetHitboxSize().x, 0, carGetter.GetHitboxSize().y);
            Physics.Raycast(
                new Ray(rb.position + transform.up * suspendDistance * transform.localScale.y + currentPoint * 0.5f, Vector3.RotateTowards(-currentUp, currentPoint.normalized, 10 * Mathf.Deg2Rad, 0)), 
                out temp, (0.75f + suspendDistance) * transform.localScale.y);
            hits[2] = temp;

            currentPoint = rb.rotation * new Vector3(-carGetter.GetHitboxSize().x, 0, -carGetter.GetHitboxSize().y);
            Physics.Raycast(
                new Ray(rb.position + transform.up * suspendDistance * transform.localScale.y + currentPoint * 0.5f, Vector3.RotateTowards(-currentUp, currentPoint.normalized, 10 * Mathf.Deg2Rad, 0)), 
                out temp, (0.75f + suspendDistance));
            hits[3] = temp;

            currentPoint = rb.rotation * new Vector3(carGetter.GetHitboxSize().x, 0, -carGetter.GetHitboxSize().y);
            Physics.Raycast(
                new Ray(rb.position + transform.up * suspendDistance + currentPoint * 0.5f, Vector3.RotateTowards(-currentUp, currentPoint.normalized, 10 * Mathf.Deg2Rad, 0)), 
                out temp, (0.75f + suspendDistance));
            hits[4] = temp;

            int groundHits = 0;
            rotationFromHit = Quaternion.identity;
            Vector3 targetPos = Vector3.zero;
            Vector3 avgContactPoint = Vector3.zero;
            int trueHits = 0;
            status &= (short)~(C_ONICE | C_ONDIRT);
            status &= (short)~C_ONTURBO;
            bool foundBoost = false;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == null || hits[i].collider.tag.Equals("Guardrail") || Vector3.Angle(rb.rotation * Vector3.up, hits[i].normal) > 60f)
                {
                    continue;
                }
                else
                {
                    groundHits++;
                }

                //check if this is the kill plane
                if (hits[i].transform.tag.Equals("KillPlane"))
                {
                    Respawn();
                    return;
                }
                if (hits[i].transform.tag.Equals("BoostPanel") && !foundBoost)
                {
                    foundBoost = true;
                }

                status = (short)(hits[i].transform.tag.Equals("SlowArea") ?
                    status | C_ONDIRT :
                    status);


                status = (short)(hits[i].transform.tag.Equals("SlipperyArea") ?
                    status | C_ONICE :
                    status);

                status = (short)(hits[i].transform.tag.Equals("FastArea") ?
                    status | C_ONTURBO :
                    status);
                //if ((status & C_ONTURBO) != 0)
                //    Debug.Log("Stopping here");
                
                

                //average this with the current average
                if (rotationFromHit == Quaternion.identity)
                {
                    rotationFromHit = Quaternion.LookRotation(Vector3.Cross(rb.rotation * Vector3.right, hits[i].normal), (hits[i].normal + rb.rotation * Vector3.up) / 2);
                }
                else
                {
                    rotationFromHit = Quaternion.Lerp(rotationFromHit,
                    Quaternion.LookRotation(Vector3.Cross(rb.rotation * Vector3.right, hits[i].normal),
                (hits[i].normal + rb.rotation * Vector3.up) / 2), 0.5f);
                }


                if (targetPos == Vector3.zero)
                {
                    targetPos = rb.position - hits[i].normal.normalized * (Vector3.Project(rb.position - hits[i].point, rb.rotation * Vector3.down).magnitude - suspendDistance);
                }
                else
                {
                    //average each one out with all the others
                    targetPos = Vector3.Lerp(targetPos,
                    rb.position - hits[i].normal.normalized * (Vector3.Project(rb.position - hits[i].point, rb.rotation * Vector3.down).magnitude - suspendDistance),
                    0.5f);
                }

                if (hits[i].distance <= suspendDistance)
                {
                    trueHits++;
                    if (avgContactPoint == Vector3.zero)
                    {
                        avgContactPoint = hits[i].point;
                    }
                    else
                    {
                        avgContactPoint = Vector3.Lerp(avgContactPoint, hits[i].point, 0.5f);
                    }
                }
                
                currentUp = rotationFromHit * Vector3.up;
            }

            if (groundHits > 0)
            {
                //track your downforce based on your distance from the ground before corrections (obsolete)
                //downforce = (-groundHit.distance / suspendDistance) * 2f + 3f;

                //attempt a double raycast
                //if (Physics.Raycast(new Ray(rb.position + rb.rotation * Vector3.forward, rb.rotation * -Vector3.up), out groundHit2, 1f))
                //{
                //    rotationFromHit = Quaternion.LookRotation(Vector3.Cross(rb.rotation * Vector3.right, groundHit2.normal),
                //        groundHit2.normal);
                //    rb.position = groundHit.point + (groundHit.normal + groundHit2.normal).normalized * (suspendDistance);
                //}
                //else
                //{

                //visual spring system;
                //springPosition = (Quaternion.Inverse(rb.rotation) * (targetPos - rb.position)).y;

                //springAcceleration = -(10f * springVelocity + 200 * springPosition);

                //springVelocity = springVelocity + springAcceleration * Time.deltaTime;

                //springPosition = Mathf.Clamp(springPosition + springVelocity * Time.deltaTime, -0.1f, 0.1f);

                rb.MovePosition(targetPos /*+ rb.rotation * Vector3.up * springPosition*/);
                //rb.MovePosition(targetPos);

                //rb.position = raycastHit.point + raycastHit.normal * suspendDistance;
                //}

                rb.MoveRotation(rotationFromHit);
                //rb.MoveRotation(rotationFromHit);

                //using the force approach
                //if (!normalGravity)
                //{
                //    rb.AddForce(rb.rotation * Vector3.down * Physics.gravity.magnitude, ForceMode.Acceleration);
                //    if (trueHits > 0)
                //    {
                //        rb.AddForceAtPosition(-Vector3.Project(rb.velocity, rb.rotation * Vector3.down), avgContactPoint, ForceMode.VelocityChange);
                //    }
                    
                //}

                //calculate downforce based on downwards velocity
                //if velocity is downwards
                if (isGoingInDirection(Vector3.down))
                {
                    rawDownforce = Mathf.Lerp(rawDownforce, Vector3.Project(rb.velocity, rb.rotation * Vector3.down).magnitude * 0.5f, 0.5f * 60 * Time.fixedDeltaTime);

                }
                else
                {
                    rawDownforce = Mathf.Lerp(rawDownforce, -Vector3.Project(rb.velocity, rb.rotation * Vector3.down).magnitude * 0.5f, 0.5f * 60 * Time.fixedDeltaTime);
                }
                downforce = normalGravity ? rawDownforce - (Physics.gravity.magnitude * Time.fixedDeltaTime) : rawDownforce;

                downforce = Mathf.Floor(Mathf.Clamp(downforce, -20, 20) * 100) / 100;
                //Debug.Log("downforce: " + downforce + "\n expected downforce: " + (Physics.gravity.magnitude * Time.fixedDeltaTime));

                //if you just landed, you get a small boost
                if ((status & C_ENABLED) != 0 && (status & C_GROUNDED) == 0 && inputHandler.GetAcceleration() > 0.01f)
                {
                    rb.velocity = Vector3.ProjectOnPlane(rb.velocity, currentUp) + rb.rotation * Vector3.forward * 50f * Mathf.Sin(Vector3.Angle(rb.velocity, rb.rotation * Vector3.forward) * Mathf.Deg2Rad);
                    //rb.velocity += rb.rotation * Vector3.forward * Vector3.Project(rb.velocity, -currentUp).magnitude * 0.5f;

                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, 400 * speedFactor);
                }
                //rb.useGravity = false;

                //if there is velocity towards the ground, cancel it out, and eliminate some of it from forward velocity?
                if (Vector3.Project(rb.velocity, rb.rotation * Vector3.down).magnitude > 0f)
                {
                    rb.velocity -= Vector3.Project(rb.velocity, rb.rotation * Vector3.down);
                }

                if (foundBoost)
                {
                    BoostPadActive();
                }

                

                status |= C_GROUNDED;
            }
            else
            {
                downforce = 0;

                if ((status & C_GROUNDED) != 0)
                    extraTimestamp = Time.time;

                status &= (short)~(C_ONICE | C_ONDIRT);
                status &= (short)~C_ONTURBO;
                status &= (short)~C_GROUNDED;
                Quaternion targetRotation;
                //rb.useGravity = true;
                if (normalGravity)
                {
                    rb.useGravity = true;
                    if (Time.time - extraTimestamp > 0.05f)
                    {
                        currentUp = Vector3.Lerp(currentUp, Vector3.up, 0.5f);
                        //if (rb.velocity != Vector3.zero && Vector3.Angle(rb.velocity, Vector3.down) > 45f)
                        //{
                        //    rb.velocity = Vector3.RotateTowards(rb.velocity,
                        //        Vector3.down,
                        //        Mathf.Deg2Rad * Mathf.Min(30, Vector3.Angle(rb.velocity, Vector3.down) - 45f) * Time.fixedDeltaTime, 0);
                        //}

                        if (Physics.Raycast(new Ray(rb.position, -transform.up), out raycastHit, 50f * transform.localScale.y) && !raycastHit.collider.CompareTag("KillPlane") && !raycastHit.collider.CompareTag("Guardrail"))
                        {
                            //rb.MoveRotation(Quaternion.Lerp(rb.rotation, Quaternion.LookRotation(
                            //Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, raycastHit.normal), raycastHit.normal),
                            //120 / raycastHit.distance * Time.fixedDeltaTime));

                            float angleDown = Mathf.Clamp(Vector3.Angle(rb.velocity, currentUp) - 90 , -45, 45) - (inputHandler.GetBrakes() > 0 ? 30 * inputHandler.GetBrakes() : 0);

                            //this is the rotation if the car was not angled in any way
                            targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, currentUp), currentUp);

                            float horizontalAngle = Vector3.Angle(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, currentUp), Vector3.ProjectOnPlane(rb.velocity, currentUp));

                            Vector3 newForward = Vector3.RotateTowards(targetRotation * Vector3.forward, -currentUp, angleDown * Mathf.Deg2Rad * Mathf.Cos(horizontalAngle * Mathf.Deg2Rad), 0);

                            Vector3 newUp = Vector3.RotateTowards(currentUp, Vector3.Project(rb.velocity, targetRotation * Vector3.right), angleDown * Mathf.Deg2Rad * Mathf.Sin(horizontalAngle * Mathf.Deg2Rad), 0);
                            targetRotation = Quaternion.LookRotation(newForward, newUp);

                            rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation,
                            /*120 / raycastHit.distance*/ 0.05f * 60 * Time.fixedDeltaTime));
                        }
                        else
                        {
                            //rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(
                            //    Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, Vector3.up), Vector3.up),
                            //    60 * 5 * Time.fixedDeltaTime));

                            float angleDown = Mathf.Clamp(Vector3.Angle(rb.velocity, currentUp) - 90 , -45, 45) - (inputHandler.GetBrakes() > 0 ? 30 * inputHandler.GetBrakes() : 0);

                            //this is the rotation if the car was not angled in any way
                            targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, currentUp), currentUp);

                            float horizontalAngle = Vector3.Angle(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, currentUp), Vector3.ProjectOnPlane(rb.velocity, currentUp));

                            Vector3 newForward = Vector3.RotateTowards(targetRotation * Vector3.forward, -currentUp, angleDown * Mathf.Deg2Rad * Mathf.Cos(horizontalAngle * Mathf.Deg2Rad), 0);

                            Vector3 newUp = Vector3.RotateTowards(currentUp, Vector3.Project(rb.velocity, targetRotation * Vector3.right), angleDown * Mathf.Deg2Rad * Mathf.Sin(horizontalAngle * Mathf.Deg2Rad), 0);
                            targetRotation = Quaternion.LookRotation(newForward, newUp);

                            rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation, 0.05f * 60 * Time.fixedDeltaTime));
                            //rb.MoveRotation(targetRotation);
                        }
                    }
                    
                }
                else
                {
                    if (Time.time - extraTimestamp > 0.1f)
                    {
                        if (Physics.Raycast(new Ray(rb.position, -currentUp), out raycastHit, 25f * transform.localScale.y / Mathf.Sin(Mathf.Deg2Rad * 30)) && !raycastHit.collider.CompareTag("KillPlane") && !raycastHit.collider.CompareTag("Guardrail"))
                        {
                            currentUp = raycastHit.normal;
                            //if (rb.velocity != Vector3.zero && Vector3.Angle(rb.velocity, rb.rotation * Vector3.down) > 45f)
                            //{
                            //    rb.velocity = Vector3.RotateTowards(rb.velocity,
                            //        rb.rotation * Vector3.down,
                            //        Mathf.Deg2Rad * Mathf.Min(30, Vector3.Angle(rb.velocity, Vector3.down) - 45f) * Time.fixedDeltaTime, 0);
                            //}
                            //rb.velocity += rb.rotation * Vector3.down * 90 * Time.fixedDeltaTime;
                            rb.AddForce(currentUp * -Physics.gravity.magnitude * rb.mass * speedFactor);

                            //rb.MoveRotation(Quaternion.Lerp(rb.rotation, Quaternion.LookRotation(
                            //Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, raycastHit.normal), raycastHit.normal),
                            //120 / raycastHit.distance * Time.fixedDeltaTime));
                            float angleDown = Mathf.Clamp(Vector3.Angle(rb.velocity, currentUp) - 90 , -45, 45) - (inputHandler.GetBrakes() > 0 ? 30 * inputHandler.GetBrakes() : 0);

                            //this is the rotation if the car was not angled in any way
                            targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, currentUp), currentUp);

                            float horizontalAngle = Vector3.Angle(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, currentUp), Vector3.ProjectOnPlane(rb.velocity, currentUp));

                            Vector3 newForward = Vector3.RotateTowards(targetRotation * Vector3.forward, -currentUp, angleDown * Mathf.Deg2Rad * Mathf.Cos(horizontalAngle * Mathf.Deg2Rad), 0);

                            Vector3 newUp = Vector3.RotateTowards(currentUp, Vector3.Project(rb.velocity, targetRotation * Vector3.right), angleDown * Mathf.Deg2Rad * Mathf.Sin(horizontalAngle * Mathf.Deg2Rad), 0);
                            targetRotation = Quaternion.LookRotation(newForward, newUp);

                            rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation,
                            /*120 / raycastHit.distance*/ 0.05f * 60 * Time.fixedDeltaTime));
                        }
                        else
                        {
                            currentUp = Vector3.up;
                            if (rb.velocity != Vector3.zero && Vector3.Angle(rb.velocity, Vector3.down) > 45f)
                            {
                                rb.velocity = Vector3.RotateTowards(rb.velocity,
                                    Vector3.down,
                                    Mathf.Deg2Rad * Mathf.Min(30, Vector3.Angle(rb.velocity, Vector3.down) - 45f) * Time.fixedDeltaTime, 0);
                            }
                            //rb.velocity += Vector3.down * 90 * Time.fixedDeltaTime;
                            rb.AddForce(currentUp * -Physics.gravity.magnitude * rb.mass * speedFactor);

                            //rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, Vector3.up), Vector3.up), 60 * 5 * Time.fixedDeltaTime));

                            
                            float angleDown = Mathf.Clamp(Vector3.Angle(rb.velocity, currentUp) - 90 , -45, 45)-(inputHandler.GetBrakes() > 0 ? 30 * inputHandler.GetBrakes() : 0);

                            //this is the rotation if the car was not angled in any way
                            targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, currentUp), currentUp);

                            float horizontalAngle = Vector3.Angle(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, currentUp), Vector3.ProjectOnPlane(rb.velocity, currentUp));

                            Vector3 newForward = Vector3.RotateTowards(targetRotation * Vector3.forward, -currentUp, angleDown * Mathf.Deg2Rad * Mathf.Cos(horizontalAngle * Mathf.Deg2Rad), 0);

                            Vector3 newUp = Vector3.RotateTowards(currentUp, Vector3.Project(rb.velocity, targetRotation * Vector3.right), angleDown * Mathf.Deg2Rad * Mathf.Sin(horizontalAngle * Mathf.Deg2Rad), 0);
                            targetRotation = Quaternion.LookRotation(newForward, newUp);

                            //targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, currentUp), currentUp);


                            //float angleDown = Mathf.Clamp(Vector3.Angle(rb.velocity, currentUp) - 90, -45, 45);
                            //if (Vector3.Angle(rb.rotation * Vector3.up, rb.velocity) < 179 && Vector3.Angle(rb.rotation * Vector3.up, rb.velocity) > 1 && false)
                            //{
                            //    //Debug.DrawRay(rb.position + transform.up * 0.5f, Vector3.Cross(rb.velocity, rb.rotation * Vector3.up), Color.magenta, 4f);
                            //    targetRotation =  Quaternion.AngleAxis(-angleDown, Vector3.Cross(rb.velocity, rb.rotation * Vector3.up)) * targetRotation;
                            //}
                            //else
                            //{
                            //    targetRotation = Quaternion.AngleAxis(angleDown, rb.rotation * Vector3.right) * targetRotation;
                            //}


                            rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation, 0.05f * 60 * Time.fixedDeltaTime));
                            //rb.MoveRotation(targetRotation);
                        }
                    }
                }

            }
            return;
        }

        //when disabled
        if (Physics.Raycast(new Ray(rb.position, rb.rotation * Vector3.down), out raycastHit, 50f * transform.localScale.y))
        {
            rotationFromHit = Quaternion.LookRotation(Vector3.Cross(rb.rotation * Vector3.right, raycastHit.normal),
                (raycastHit.normal + rb.rotation * Vector3.up) / 2);

            //Vector3 targetPos = rb.position - raycastHit.normal.normalized * (Vector3.Project(rb.position - raycastHit.point, rb.rotation * Vector3.down).magnitude - suspendDistance);

            //rb.position = targetPos;

            rb.position = raycastHit.point + raycastHit.normal * suspendDistance;

            rb.rotation = Quaternion.Lerp(rb.rotation, rotationFromHit, 0.75f);

            //calculate downforce based on downwards velocity
            //if velocity is downwards

            //if there is velocity towards the ground, cancel it out, and eliminate some of it from forward velocity?
            if (Vector3.Project(rb.velocity, rb.rotation * Vector3.down).magnitude > 0f)
            {
                //rb.velocity += rb.rotation * Vector3.forward * Vector3.Project(rb.velocity, rb.rotation * Vector3.down).magnitude * 0.1f;
                rb.velocity -= Vector3.Project(rb.velocity, rb.rotation * Vector3.down);
            }
        }
    }

    protected virtual void CustomUpdate()
    {
        WaitForStart();
        //Debug.Log("status: " + System.Convert.ToString(status, 2));
        //Debug.Log("turbo: " + System.Convert.ToString(C_ONTURBO, 2));

        

        //terrain modifiers
        if (isDrivingOnIce())
        {
            rawTerrainModifier = 0.5f;
        }
        else if (isDrivingOnDirt())
        {
            rawTerrainModifier = 0.75f;
        }
        else
            rawTerrainModifier = 1;
        rawTractionModifier = 1;
        rawOversteerModifier = 0;

        driftAngle = Vector3.Angle(Vector3.ProjectOnPlane(rb.velocity, rb.rotation * Vector3.up), rb.rotation * Vector3.forward);

        

        if (isEnabled())
        {
            //respawning
            if (inputHandler.GetResetDown())
            {
                Respawn();
            }
            if (isGrounded())
            {

                //max speed
                //rb.velocity = Vector3.ClampMagnitude(rb.velocity, 600);
                rawOversteerModifier = oversteer * inputHandler.GetAcceleration();
                //accelerating
                if (inputHandler.GetAcceleration() > 0)
                {
                    Accelerate();
                }

                //brakes
                if ((inputHandler.GetBrakes() > 0 || (inputHandler.GetEBrakes() > 0)) && (status & C_GROUNDED) != 0)
                {
                    if ((inputHandler.GetBrakes() > 0) && (status & C_GROUNDED) != 0)
                    {
                        status |= (short)C_BRAKING;
                        rawOversteerModifier = Mathf.Clamp(inputHandler.GetBrakes() > 0.9f ? oversteer + 0.2f : oversteer * inputHandler.GetAcceleration(), 0, 0.95f);

                    }
                    else
                    {
                        status &= (short)~C_BRAKING;
                    }

                    //e brakes
                    if ((inputHandler.GetEBrakes() > 0) && (status & C_GROUNDED) != 0)
                    {
                        status |= (short)C_SLIDING;
                        rawTractionModifier *= inputHandler.GetEBrakes() > 0.9f ? 0.1f : 0.9f;
                        rawOversteerModifier = Mathf.Clamp(inputHandler.GetEBrakes() > 0.9f ? oversteer + 0.4f : oversteer * inputHandler.GetAcceleration(), 0, 0.95f);

                    }
                    else
                    {
                        status &= (short)~C_SLIDING;
                    }
                    Brake();
                }
                else
                {
                    status &= (short)~C_BRAKING;
                    status &= (short)~C_SLIDING;
                }

                
                if (inputHandler.GetBrakes() <= 0)
                {
                    //apply drag
                    if (inputHandler.GetAcceleration() <= 0 || (rb.velocity.magnitude > maxSpeed && !isBoosting()))
                    {
                        ApplyDrag();
                    }

                    //apply engine brakes
                    if (frontProjection.magnitude > 0.1f && inputHandler.GetAcceleration() <= 0)
                    {
                        rawOversteerModifier = oversteer * 0.25f;
                        ApplyEngineBrakes();
                    }
                }
            }

            ////ebrakes
            //if (inputHandler.GetEBrakes())
            //{
            //    status |= (short)C_SLIDING;
            //    rawTractionModifier *= 0.2f;

            //    ApplyEBrakes();
            //}
            //else
            //{
            //    status &= (short)~C_SLIDING;
            //}

            CalculateHandling();
        }

        //drift condition raw
        //if (!drifting && sideProjection.magnitude >= traction * 50)
        //    drifting = true;
        //else if (drifting && sideProjection.magnitude <= traction * 50 * (1 - traction))
        //    drifting = false;

        //drift conditions with downforce(for 1.7 - 2.3)
        //if (!drifting && sideProjection.magnitude >= 50 * traction * downforce)
        //    drifting = true;
        //else if (drifting && sideProjection.magnitude <= 55 * traction * downforce)
        //    drifting = false;


        CalculateBoost();

        if (isBoosting())
        {
            rawTractionModifier *= 1.2f;
        }

        //if (inputHandler.GetSteering() * ((Quaternion.Inverse(rb.rotation) * rb.velocity).x) > 0)
        //{
        //    //Debug.DrawRay(rb.position + rb.transform.up, rb.transform.up, Color.yellow, Time.deltaTime);
        //    currentTraction *= 1 - 0.75f * Mathf.Abs(inputHandler.GetSteering());
        //}

        terrainModifier = Mathf.Abs(rawTerrainModifier - 1) > 0.1f ? 
            rawTerrainModifier : 
            Mathf.MoveTowards(terrainModifier, rawTerrainModifier, 4f * Time.deltaTime);

        tractionModifier = rawTractionModifier < tractionModifier ?
            rawTractionModifier :
            Mathf.MoveTowards(tractionModifier, rawTractionModifier, 4f * Time.deltaTime);
        oversteerModifier = rawOversteerModifier > oversteerModifier ?
            rawOversteerModifier : 
            Mathf.MoveTowards(oversteerModifier, rawOversteerModifier, 6f * Time.deltaTime);
        //Debug.Log($"terrainModifier = {terrainModifier}");

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, 600 * speedFactor);
        lastAngularVelocity = rb.angularVelocity;
        lastVelocity = rb.velocity;
        elapsedSinceBoost += Time.deltaTime;
        elapsedSinceQuickBoost += Time.deltaTime;
    }


    //collision exclusive information
    ContactPoint lastContact = new ContactPoint();
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag.Equals("KillPlane"))
        {
            Respawn();
            return;
        }

        if (collision.transform.tag.Equals("Player"))
        {
            rb.velocity += collision.GetContact(0).normal.normalized * 10f;
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, lastAngularVelocity, 0.5f);
        }
        else
        {
            //restore car if the collision is vertical
            if (Vector3.Angle(transform.up, collision.GetContact(0).normal) < 30f)
            {
                rb.position += collision.GetContact(0).normal.normalized * 0.15f;
                rb.velocity = rb.velocity.normalized * lastVelocity.magnitude;
                rb.angularVelocity = lastAngularVelocity;

                //show me that it works :)
                //Debug.DrawRay(collision.GetContact(0).point, collision.GetContact(0).normal, Color.red, 10f);
            }
            else
            {
                //track the collision

                //look at the last velocity only in the direction of the collision, and then reorient it to the car rotation, and then get the x value of that
                lastCollisionStrength = (Quaternion.Inverse(rb.rotation) * Vector3.Project(lastVelocity, -collision.GetContact(0).normal)).x;


                Vector3 targetVector = Vector3.Project(rb.rotation * Vector3.forward, Vector3.Cross(collision.GetContact(0).normal, rb.rotation * Vector3.up));
                Quaternion forwardFromNormal = Quaternion.LookRotation(targetVector, rb.rotation * Vector3.up);

                if (Vector3.Angle(lastVelocity, rb.velocity) > 10f)
                {
                    //rb.velocity = Vector3.RotateTowards(lastVelocity, Vector3.Project(rb.velocity, forwardFromNormal * Vector3.forward), 10f, 300f);
                }
                rb.position += rb.rotation * Vector3.up * 0.1f /*+ collision.GetContact(0).normal.normalized * 0.1f*/;

                if (boostMeter >= 0.1f)
                {
                    
                    if (Vector3.Angle(rb.transform.forward, collision.GetContact(0).normal) < 60 && Time.time - lastCollisionTime > 0.25f && Mathf.Abs(lastCollisionStrength) / 300 < boostMeter)
                    {
                        rb.velocity += rb.transform.forward * lastCollisionStrength * 0.5f;
                    }
                    else
                    {
                        rb.velocity -= rb.velocity.normalized * Vector3.Project(lastVelocity, -collision.GetContact(0).normal).magnitude * 0.2f;
                    }
                    
                    boostMeter -= Mathf.Abs(lastCollisionStrength) / 600;
                    doubleBoostMeter = Mathf.Clamp01(doubleBoostMeter - Mathf.Abs(lastCollisionStrength) / 450);
                }
                else
                {
                    boostMeter = 0;
                    doubleBoostMeter = Mathf.Clamp01(doubleBoostMeter - Mathf.Abs(lastCollisionStrength) / 450);
                    rb.velocity -= rb.velocity.normalized * Vector3.Project(lastVelocity, -collision.GetContact(0).normal).magnitude * 0.2f;
                }

                //rb.velocity += collision.GetContact(0).normal.normalized * 5f;
                //status &= C_DRIFTING;

                //smooth out collision
                //if (transform.InverseTransformVector(collision.GetContact(0).normal).z < 0 && transform.InverseTransformVector(collision.GetContact(0).normal).z > -0.9f)
                //{
                //    rb.rotation = Quaternion.RotateTowards(rb.rotation, forwardFromNormal, 10f);
                //}

                if (Vector3.Angle(-collision.GetContact(0).normal, rb.rotation * Vector3.forward) < 90f)
                {
                    rb.velocity += collision.GetContact(0).normal * 0.5f;
                    rb.rotation = Quaternion.RotateTowards(rb.rotation, forwardFromNormal, 4f);
                }



                //rb.angularVelocity = Vector3.MoveTowards(lastAngularVelocity, rb.angularVelocity, currentTraction * 10f);
                rb.angularVelocity = Vector3.zero;
            }

            //archived methods
            

            //rb.velocity = rb.velocity.normalized * Mathf.Lerp(rb.velocity.magnitude, lastVelocity.magnitude, 0.5f);


            //rb.velocity += collision.GetContact(0).normal.normalized * 10f;

            //calculate the strength
            //Vector3 finalTorqueApplied = Vector3.Cross(lastRotation * Vector3.forward, forwardFromNormal * Vector3.forward) * 1f;
            //determining the direction of the rotation.
            //if (inputHandler.GetSteering() > inputHandler.GetDeadzone())
            //{
            //    finalTorqueApplied *= 0.1f;
            //}

        }
        //drifting = true;
        lastContact = collision.GetContact(0);
        lastCollisionTime = Time.time;
    }

    
    private void OnCollisionStay(Collision collision)
    {
        //OnCollisionEnter(collision);

        if (collision.transform.tag.Equals("Player"))
        {
            rb.velocity += collision.GetContact(0).normal.normalized * 10f * Time.deltaTime;
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, lastAngularVelocity, 0.5f * 60 * Time.deltaTime);
        }
        else
        {
            if (transform.InverseTransformVector(collision.GetContact(0).normal).y > 0.5f)
            {
                //rb.position += collision.GetContact(0).normal.normalized * 0.5f;
                rb.velocity = rb.velocity.normalized * lastVelocity.magnitude;
                //rb.angularVelocity = lastAngularVelocity;

                //show me that it works :)
                //Debug.DrawRay(collision.GetContact(0).point, collision.GetContact(0).normal, Color.red, 10f);
            }
            else
            {

                //if (Vector3.Angle(lastVelocity, rb.velocity) > 10f)
                //{
                //    rb.velocity -= Vector3.Project(rb.velocity, collision.GetContact(0).normal) * 60 * Time.deltaTime;
                //}
                Vector3 targetVector = Vector3.Project(rb.rotation * Vector3.forward, Vector3.Cross(collision.GetContact(0).normal, rb.rotation * Vector3.up));
                Quaternion forwardFromNormal = Quaternion.LookRotation(targetVector, rb.rotation * Vector3.up);

                //rb.velocity += collision.GetContact(0).normal.normalized * 1f * 60 * Time.deltaTime;
                rb.velocity -= rb.velocity.normalized * Vector3.Project(lastVelocity, -collision.GetContact(0).normal).magnitude * 0.2f * 60 * Time.deltaTime;

                //smooth out collision
                //if (transform.InverseTransformVector(collision.GetContact(0).normal).z < 0 && transform.InverseTransformVector(collision.GetContact(0).normal).z > -0.9f)
                //{
                //    rb.rotation = Quaternion.RotateTowards(rb.rotation, forwardFromNormal, 10f * 60 * Time.deltaTime);
                //}

                if (Vector3.Angle(-collision.GetContact(0).normal, rb.rotation * Vector3.forward) < 90)
                {
                    //rb.velocity += collision.GetContact(0).normal * 20 * 60 * Time.deltaTime;
                    rb.rotation = Quaternion.RotateTowards(rb.rotation, forwardFromNormal, 2f * 60 * Time.deltaTime);
                }
            }

            
        }

        //rb.angularVelocity = Vector3.MoveTowards(lastAngularVelocity, rb.angularVelocity, currentTraction * 10f * Time.deltaTime);
        lastContact = collision.GetContact(0);
    }

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (!collision.transform.tag.Equals("Player"))
    //    {
    //        rb.velocity = Vector3.ClampMagnitude(Vector3.Project(rb.velocity, lastContact.normal), 5f) + Vector3.ProjectOnPlane(rb.velocity, lastContact.normal);
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag.Equals("BoostPanel"))
        {
            BoostPadActive();
        }
        if (other.transform.tag.Equals("JumpPad"))
        {
            rb.velocity = (other.transform.rotation * Vector3.up * other.GetComponent<JumpStrength>().jumpStrength * rb.velocity.magnitude / 150) + Vector3.ProjectOnPlane(rb.velocity, other.transform.rotation * Vector3.up);
            rb.position += other.transform.rotation * Vector3.up * 1;
            status &= (byte)~C_GROUNDED;
            extraTimestamp = Time.time + 0.3f;
        }

        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.tag.Equals("BoostPanel"))
        {
            BoostPadActive();
        }
    }

    private void BoostPadActive()
    {
        if (BoostRoutine != null)
            StopCoroutine(BoostRoutine);

        BoostRoutine = StartCoroutine(Boost(boostStrength, 0.5f));
    }

    private void WaitForStart()
    {
        if ((status & C_ENABLED) == 0 && Time.time - countdownTimestamp >= 1.5f)
        {
            BoostRoutine = StartCoroutine(Boost(10f * speedFactor, 0.5f));

            status |= C_ENABLED;
        }
    }

    private void Accelerate()
    {
        //max speed
        //rb.velocity = Vector3.ClampMagnitude(rb.velocity, 600);

        //accelerating
        

        if (maxSpeedUsed)
        {


            if (isDrivingOnDirt())
            {
                //if (rb.velocity.magnitude < maxSpeed * 0.75f)
                    rb.velocity += rb.rotation * Vector3.forward * ((maxSpeed * 0.75f + maxSpeedModifier) - rb.velocity.magnitude) / (maxSpeed * 0.75f + maxSpeedModifier) * (acceleration + accModifier) * inputHandler.GetAcceleration() * Time.deltaTime;
            }
            //else if (isDrivingOnTurbo())
            //{
            //    //if (rb.velocity.magnitude < maxSpeed + 100f)
            //        rb.velocity += rb.rotation * Vector3.forward * Mathf.Max(((maxSpeed + maxSpeedModifier/* + 100f*/) - rb.velocity.magnitude) / (maxSpeed + maxSpeedModifier/* + 100f*/), 40) * (acceleration + accModifier) * inputHandler.GetAcceleration() * Time.deltaTime;
            //}
            else /*if (rb.velocity.magnitude < maxSpeed)*/
            {
                if (isDrivingOnIce())
                {
                    rb.velocity += rb.rotation * Vector3.forward * Mathf.Max((maxSpeed + maxSpeedModifier - rb.velocity.magnitude) / (maxSpeed + maxSpeedModifier) * (acceleration + accModifier), -5f) * 0.5f * inputHandler.GetAcceleration() * Time.deltaTime;
                }

                else
                {
                    rb.velocity += rb.rotation * Vector3.forward * Mathf.Max((maxSpeed + maxSpeedModifier - rb.velocity.magnitude) / (maxSpeed + maxSpeedModifier) * (acceleration + accModifier), -5f) * inputHandler.GetAcceleration() * Time.deltaTime;
                }
            }

        }
        else
        {
            rb.AddRelativeForce(Vector3.forward * rb.mass * acceleration * rawTerrainModifier * inputHandler.GetAcceleration() * 60 * Time.deltaTime);
        }


        //rb.AddRelativeForce(Vector3.forward * rb.mass * 2200f * Time.deltaTime);
    }

    private void Brake()
    {
        //rb.AddRelativeForce(-Vector3.forward * rb.mass * 10);
        Vector3 velocity = Vector3.Project(rb.velocity, rb.rotation * Vector3.forward);
        //if they projection normalized is the same as the car's direction
        if (isBraking())
        {
            if (Vector3.Angle(velocity.normalized, (rb.rotation * Vector3.forward).normalized) < 30f)
            {
                float brakingStrength = car.GetBrakes() * 0.75f;
                rb.velocity += rb.velocity.normalized * -brakingStrength * speedFactor * inputHandler.GetBrakes() * Time.deltaTime;
            }
            else
            {
                rb.velocity += rb.rotation * Vector3.forward * (maxSpeed * 0.75f - rb.velocity.magnitude) / (maxSpeed * 0.75f) * -0.9f * (acceleration) * 1.2f * inputHandler.GetBrakes() * Time.deltaTime;
            }
        }
        if (isSliding())
        {
            float brakingStrength = car.GetBrakes() * 0.5f;
            rb.velocity += rb.velocity.normalized * -brakingStrength * speedFactor * inputHandler.GetEBrakes() * Time.deltaTime;
        }
    }

    private void ApplyDrag()
    {
        float modifiedAcc = Mathf.Lerp(125, 175, (acceleration * 2 / speedFactor) / 250) * speedFactor;
        if ((C_ONDIRT & status) != 0)
        {
            rb.AddForce(rb.velocity * rb.mass * -6f * Time.deltaTime);
        }
        else if ((C_ONDIRT & status) == 0)
        {
            rb.AddForce(rb.velocity * rb.mass * -0.5f * Time.deltaTime);
        }
        
    }

    private void ApplyEngineBrakes()
    {
        if (isGoingForward())
            rb.AddForce(transform.rotation * Vector3.forward * rb.mass * speedFactor * -720 * Time.deltaTime);
        else if (frontProjection.normalized != (rb.rotation * Vector3.forward).normalized)
            rb.AddForce(transform.rotation * Vector3.forward * rb.mass * speedFactor * 720 * Time.deltaTime);
    }

    //private void ApplyEBrakes()
    //{
    //    if (isGrounded())
    //    {
    //        if (rb.velocity.magnitude > 1)
    //        {
    //            float brakingStrength = (3f * acceleration + 700 * currentTraction * 60);
    //            rb.AddForce(rb.velocity.normalized * rb.mass * -brakingStrength * Time.deltaTime);
    //        }
    //        else
    //        {
    //            rb.velocity = Vector3.zero;
    //        }
    //    }
    //}

    private void CalculateHandling()
    {
        float currentHandling = 0;
        float targetVelocity;
        float responceModifier = 1;

        //steering (either direction)

        //exponential but better
        
        if (isSliding())
        {
            handlingLimiter = Mathf.Clamp01(Mathf.Max(handlingLimiter, (driftAngle - fullDriftAngle) / fullDriftAngle));
            //targetVelocity = (handling - (handling - dHandling) * Mathf.Clamp01(driftAngle * 3 / fullDriftAngle)) * inputHandler.GetSteering();
            currentHandling = handling;
            //lerp = (Quaternion.Inverse(rb.rotation) * rb.angularVelocity).y + 6f * iceModifier *
            //    ((currentHandling * inputHandler.GetSteering()) - (Quaternion.Inverse(rb.rotation) * rb.angularVelocity).y) * Time.deltaTime;

            //if drifting in the opposite direction of steering, simply turn harder.
            if (inputHandler.GetSteering() * (Quaternion.Inverse(rb.rotation) * rb.velocity).x > 0)
            {
                currentHandling += 1f * (driftAngle / 90);
                rawTractionModifier *= 1 - (0.15f * Mathf.Abs(inputHandler.GetSteering()));
                //responceModifier *= 1f;
            }
            else if (inputHandler.GetSteering() * (Quaternion.Inverse(rb.rotation) * rb.velocity).x < 0)
            {
                rawTractionModifier *= 1 + (0.15f * Mathf.Abs(inputHandler.GetSteering()));
            }
        }
        else
        {
            handlingLimiter = 0;
            //targetVelocity = handling * inputHandler.GetSteering();
            currentHandling = handling;
        }

        if (isSliding())
        {
            //targetVelocity = (dHandling + 0.75f) * inputHandler.GetSteering();
            currentHandling += 0.25f * inputHandler.GetBrakes();
            //lerp = (Quaternion.Inverse(rb.rotation) * rb.angularVelocity).y + 8f * iceModifier * (((dHandling + 0.5f) * inputHandler.GetSteering()) - (Quaternion.Inverse(rb.rotation) * rb.angularVelocity).y) * Time.deltaTime;
        }
        if (inputHandler.GetAcceleration() < 0.1f)
        {
            currentHandling += 0.12f;
        }

        //6 * icemodifier * (targetVelocity - angularVelocity)

        if (!isDrifting())
            targetVelocity = currentHandling * inputHandler.GetSteering();
        else
        {
            float conservedV = Mathf.Clamp(handlingVel, -6, 6) * oversteerModifier;
            if (inputHandler.GetSteering() >= 0)
            {
                targetVelocity = conservedV + currentHandling * inputHandler.GetSteering() * Mathf.Clamp(-Mathf.Pow(2.71828f, (conservedV - currentHandling * inputHandler.GetSteering())) * 0.9f + 1, 0.1f, 1);
            }
            else
            {
                targetVelocity = conservedV + currentHandling * inputHandler.GetSteering() * Mathf.Clamp(-Mathf.Pow(2.71828f, -(conservedV - currentHandling * inputHandler.GetSteering())) * 0.9f + 1, 0.1f, 1);
            }
        }


        //linear acceleration
        //if (Time.deltaTime == 0)
        //{
        //    handlingAcc = 0;
        //}
        //else
        //{
        //    float responsiveness = 7 * responceModifier;

        //    //return to 0 steering faster than normal
        //    if ((targetVelocity - handlingVel) == 0 ||
        //    Mathf.Abs(inputHandler.GetSteering()) <= 0.01f)
        //        responsiveness *= 1.2f;

        //    handlingAcc = Mathf.Abs(targetVelocity - handlingVel) / Time.deltaTime < responsiveness ?
        //    (targetVelocity - handlingVel) / Mathf.Max(Time.deltaTime, 0.00000001f) :
        //    responsiveness * (targetVelocity - handlingVel) / Mathf.Abs(targetVelocity - handlingVel);
        //}



        //overdamped system handling
        //handlingAcc = handlingAcc + (-10f * handlingAcc - 54f * responceModifier * (handlingVel - targetVelocity)) * Time.deltaTime;
        //exponential handling
        //handlingAcc = 6 * responceModifier * (targetVelocity - angularVelocity);
        //handlingAcc = 6 * responceModifier * (targetVelocity - handlingVel);

        //handlingAcceleration = Mathf.Clamp(handlingAcceleration, -15, 15);
        //handlingVel = (Quaternion.Inverse(rb.rotation) * rb.angularVelocity).y + handlingAcc * Time.deltaTime;

        //exponential handling + linear minimum
        if (Time.deltaTime == 0 || (targetVelocity - handlingVel) == 0)
        {
            handlingAcc = 0;
        }
        else
        {
            float minimumResponsiveness;
            
            float exponential;

            if (isSliding())
            {
                minimumResponsiveness = 2.5f;
                exponential = 3.25f * responceModifier * (targetVelocity - handlingVel);
            }
            else if ((Mathf.Abs(targetVelocity) > Mathf.Abs(handlingVel) && handlingVel * targetVelocity > 0) || Mathf.Abs(handlingVel) < 0.01f)
            {
                minimumResponsiveness = 1.25f;
                exponential = 3.25f * responceModifier * (targetVelocity - handlingVel);
            }
            else
            {
                minimumResponsiveness = 1.5f;
                exponential = 3.25f * responceModifier * (targetVelocity - handlingVel);
            }
            float linear = minimumResponsiveness * responceModifier * (targetVelocity - handlingVel) / Mathf.Abs(targetVelocity - handlingVel);

            if (!isGrounded())
            {
                linear *= 0.5f;
                exponential *= 0.5f;
                minimumResponsiveness *= 0.5f;
            }

            if (Mathf.Abs(targetVelocity - handlingVel) / Time.deltaTime < minimumResponsiveness)
            {
                handlingAcc = (targetVelocity - handlingVel) / Mathf.Max(Time.deltaTime, 0.00000001f);
            }
            else if (Mathf.Abs(linear) > Mathf.Abs(exponential))
            {
                handlingAcc = Mathf.Lerp(handlingAcc, linear, 0.66f * 60 * Time.deltaTime);
            }
            else
            {
                handlingAcc = Mathf.Lerp(handlingAcc, exponential, 0.66f * 60 * Time.deltaTime);
            }

        }


        //handlingVel = handlingVel + handlingAcc * Time.deltaTime; //accounts for delta time
        //handlingVel = handlingVel + handlingAcc; //delta time accounted on handlingAcc's discretion

        //don't lower the angular speed if hardpressing
        if ((inputHandler.GetSteering() < 0 && handlingVel >= targetVelocity) ||
            (inputHandler.GetSteering() > 0 && handlingVel <= targetVelocity) ||
            Mathf.Abs(inputHandler.GetSteering()) < 0.9f)
        {
            handlingVel = handlingVel + handlingAcc * Time.deltaTime;
        }
        else if (Mathf.Abs(inputHandler.GetSteering()) >= 0.9f)
        {
            //lower it, but very slowly
            handlingVel = handlingVel + handlingAcc * 0.25f * Time.deltaTime;
        }

        if (Mathf.Abs(handlingVel) < 0.001f)
            handlingVel = 0;
        //if (Mathf.Abs((Quaternion.Inverse(rb.rotation) * rb.angularVelocity).y) <= 0.001f)
        //    rb.angularVelocity = rb.rotation * new Vector3(0, 0);


        //oversteer proportional to drift angle

        //raw oversteer
        oversteerVel = 0;
        if ((Quaternion.Inverse(rb.rotation) * rb.velocity).x < 0 && driftAngle < 150)
        {
            if (driftAngle < 90)
                oversteerVel = driftAngle * Mathf.Deg2Rad;
            else
                oversteerVel = (-3 * driftAngle + 360) * Mathf.Deg2Rad;
        }
        else if ((Quaternion.Inverse(rb.rotation) * rb.velocity).x > 0 && driftAngle < 150)
        {
            if (driftAngle < 90)
                oversteerVel = -driftAngle * Mathf.Deg2Rad;
            else
                oversteerVel = -(-3 * driftAngle + 360) * Mathf.Deg2Rad;
        }

        //how much gets used
        if (inputHandler.GetBrakes() > 0.9f)
        {
            oversteerVel *= 0.25f;
        }
        else
        {
            oversteerVel *= isDrifting() ? 0.125f : 0f;
        }



        //Debug.Log(oversteerVel);

        //basic rotation
        //rb.angularVelocity = rb.rotation * new Vector3(0, handlingVel);
        rb.angularVelocity = Vector3.zero;

        //rotate around the center of gravity if not replaying
        //oversteer separately from normal handling

        Vector3 frontCenter = rb.position + (carGetter.GetHitboxSize().y * 0.75f) * transform.forward;
        Vector3 backCenter = rb.position - (carGetter.GetHitboxSize().y * 0.75f) * transform.forward;

        try
        {
            //handling
            Quaternion q = Quaternion.AngleAxis(handlingVel * Mathf.Rad2Deg * Time.deltaTime, rb.rotation * Vector3.up);
            Vector3 frontNewPosition = q * (frontCenter - backCenter) + backCenter;
            Vector3 backNewPosition = backCenter + rb.velocity * 0.05f * oversteerVel * Time.deltaTime;
            //rb.MovePosition(q * (rb.position - backCenter) + backCenter);
            rb.MovePosition((frontNewPosition + backNewPosition) * 0.5f);
            rb.MoveRotation(Quaternion.LookRotation(frontNewPosition - backNewPosition, transform.up));

            //oversteer
            //q = Quaternion.AngleAxis(oversteerVel * Mathf.Rad2Deg * Time.deltaTime, rb.rotation * Vector3.up);
            //rb.MovePosition(q * (rb.position - frontCenter) + frontCenter);
            //rb.MoveRotation(q * rb.rotation);
        }
        catch
        {
            Debug.LogError($"handlingVel {handlingVel}, {oversteerVel} caused an error");
            Debug.Break();
        }


    }

    private void CalculateBoost()
    {
        ////sliding and boosting
        //if ((status & C_SLIDING) == 0)
        //{
        //    //boost release
        //    if (quickBoostMeter >= 0.01f)
        //    {

        //        QuickBoostRoutine = StartCoroutine(Boost(boostStrength * quickBoostMeter * 2f, 0.1f, 0.0f));
        //        status &= (byte)~C_BSTREADY;
        //        quickBoostMeter = 0;
        //        elapsedSinceQuickBoost = 0;
        //    }
        //    else if (quickBoostMeter < 0.01f)
        //    {
        //        quickBoostMeter = 0;
        //        status &= (byte)~C_BSTREADY;
        //    }

        //}
        //else
        //{
        //    //boost charge
        //    if (elapsedSinceQuickBoost >= 0.2f)
        //    {
        //        quickBoostMeter = Mathf.Clamp(quickBoostMeter + Mathf.Abs(handlingVel / handling) * 0.75f * Time.deltaTime, 0, 1f);
        //    }
        //    //boost condition
        //    //if (sideProjection.magnitude >= frontProjection.magnitude)
        //    //{
        //    //    boostReady = true;
        //    //}
        //    if (quickBoostMeter >= 1)
        //    {
        //        status |= C_BSTREADY;
        //        quickBoostMeter = 1f;
        //    }
        //}

        ////drifting to charge boost (turbo)
        //if ((status & C_DRIFTING) != 0)
        //{
        //    //boost charge
        //    if (boostMeter >= 1)
        //    {
        //        status |= C_BSTREADY;
        //        boostMeter = 1.2f;
        //    }
        //    else
        //    {
        //        boostMeter += driftAngle / 45 * Time.deltaTime;
        //    }

        //}
        //else
        //{
        //    //boost release
        //    if (boostMeter >= 0.1f && (status & C_BSTING) == 0 && elapsedSinceBoost >= 0.1f)
        //    {

        //        BoostRoutine = StartCoroutine(Boost(boostStrength * boostMeter, 0.5f * boostMeter));
        //        status &= (byte)~C_BSTREADY;
        //        boostMeter = 0;
        //    }
        //}


        //drifting to charge, f-zero style boost

        //respect any boost coroutines running. don't stack, and don't interrupt
        if (!BST_running)
        {
            //if (inputHandler.GetBoost())
            //{
            //    //boost release
            //    if (boostMeter >= 0.01f)
            //    {
            //        rb.velocity += rb.rotation * Vector3.forward * boostStrength * Time.deltaTime;
            //        status |= C_BSTING;
            //        boostMeter -= 0.5f * Time.deltaTime;
            //        rawTractionModifier *= 1.2f;
            //    }
            //    else
            //    {
            //        boostMeter = 0;
            //        status &= (short)~C_BSTING;
            //        status &= (short)~C_BSTREADY;
            //    }

            //}
            //else
            //{
            //    //not boosting condition
            //    status &= (short)~C_BSTING;
            //}

            //f-zero style
            if (inputHandler.GetBoost())
            {
                //boost release
                if (boostMeter >= 0.01f)
                {
                    BoostRoutine = StartCoroutine(BoostWithMeter(boostStrength * 0.8f, Mathf.Lerp(boostStrength / 100f, 0.5f, 0.7f)));
                }
                else
                {
                    boostMeter = 0;
                }
            }
        }
        //boost charge: charge normally when sliding, charge slower while only drifting
        if (isSliding())
        {

            if (elapsedSinceBoost >= 0.1f)
            {
                boostMeter = Mathf.Clamp(boostMeter + inputHandler.GetEBrakes() * (0.15f) * Mathf.Clamp01((rb.velocity.magnitude - 50) / (300 * speedFactor)) * Time.deltaTime, 0, 1);

                doubleBoostMeter = Mathf.Clamp(doubleBoostMeter + inputHandler.GetEBrakes() * (0.2f) * Mathf.Clamp01((rb.velocity.magnitude - 50) / (300 * speedFactor)) * Time.deltaTime, 0, 1);
            }
        }
        if (isBraking())
        {

            if (elapsedSinceBoost >= 0.1f)
            {
                boostMeter = Mathf.Clamp(boostMeter + inputHandler.GetBrakes() * (0.3f) * Mathf.Clamp01((rb.velocity.magnitude - 50) / (300 * speedFactor)) * Time.deltaTime, 0, 1);

                doubleBoostMeter = Mathf.Clamp(doubleBoostMeter + inputHandler.GetBrakes() * (0.3f) * Mathf.Clamp01((rb.velocity.magnitude - 50) / (300 * speedFactor)) * Time.deltaTime, 0, 1);
            }
        }
        if (isDrifting())
        {
            //boost charge
            if (elapsedSinceBoost >= 0.1f)
            {
                boostMeter = Mathf.Clamp(boostMeter + driftAngle / 45 * (0.3f) * Mathf.Clamp01((rb.velocity.magnitude - 50) / 100) * Time.deltaTime, 0, 1);
                if (isSliding())
                {
                    doubleBoostMeter = Mathf.Clamp(doubleBoostMeter + inputHandler.GetBrakes() * driftAngle / 45 * (0.3f) * Mathf.Clamp01((rb.velocity.magnitude - 50) / 100) * Time.deltaTime, 0, 1);
                }
            }
        }

        //charge panels
        if (isDrivingOnTurbo())
        {
            boostMeter = Mathf.Clamp(boostMeter + 0.6f * Time.deltaTime, 0, 1);
        }

        //speed very slowly charges boost
        if (rb.velocity.magnitude > 100f)
        {
            if (elapsedSinceBoost >= 0.1f)
            {
                boostMeter = Mathf.Clamp(boostMeter + (rb.velocity.magnitude / (300 * speedFactor)) / 50 * Time.deltaTime, 0, 1);
            }
        }
    }

    private void TractionControl(float deltaTime)
    {
        //traction control
        //drift conditions by angle (for 2.4 - current)
        if ((status & C_DRIFTING) == 0 && driftAngle > fullDriftAngle)
        {
            status |= C_DRIFTING;
        }

        else if ((status & C_DRIFTING) != 0 && driftAngle < slightDriftAngle && Mathf.Abs(handlingVel) < handling)
        {
            status &= (byte)~C_DRIFTING;
        }

        float newTraction = Mathf.LerpUnclamped(0.05f, 0.1f, traction) * speedFactor + (isDrifting() ? 0 : 0.005f * speedFactor);

        if (!isGrounded())
        {
            newTraction *= 0.3f;
        }

        if (inputHandler.GetSteering() * ((Quaternion.Inverse(rb.rotation) * rb.velocity).x) > 0 && false)
        {
            //Debug.DrawRay(rb.position + rb.transform.up, rb.transform.up, Color.yellow, Time.deltaTime);
            currentTraction = newTraction * (1 - 0.1f * Mathf.Abs(inputHandler.GetSteering()));
        }
        else
        {
            currentTraction = Mathf.Lerp(currentTraction, newTraction, 0.05f * 60 * deltaTime);
        }
        
        //traction calculation and drift acceleration
        if (!IsRotatingInAir())
        {

            //rb.velocity -= Vector3.ClampMagnitude(sideProjection,
            //Mathf.Clamp(currentTraction * terrainModifier * 60 + downforce, 0.01f, float.MaxValue)) * 60 * Time.deltaTime;

            float rawFriction = Mathf.Max(currentTraction, 0.001f) * tractionModifier * terrainModifier * 60 + downforce;

            rb.velocity -= sideProjection.normalized * Mathf.Clamp(rawFriction, 0, sideProjection.magnitude) * 60 * deltaTime;

            if (isDrifting())
            {
                //last version factors: 0.5=8f, 0.1f
                //classic version
                //rb.AddRelativeForce(Vector3.forward * rb.mass *
                //    (0.8f * sideProjection.magnitude +
                //    driftAcceleration) *
                //    60 * Time.deltaTime);

                //classic + sqrt
                //rb.AddRelativeForce(Vector3.forward * rb.mass *
                //(Mathf.Sqrt(sideProjection.magnitude) +
                //driftAcceleration) *
                //60 * Time.deltaTime);

                //constant acceleration edition
                //rb.AddRelativeForce(Vector3.forward * rb.mass * acceleration * 120 * Time.deltaTime);

                //perfect match edition (1.8)
                //rb.velocity += rb.rotation * Vector3.forward *
                //    (Mathf.Clamp(sideProjection.magnitude, 0, 0.05f * 60 * Mathf.Clamp01(driftAngle / 60f) + driftAcceleration * 0.05f)) * 60 * Time.deltaTime;

                //perfect match edition (2.0)
                //(0.5f * (currentTraction - 0.05f) + 0.05f)
                //Mathf.Clamp01(driftAngle / (fullDriftAngle * 2))

                //true match + capped (2.?)
                //float standInSpeed = 50f;
                float trueMatch = 0;
                float discriminant = frontProjection.magnitude * frontProjection.magnitude + rawFriction * (2 * sideProjection.magnitude - rawFriction);
                if (discriminant > 0)
                    //rb.velocity += rb.rotation * Vector3.forward * (-frontProjection.magnitude + Mathf.Sqrt(discriminant)) * 0.9f * 60 * deltaTime;
                    trueMatch = (-frontProjection.magnitude + Mathf.Sqrt(discriminant));

                rb.velocity += rb.rotation * Vector3.forward * Mathf.Min(trueMatch * Mathf.Lerp(0.5f, 1, 1 - Mathf.Pow((driftAngle - 20) / 70, 2)) * 60, driftAcceleration * 3f + 75f/*, driftAngle / 90*/) * inputHandler.GetAcceleration() * deltaTime;

                if ((status & C_GROUNDED) != 0)
                {
                    //if (driftAngle > fullDriftAngle)
                    //{
                    //    //full drifting
                    //    rb.velocity += rb.rotation * Vector3.forward *
                    //    Mathf.Clamp(driftAcceleration * 2 * Mathf.Clamp01(-(driftAngle - fullDriftAngle) / 120 + 1), 0, sideProjection.magnitude) * Time.deltaTime;
                    //}
                    //else
                    //{
                    //    //slight drifting
                    //    rb.velocity += rb.rotation * Vector3.forward *
                    //    Mathf.Clamp(driftAcceleration, 0, sideProjection.magnitude) * Time.deltaTime;
                    //}

                    //update the limiter. to only go up
                    //driftAccLimiter = Mathf.Clamp01(Mathf.Max(driftAccLimiter, driftAngle / 30));

                    //only limited at slignt angles
                    rb.velocity += rb.rotation * Vector3.forward * Mathf.Lerp(0, driftAcceleration * Mathf.Pow(0.9977f, rb.velocity.magnitude / speedFactor), Mathf.Clamp01(driftAngle / 20)) * deltaTime;

                    //treat drift acceleration as normal acceleration, but using frontvelocity
                    //rb.velocity += rb.rotation * Vector3.forward * (maxSpeed - frontProjection.magnitude) / maxSpeed * driftAcceleration * 10 * inputHandler.GetAcceleration() * deltaTime;
                }
            }
            else
            {
                driftAccLimiter = 0;
            }

            //Debug.DrawRay(rb.position + transform.up, sideProjection, Color.red, Time.deltaTime);
        }
    }

    private void Reset()
    {
        StartCoroutine(ResetDelayed());
    }
    
    private IEnumerator ResetDelayed()
    {
        yield return null;
        yield return new WaitForFixedUpdate();

        rb.position = FinishLine.transform.position + FinishLine.transform.rotation * Vector3.back * 5;
        rb.position += FinishLine.transform.rotation * Vector3.right * (((inputHandler.playerNum / 6) * 1.5f) % 6 + (inputHandler.playerNum * 5f) % 30f);
        rb.position += FinishLine.transform.rotation * Vector3.back * (inputHandler.playerNum / 6) * 5f;

        //offset to center the row of cars
        rb.position -= FinishLine.transform.rotation * Vector3.right * ((float)(carsInARow - 1) / 2 * 5f);
        //Debug.Log(FinishLine.transform.rotation * Vector3.right * ((float)(carsInARow - 1) / 2 * 3f));

        rb.rotation = FinishLine.transform.rotation;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        countdownTimestamp = Time.time;
        boostsReady = 0;
        boostMeter = 0f;
        doubleBoostMeter = 0f;
        rb.useGravity = normalGravity;

        status = 0x0;

        StopAllCoroutines();
    }

    public void Respawn()
    {
        Debug.Log("Respawn Called");
        rb.rotation = Quaternion.LookRotation(inputHandler.GetLastPoint().GetDir(), inputHandler.GetLastPoint().GetNormal());
        currentUp = inputHandler.GetLastPoint().GetNormal();
        rb.position = inputHandler.GetLastPoint().GetPos() + currentUp * 2f;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
        boostsReady = 0;
        boostMeter = boostMeter * 0.3f - 0.1f;
        doubleBoostMeter = doubleBoostMeter * 0.28f - 0.1f;

        status = 0x0;
        status |= C_ENABLED;
    }

    IEnumerator Boost(float strength, float length)
    {
        status |= C_BSTING;
        boostStart = true;
        BST_running = true;

        float timestamp = Time.time;
        while (Time.time - timestamp < length && inputHandler.GetAcceleration() > 0.01f)
        {
            //rb.velocity += rb.rotation * Vector3.forward * (strength * Time.deltaTime) / length;

            //if (((maxSpeed + strength) - rb.velocity.magnitude) * 0.66f < 20)
            //{
            //    rb.velocity += rb.rotation * Vector3.forward * 20 * Time.deltaTime;
            //}
            //else
            //{
            //    rb.velocity += rb.rotation * Vector3.forward * ((maxSpeed + strength) - rb.velocity.magnitude) * 0.66f * Time.deltaTime;
            //}

            rb.velocity += rb.rotation * Vector3.forward * Mathf.Pow(0.996f, frontProjection.magnitude / speedFactor - 200) * strength * Time.deltaTime;

            yield return null;
            boostStart = false;
        }

        elapsedSinceBoost = 0;
        status &= (byte)~C_BSTING;
        BST_running = false;
    }
    IEnumerator Boost(float strength, float length, float cooldown)
    {
        status |= C_BSTING;
        boostStart = true;
        BST_running = true;

        float timestamp = Time.time;
        while (Time.time - timestamp < length && inputHandler.GetAcceleration() > 0.01f)
        {
            //rb.velocity += rb.rotation * Vector3.forward * (strength / length) * Time.deltaTime;

            //if (((maxSpeed + strength) - rb.velocity.magnitude) * 0.66f < 20)
            //{
            //    rb.velocity += rb.rotation * Vector3.forward * 20 * Time.deltaTime;
            //}
            //else
            //{
            //    rb.velocity += rb.rotation * Vector3.forward * ((maxSpeed + strength) - rb.velocity.magnitude) * 0.66f * Time.deltaTime;
            //}

            rb.velocity += rb.rotation * Vector3.forward * Mathf.Pow(0.996f, frontProjection.magnitude / speedFactor - 200) * strength * Time.deltaTime;

            yield return null;
            boostStart = false;
        }


        elapsedSinceBoost = 0;
        status &= (byte)~C_BSTING;
        timestamp = Time.time;

        while (Time.time - timestamp < cooldown && inputHandler.GetAcceleration() > 0.01f)
        {
            yield return null;
        }

        BST_running = false;
    }

    /// <summary>
    /// Boosts using meter
    /// </summary>
    /// <param name="strength">The multiplier applied to car's boostStrength in this function.</param>
    /// <param name="drain">How much meter is drained per second</param>
    /// <param name="cooldown">how much time before you can boost again</param>
    /// <returns></returns>
    IEnumerator BoostWithMeter(float strength, float drain, float cooldown = 0.25f)
    {
        status |= C_BSTING;
        BST_running = true;
        boostStart = true;

        while (boostMeter > 0f && inputHandler.GetBoost())
        {
            //if (((maxSpeed + strength) - rb.velocity.magnitude) * 0.66f < 20)
            //{
            //    rb.velocity += rb.rotation * Vector3.forward * 20 * Time.deltaTime;
            //}
            //else
            //{
            //    rb.velocity += rb.rotation * Vector3.forward * ((maxSpeed + strength) - rb.velocity.magnitude) * 0.66f * Time.deltaTime;
            //}

            if (doubleBoostMeter > 0f)
            {
                status |= C_DBSTING;

                rb.velocity += rb.rotation * Vector3.forward * Mathf.Pow(0.994f, frontProjection.magnitude / speedFactor - 200) * strength * Time.deltaTime;

                doubleBoostMeter -= drain * Time.deltaTime;
            }
            else
            {
                doubleBoostMeter = 0;
                status &= (short)~C_DBSTING;
            }
            rb.velocity += rb.rotation * Vector3.forward * Mathf.Pow(0.996f, frontProjection.magnitude / speedFactor - 200) * strength * Time.deltaTime;

            boostMeter -= drain * Time.deltaTime;

            yield return null;
            boostStart = false;
        }

        if (boostMeter < 0.01f)
            boostMeter = 0;


        elapsedSinceBoost = 0;
        status &= (short)~C_BSTING;
        float timestamp = Time.time;

        while (Time.time - timestamp < cooldown && inputHandler.GetAcceleration() > 0.01f)
        {
            yield return null;
        }

        BST_running = false;
    }
}


