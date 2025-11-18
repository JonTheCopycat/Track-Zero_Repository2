using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using CarBehaviour;

public class CameraControl : MonoBehaviour
{
    private CarControl carControl;
    private Camera mainCamera;
    [SerializeField]
    private VisualEffect SpeedLines;
    private float currentFov;

    public Rigidbody target;
    public Vector3 offset;
    public float cameraStiffness;
    public float cameraAngle;
    float targetRotationSpeed;
    bool lockedCam;
    RaycastHit cameraHit;
    Vector3 extraOffset;
    Vector3 boostOffset;
    Vector3 velocity;
    Vector3[] lastVs;
    Vector3 deltaV;
    Queue<Quaternion> lastRotation;
    Quaternion speedDirection;
    Quaternion carDirection;
    Quaternion postOffsetRotDirection;
    Quaternion finalPosDirection;
    Quaternion finalRotDirection;
    Quaternion lastRotationDirection;
    Quaternion lastPositionDirection;

    //screen shake
    float shakeAcceleration;
    float shakeVelocity;
    float shakePosition;

    // Start is called before the first frame update
    void Start()
    {
        //offset = new Vector3(0, 4f, -6);
        cameraAngle = 0;
        lockedCam = false;

        velocity = Vector3.zero;
        lastVs = new Vector3[2];
        lastVs[0] = Vector3.zero;
        lastVs[1] = Vector3.zero;
        lastRotation = new Queue<Quaternion>();
        if (target == null)
        {
            for (int i = 0; i < 2; i++)
                lastRotation.Enqueue(Quaternion.identity);
            
        }
        else
        {

            for (int i = 0; i < 3; i++)
                lastRotation.Enqueue(target.transform.rotation);
        }
        lastRotationDirection = Quaternion.identity;
        lastPositionDirection = Quaternion.identity;
        targetRotationSpeed = 0;

        if (GameSettings.cameraPosition >= 0 && 
            GameSettings.cameraPosition < GameSettings.allCameraPositions.Length)
        {
            offset = GameSettings.allCameraPositions[GameSettings.cameraPosition].GetOffset() * target.transform.localScale.z;
            cameraAngle = GameSettings.allCameraPositions[GameSettings.cameraPosition].GetAngle();
            lockedCam = GameSettings.allCameraPositions[GameSettings.cameraPosition].IsLockedCam();
        }

        cameraStiffness = GameSettings.cameraStiffness;

        shakeAcceleration = 0;
        shakeVelocity = 0;
        shakePosition = 0;
        boostOffset = Vector3.zero;

        carControl = target.GetComponent<CarControl>();
        mainCamera = GetComponent<Camera>();

        if (mainCamera != null)
        {
            currentFov = mainCamera.fieldOfView;
        }
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            //direction of the car, including rotation offset
            if (target.velocity.magnitude < 10 || Vector3.Angle(target.rotation * Vector3.forward, target.velocity) < 150)

            {
                //if target is going in the direction they are facing
                carDirection = Quaternion.LookRotation(
                lastRotation.Peek() * Vector3.forward * Mathf.Cos(Mathf.Deg2Rad * (cameraAngle - (targetRotationSpeed * 0.2f))) +
                lastRotation.Peek() * Vector3.down * Mathf.Sin(Mathf.Deg2Rad * cameraAngle) -
                lastRotation.Peek() * Vector3.right * Mathf.Sin(Mathf.Deg2Rad * (targetRotationSpeed * 0.2f)),
                    lastRotation.Peek() * new Vector3(0, 1, 0.2f)
                    );
            }
            else
            {
                //if target is going backwards
                carDirection = Quaternion.LookRotation(
                lastRotation.Peek() * -Vector3.forward * Mathf.Cos(Mathf.Deg2Rad * cameraAngle) +
                lastRotation.Peek() * Vector3.down * Mathf.Sin(Mathf.Deg2Rad * cameraAngle),
                    lastRotation.Peek() * new Vector3(0, 1, 0.2f)
                    );
            }


            //transform.rotation = Quaternion.Lerp(transform.rotation, speedDirection, 0.4f);
            if (Mathf.Floor(target.velocity.magnitude) >= 60 && !lockedCam)
            {
                //direction of the speed, including car tilt and rotation offset
                speedDirection = Quaternion.LookRotation(
                    target.velocity.normalized * Mathf.Cos(Mathf.Deg2Rad * cameraAngle) + target.transform.rotation * Vector3.down * Mathf.Sin(Mathf.Deg2Rad * cameraAngle),
                        target.transform.rotation * new Vector3(0, 1, 0.5f));
                //the averaged direction of the car and speed

                Quaternion rawPosDirection = Quaternion.Lerp(speedDirection, carDirection, 0.2f);
                Quaternion rawRotDirection = Quaternion.Lerp(speedDirection, carDirection, 0.3f);
                //finalPosDirection = Quaternion.LookRotation(
                //    Vector3.RotateTowards(lastPositionDirection * Vector3.forward, rawPosDirection * Vector3.forward, Mathf.PI * 1.2f * Time.deltaTime, 0),
                //    Vector3.RotateTowards(lastPositionDirection * Vector3.up, rawPosDirection * Vector3.up, Mathf.PI * 0.33f * Time.deltaTime, 0)
                //    );
                //finalRotDirection = Quaternion.LookRotation(
                //    Vector3.RotateTowards(lastRotationDirection * Vector3.forward, rawRotDirection * Vector3.forward, Mathf.PI * 1.2f * Time.deltaTime, 0),
                //    Vector3.RotateTowards(lastRotationDirection * Vector3.up, rawRotDirection * Vector3.up, Mathf.PI * 0.33f * Time.deltaTime, 0)

                //finalPosDirection = Quaternion.LookRotation(
                //    Vector3.RotateTowards(rawPosDirection * Vector3.forward, lastPositionDirection * Vector3.forward,
                //    Mathf.Clamp(Vector3.Angle(rawPosDirection * Vector3.forward, lastPositionDirection * Vector3.forward) * Mathf.Deg2Rad - Mathf.PI * 0.66f * Time.deltaTime, 0, Mathf.PI * 0.125f), 0),
                //    Vector3.RotateTowards(rawPosDirection * Vector3.up, lastPositionDirection * Vector3.up,
                //    Mathf.Clamp(Vector3.Angle(rawPosDirection * Vector3.up, lastPositionDirection * Vector3.up) * Mathf.Deg2Rad - Mathf.PI * 0.25f * Time.deltaTime, 0, Mathf.PI * 0.125f), 0)
                //    );
                //finalRotDirection = Quaternion.LookRotation(
                //    //forward
                //    Vector3.RotateTowards(rawRotDirection * Vector3.forward, carDirection * Vector3.forward, 
                //    Mathf.Clamp(Vector3.Angle(rawRotDirection * Vector3.forward, lastRotationDirection * Vector3.forward) * Mathf.Deg2Rad - Mathf.PI * 0.66f * Time.deltaTime, 0, Mathf.PI * 0.125f), 0),
                //    //up
                //    Vector3.RotateTowards(rawRotDirection * Vector3.up, carDirection * Vector3.up, 
                //    Mathf.Clamp(Vector3.Angle(rawRotDirection * Vector3.up, lastRotationDirection * Vector3.up) * Mathf.Deg2Rad - Mathf.PI * 0.25f * Time.deltaTime, 0, Mathf.PI * 0.125f), 0)
                //    );
                finalPosDirection = Quaternion.Lerp(lastPositionDirection, Quaternion.Lerp(speedDirection, carDirection, 0.2f),
                    0.75f * 60 * Time.deltaTime);
                finalRotDirection = Quaternion.Lerp(lastRotationDirection, Quaternion.Lerp(speedDirection, carDirection, 0.3f), 0.8f * 60 * Time.deltaTime);

                //finalPosDirection = Quaternion.Lerp(speedDirection, carDirection, 0.2f);
                //finalRotDirection = Quaternion.Lerp(speedDirection, carDirection, 0.3f);
            }
            else
            {
                finalPosDirection = SpecialLerp(lastPositionDirection, carDirection,
                    0.05f + Mathf.Pow(100, cameraStiffness - 1));
                finalRotDirection = SpecialLerp(lastRotationDirection, carDirection, 0.05f + Mathf.Pow(100, cameraStiffness - 1));
            }


            //calculate extraOffset using the change in velocity from the target
            //extraOffset = /*Vector3.ProjectOnPlane(-deltaV * (1 - cameraStiffness) * 2f, target.transform.up) +*/ Vector3.ClampMagnitude(-target.velocity * 0.005f, 2.5f);
            //extraOffset = extraOffset.sqrMagnitude * 2 * extraOffset.normalized;

            extraOffset = -deltaV * (1 - cameraStiffness) * 0.75f;

            Vector3 directionToUse = Vector3.Cross(transform.right, target.transform.up);
            Quaternion quaternionToUse = Quaternion.LookRotation(directionToUse, target.transform.up);
            //clamp the offset
            float clampedY;
            float clampedZ;
            float clampedX;

            if (Physics.Raycast(
                new Ray(transform.position + finalPosDirection * offset, target.transform.up), out cameraHit, 20f))
            {
                clampedY = Mathf.Clamp((Quaternion.Inverse(quaternionToUse) * Vector3.Project(extraOffset, target.transform.up)).y,
                        -offset.y * (1 + cameraAngle / 15), 4f);
            }
            else
            {
                clampedY = Mathf.Clamp((Quaternion.Inverse(quaternionToUse) * Vector3.Project(extraOffset, target.transform.up)).y,
                        -offset.y * (1 + cameraAngle / 15), 4f);
            }

            //if there is something in the path of the camera (forwards and backwards direction)
            if (Physics.Raycast(
                new Ray(transform.position + finalPosDirection * offset + new Vector3(0, clampedY, 0),
                transform.rotation * Vector3.forward * (Quaternion.Inverse(transform.rotation) * Vector3.Project(extraOffset, transform.rotation * Vector3.forward)).z),
                out cameraHit, 35f))
            {
                clampedZ = Mathf.Clamp((Quaternion.Inverse(quaternionToUse) * Vector3.Project(extraOffset, directionToUse)).z,
                    -cameraHit.distance + 0.25f, 2f);
            }
            else
            {
                clampedZ = Mathf.Clamp((Quaternion.Inverse(quaternionToUse) * Vector3.Project(extraOffset, directionToUse)).z,
                    -4f, 1.5f);
            }

            //if there is something in the path of the camera (forwards and backwards direction)
            if (Physics.Raycast(
                new Ray(transform.position + finalPosDirection * offset + new Vector3(0, clampedY, clampedZ),
                transform.rotation * Vector3.right * (Quaternion.Inverse(transform.rotation) * Vector3.Project(extraOffset, transform.rotation * Vector3.right)).x),
                out cameraHit, 35f))
            {
                clampedX = Mathf.Clamp((Quaternion.Inverse(quaternionToUse) * Vector3.Project(extraOffset, target.transform.right)).x,
                    -cameraHit.distance + 0.25f, 1f);
            }
            else
            {
                clampedX = Mathf.Clamp((Quaternion.Inverse(quaternionToUse) * Vector3.Project(extraOffset, target.transform.right)).x,
                    -4f, 1f);
            }
            postOffsetRotDirection = carDirection * Quaternion.Euler(new Vector3(clampedY * 0.5f * Mathf.Rad2Deg, 0, 0));
            //reconstruct the extraOffset
            extraOffset = quaternionToUse * new Vector3(clampedX * 0.1f, clampedY, clampedZ);

            //hood cam special
            if (lockedCam)
            {
                finalRotDirection = SpecialLerp(lastRotationDirection, carDirection,
                30);
                finalPosDirection = carDirection;

                extraOffset = extraOffset * 0.1f;
            }

            //calculate the final direction to always look at the player
            if (Mathf.Floor(target.velocity.magnitude) >= 60 && !lockedCam)
            {
                //postOffsetRotDirection = carDirection * Quaternion.Euler(new Vector3((Quaternion.Inverse(carDirection) * extraOffset).y * 0.25f * Mathf.Rad2Deg,
                //    (Quaternion.Inverse(carDirection) * extraOffset).x * 0.25f * Mathf.Rad2Deg,
                //    0));

                //postOffsetRotDirection = Quaternion.RotateTowards(speedDirection, Quaternion.LookRotation(
                //     -(finalPosDirection * offset + extraOffset) * Mathf.Cos(Mathf.Deg2Rad * 5) + target.transform.up * Mathf.Sin(Mathf.Deg2Rad * 15),
                //    Vector3.RotateTowards(lastRotationDirection * Vector3.up, target.transform.rotation * new Vector3(0, 1, 0.25f), Mathf.PI * 0.25f * Time.deltaTime, 0)),
                //    30);

                postOffsetRotDirection = SpecialLerp(lastRotationDirection, Quaternion.Lerp(postOffsetRotDirection, finalRotDirection, 0.7f), 0.05f + Mathf.Pow(100, 0.4f));

                //postOffsetRotDirection = Quaternion.Lerp(postOffsetRotDirection, finalRotDirection, 0.3f);

                //postOffsetRotDirection = finalRotDirection;
            }
            else if (!lockedCam)
            {

                postOffsetRotDirection = finalRotDirection;
            }

            //to do: make the finalRotation based on the extraOffset as well.
            //order should be: position > extraOffset > rotation

            
            

            Vector3 shakeOffset;
            //screenshake
            if (carControl != null)
            {
                shakeAcceleration = -(15f * shakeVelocity + 300 * shakePosition);

                if (Time.time - carControl.GetLastCollision().Item2 <= Time.deltaTime)
                    shakeVelocity += 0.25f * carControl.GetLastCollision().Item1;
                shakeVelocity = shakeVelocity + shakeAcceleration * Time.deltaTime;

                shakePosition = shakePosition + shakeVelocity * Time.deltaTime;

                //if (Mathf.Abs(shakePosition) < 0.01f && Mathf.Abs(shakeVelocity) < 0.01f && carControl.GetLastCollision().Item2 <= Time.deltaTime * 2)
                //{
                //    shakePosition = shakeVelocity = 0;
                //}
                //Debug.Log(shakePosition);
                shakeOffset = transform.right * shakePosition;
            }
            else
            {
                shakeOffset = Vector3.zero;
            }

            //fov modifier
            if (mainCamera != null && carControl != null)
            {
                float basePov = 85 + target.velocity.magnitude / 500 * 25;
                if (carControl.isBoosting())
                {
                    currentFov = Mathf.MoveTowards(currentFov, basePov + 5, (10 / 0.4f) * Time.deltaTime);

                    boostOffset = -target.transform.forward * Mathf.Lerp(boostOffset.magnitude,
                            1.5f,
                            0.2f * 60f * Time.deltaTime);
                }
                else if (carControl.isBoostDrifting()) {
                    currentFov = Mathf.MoveTowards(currentFov, basePov + 4, (10 / 0.4f) * Time.deltaTime);

                    boostOffset = -target.transform.forward * Mathf.Lerp(boostOffset.magnitude,
                            1f,
                            0.2f * 60f * Time.deltaTime);
                }
                else
                {
                    currentFov = Mathf.MoveTowards(currentFov, basePov, (10 / 0.4f) * Time.deltaTime);

                    boostOffset = -target.transform.forward.normalized * Mathf.MoveTowards(boostOffset.magnitude, 0, 1f * Time.deltaTime);
                }

                mainCamera.fieldOfView = currentFov;
            }
            if (lockedCam)
                boostOffset = Vector3.zero;

            //speedlines spawnrate modifier
            if (target.velocity.magnitude > 300 * CarControl.speedFactor && GameSettings.speedlines)
            {
                SpeedLines.SetFloat("radius", Mathf.Lerp(2.6f, 1.85f, (target.velocity.magnitude - 300 * CarControl.speedFactor) / (75 * CarControl.speedFactor)));
                SpeedLines.SetFloat("spawnRate", 600);
            }
            else
            {
                SpeedLines.SetFloat("spawnRate", 0);
            }
            

            //transform.position = target.transform.position + finalPosDirection * offset + extraOffset;
            //transform.rotation = postOffsetRotDirection;
            //transform.rotation = finalDirection;
            transform.rotation = postOffsetRotDirection;
            transform.position = target.transform.position + finalPosDirection * offset + extraOffset + boostOffset + shakeOffset + Vector3.ClampMagnitude(-velocity.normalized * Mathf.Sqrt(velocity.magnitude / (350 * CarControl.speedFactor)) * 2f, 3f);

            lastPositionDirection = finalPosDirection;
            lastRotationDirection = postOffsetRotDirection;

            lastRotation.Dequeue();
            lastRotation.Enqueue(target.transform.rotation);
        }
    }

    
    private void FixedUpdate()
    {
        //manually calculate velocity of the transform. attempt to fix jitter
        velocity = Vector3.Lerp(velocity, target.velocity, Time.fixedDeltaTime * 7.5f);
        if (velocity.magnitude >= 1000)
            velocity = Vector3.Lerp(velocity, Vector3.zero, Time.fixedDeltaTime * 60f);

        //velocity = target.velocity;
        Vector3 rawDeltaV = (velocity - lastVs[1]) * 0.5f;

        //deltaV = Vector3.Lerp(deltaV, velocity - lastVelocity, Time.deltaTime * 3f);
        deltaV = Vector3.MoveTowards(deltaV, Vector3.Lerp(deltaV, rawDeltaV, Time.fixedDeltaTime * 7.5f), Time.fixedDeltaTime * 10f);
        //Debug.Log($"deltaV: {deltaV}");

        Quaternion lastProjectedRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(lastRotation.Peek() * Vector3.forward, target.rotation * Vector3.up), target.rotation * Vector3.up);
        float targetRotationDelta = Quaternion.Angle(target.rotation, lastProjectedRotation) * ((Quaternion.Inverse(target.rotation) * lastProjectedRotation * Vector3.forward).x < 0 ? 1 : -1);
        //Debug.Log($"targetRotation: {targetRotationDelta} in {Time.fixedDeltaTime} seconds");
        //rotation speed(in degrees)
        //targetRotationSpeed = Mathf.MoveTowardsAngle(
        //    targetRotationSpeed,
        //    targetRotationDelta / Time.fixedDeltaTime,
        //    Mathf.Abs(targetRotationDelta / Time.fixedDeltaTime) < 0.01f ? 6f : 4f
        //    );
        ////targetRotationSpeed = targetRotationDelta / Time.fixedDeltaTime;
        targetRotationSpeed = 0;

        lastVs[1] = lastVs[0];
        lastVs[0] = velocity;
    }

    // Update is called once per frame
    void Dormant()
    {
        //direction of the car, including rotation offset
        carDirection = Quaternion.LookRotation(
            target.rotation * Vector3.forward * Mathf.Cos(Mathf.Deg2Rad * cameraAngle) + target.rotation * Vector3.down * Mathf.Sin(Mathf.Deg2Rad * cameraAngle),
                target.rotation * new Vector3(0, 1, 0.25f));

        

        //transform.rotation = Quaternion.Lerp(transform.rotation, speedDirection, 0.4f);
        if (Mathf.Floor(target.velocity.magnitude) >= 100 && !lockedCam)
        {
            //direction of the speed, including car tilt and rotation offset
            speedDirection = Quaternion.LookRotation(
                target.velocity.normalized * Mathf.Cos(Mathf.Deg2Rad * cameraAngle) + target.rotation * Vector3.down * Mathf.Sin(Mathf.Deg2Rad * cameraAngle),
                    target.rotation * new Vector3(0, 1, 0.25f));
            //the averaged direction of the car and speed
            finalPosDirection = SpecialLerp(lastPositionDirection, Quaternion.Lerp(speedDirection, carDirection, 0.6f),
                0.001f + Mathf.Pow(600, cameraStiffness - 1));

            //dampen certain movements
            //Vector3 newPosition = Vector3.MoveTowards(
            //    Vector3.Project(lastPosition, generalTargetRotation * Vector3.up),
            //    Vector3.Project(transform.position, generalTargetRotation * Vector3.up),
            //    60 * Time.deltaTime
            //    );
            //newPosition += Vector3.ProjectOnPlane(transform.position, generalTargetRotation * Vector3.up);
            //transform.position = newPosition;
        }
        else
        {
            finalPosDirection = SpecialLerp(lastPositionDirection, carDirection,
                0.001f + Mathf.Pow(600, cameraStiffness - 1));
        }

        //transform.position = Vector3.Lerp(transform.position, target.position + finalDirection * offset + extraOffset, 0.9f);
        //transform.position = target.position + speedDirection * offset - Vector3.Project(deltaV, target.rotation * Vector3.forward) * 0.5f;
        //transform.position = target.position + finalDirection * offset + extraOffset;
        //transform.rotation = Quaternion.Lerp(transform.rotation, finalDirection, 0.4f);
        //transform.rotation = finalDirection;

        //lastPosition = transform.position;
        //lastPositionDirection = finalPosDirection;

        //deltaV = Vector3.MoveTowards(deltaV, target.velocity - lastVelocity, 10f * Time.deltaTime);
        //lastRotationDirection = finalRotDirection;
        //lastVelocity = target.velocity;
    }

    Quaternion SpecialLerp(Quaternion a, Quaternion b, float t)
    {
        float angleDifference = Vector3.Angle(a * Vector3.forward, b * Vector3.forward);
        return Quaternion.RotateTowards(a, b, angleDifference * t * 60 * Time.deltaTime);
    }
}
