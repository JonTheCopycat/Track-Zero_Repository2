using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System.IO;
using Cars;
using AI;

namespace CarBehaviour
{
    public class CarInputHandler : MonoBehaviour
    {
        [SerializeField]
        AllInputActions controls;
        private CarAI carAI;
        private CarControl carControl;
        public int playerNum;

        //ai variables
        bool boosting = false;
        int turnState = 0;
        bool reversing = false;
        /*
         * 0 = straightaway
         * 1 = slightturn
         * 2 = tight turn
         * 3 = extra tight turn
         */

        //here temporarily
        Rigidbody rb;

        public enum InputType
        {
            PLAYER,
            COMPUTER,
            ONLINE,
            REPLAY,
            NONE
        }

        public InputType inputType;

        float acceleration, brakes, ebrake, steering, pitch, rawSteering;
        bool boost, respawn, start;
        bool isEbrake, isBoost, isRespawnPressed, isStart;

        //ai only
        Car car;
        Vector3[] correctDirection = new Vector3[4];
        Vector3[] correctNormal = new Vector3[4];

        Vector3 lastV; //short for last velocity
        float rubberband = 0;

        float trackCurveAngle;
        float trackCurveAngle2;
        float speedCurveAngle;

        private bool scanning;
        AIPath aiPath;
        float timestamp;
        bool backingUp;

        bool started = false;
        public bool collectingData;
        List<List<float>> drivingData;

        public float GetAcceleration()
        {
            return acceleration;
        }
        public float GetBrakes()
        {
            return brakes;
        }
        public float GetSteering()
        {
            return steering;
        }
        public float GetPitch()
        {
            return pitch;
        }
        public float GetEBrakes()
        {
            return ebrake;
        }

        public bool GetBoost()
        {
            return boost;
        }

        public bool GetBoostDown()
        {
            return boost && !isBoost;
        }

        public bool GetReset()
        {
            return respawn;
        }
        public bool GetResetDown()
        {
            return respawn && !isRespawnPressed;
        }
        public bool GetStart()
        {
            return start;
        }
        public bool GetStartDown()
        {
            return start && !isStart;
        }

        public float GetDeadzone()
        {
            return GameSettings.deadzone;
        }

        public void SetAcceleration(float a)
        {
            acceleration = a;
        }
        public void SetBrakes(float b)
        {
            brakes = b;
        }
        public void SetSteering(float s)
        {
            steering = s;
        }
        public void SetPitch(float p)
        {
            pitch = p;
        }
        public void SetEBrakes(float e)
        {
            ebrake = e;
        }

        public void SetBoost(bool b)
        {
            boost = b;
        }

        public void SetReset(bool r)
        {
            respawn = r;
        }

        public void SetStart(bool s)
        {
            start = s;
        }

        public AIPath.Point GetLastPoint()
        {
            if (carAI == null || !carAI.isActiveAndEnabled)
                return new AIPath.Point();
            else
                return carAI.GetLastRespawnPoint();
        }

        public void SetRubberband(float value)
        {
            rubberband = Mathf.Clamp(value, -100, 100);
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            rb = GetComponent<Rigidbody>();
            carAI = GetComponent<CarAI>();
        }

        public void CustomStart()
        {
            car = GetComponent<CarGetter>().GetCar();
            carControl = GetComponent<CarControl>();

            controls = InputManager.inputActions;
            controls.Gamepad.Enable();
            controls.Keyboard.Enable();
            backingUp = false;

            lastV = rb.velocity;
            trackCurveAngle = 0;
            trackCurveAngle2 = 0;
            speedCurveAngle = 0;

            timestamp = 0;
            if (collectingData == true)
            {
                drivingData = new List<List<float>>();
            }

            aiPath = PathDataManager.LoadAIPath(ScreenManager.current.GetSceneName());

            Debug.Log("CarInputHandler");
            started = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (this == null)
            {
                Debug.Break();
            }

            if (started)
            {
                if (carAI != null)
                    scanning = carAI.isScanning();
                else
                    scanning = false;

                RaycastHit result;
                bool isGrounded;
                if (Physics.Raycast(new Ray(rb.position, rb.rotation * -Vector3.up), out result, 1f * transform.localScale.y))
                {
                    if (result.transform.tag.Equals("KillPlane"))
                        isGrounded = false;
                    else
                        isGrounded = true;
                }
                else
                    isGrounded = false;


                if (carAI != null)
                {
                    correctDirection = carAI.GetCorrectDirections();
                    correctNormal = carAI.GetCorrectNormals();
                }

                //take in input if applicable
                if (inputType == InputType.PLAYER)
                {
                    //no longer checking if controller is being used. use whatever is being pressed harder.
                    //Debug.Log($"Gamepad steer: {controls.Gamepad.G_Steer.ReadValue<float>()}");
                    if (Mathf.Abs(controls.Gamepad.G_Steer.ReadValue<float>()) > GetDeadzone() ||
                        Mathf.Abs(controls.Keyboard.K_Steer.ReadValue<float>()) > GetDeadzone())
                    {
                        //Debug.Log("Steering");
                        if (Mathf.Abs(controls.Gamepad.G_Steer.ReadValue<float>()) > Mathf.Abs(controls.Keyboard.K_Steer.ReadValue<float>()))
                        {
                            if (controls.Gamepad.G_Steer.ReadValue<float>() > GetDeadzone())
                                steering = (controls.Gamepad.G_Steer.ReadValue<float>() - GetDeadzone()) / (1 - GetDeadzone());
                            else if (controls.Gamepad.G_Steer.ReadValue<float>() < -GetDeadzone())
                                steering = (controls.Gamepad.G_Steer.ReadValue<float>() + GetDeadzone()) / (1 - GetDeadzone());
                        }
                        else
                        {
                            float targetSteering = 0;
                            if (controls.Keyboard.K_Steer.ReadValue<float>() > GetDeadzone())
                                targetSteering = (controls.Keyboard.K_Steer.ReadValue<float>() - GetDeadzone()) / (1 - GetDeadzone());
                            else if (controls.Keyboard.K_Steer.ReadValue<float>() < -GetDeadzone())
                                targetSteering = (controls.Keyboard.K_Steer.ReadValue<float>() + GetDeadzone()) / (1 - GetDeadzone());

                            if (Mathf.Abs(targetSteering) > Mathf.Abs(steering) && steering * targetSteering > 0)
                                steering = Mathf.MoveTowards(steering, targetSteering, 4f * Time.deltaTime);
                            else
                                steering = targetSteering;
                        }
                    }
                    else
                        steering = 0;

                    if (Mathf.Abs(controls.Gamepad.G_Pitch.ReadValue<float>()) > GetDeadzone() ||
                        Mathf.Abs(controls.Keyboard.K_Pitch.ReadValue<float>()) > GetDeadzone())
                    {
                        //Debug.Log("Steering");
                        if (Mathf.Abs(controls.Gamepad.G_Pitch.ReadValue<float>()) > Mathf.Abs(controls.Keyboard.K_Pitch.ReadValue<float>()))
                        {
                            if (controls.Gamepad.G_Pitch.ReadValue<float>() > GetDeadzone())
                                pitch = (controls.Gamepad.G_Pitch.ReadValue<float>() - GetDeadzone()) / (1 - GetDeadzone());
                            else if (controls.Gamepad.G_Pitch.ReadValue<float>() < -GetDeadzone())
                                pitch = (controls.Gamepad.G_Pitch.ReadValue<float>() + GetDeadzone()) / (1 - GetDeadzone());
                        }
                        else
                        {
                            float targetSteering = 0;
                            if (controls.Keyboard.K_Pitch.ReadValue<float>() > GetDeadzone())
                                targetSteering = (controls.Keyboard.K_Pitch.ReadValue<float>() - GetDeadzone()) / (1 - GetDeadzone());
                            else if (controls.Keyboard.K_Pitch.ReadValue<float>() < -GetDeadzone())
                                targetSteering = (controls.Keyboard.K_Pitch.ReadValue<float>() + GetDeadzone()) / (1 - GetDeadzone());

                            if (Mathf.Abs(targetSteering) > Mathf.Abs(steering) && steering * targetSteering > 0)
                                pitch = Mathf.MoveTowards(steering, targetSteering, 4f * Time.deltaTime);
                            else
                                pitch = targetSteering;
                        }
                    }
                    else
                        pitch = 0;

                    //acceleration
                    if (controls.Gamepad.G_Accelerate.ReadValue<float>() > GetDeadzone() ||
                        controls.Keyboard.K_Accelerate.ReadValue<float>() > 0.1f)
                    {
                        //Debug.Log("Accelerating");
                        if (Mathf.Abs(controls.Gamepad.G_Accelerate.ReadValue<float>()) > Mathf.Abs(controls.Keyboard.K_Accelerate.ReadValue<float>()))
                            acceleration = (controls.Gamepad.G_Accelerate.ReadValue<float>() - GetDeadzone()) / (1 - GetDeadzone());
                        else
                            acceleration = (controls.Keyboard.K_Accelerate.ReadValue<float>() - GetDeadzone()) / (1 - GetDeadzone());
                    }
                    else
                        acceleration = 0;

                    //brakes
                    if (controls.Gamepad.G_Brake.ReadValue<float>() > GetDeadzone() ||
                        controls.Keyboard.K_Brake.ReadValue<float>() > 0.1f)
                    {
                        float keyboardBrakes = controls.Keyboard.K_Brake.ReadValue<float>() > 0.1f ? 1 : 0;
                        float gamepadBrakes = controls.Gamepad.G_Brake.ReadValue<float>() > GetDeadzone() ? (controls.Gamepad.G_Brake.ReadValue<float>() - GetDeadzone()) / (1 - GetDeadzone()) : 0;

                        if (gamepadBrakes > keyboardBrakes)
                        {
                            brakes = gamepadBrakes;
                        }

                        else
                            brakes = keyboardBrakes;
                    }
                    else
                        brakes = 0;

                    //ebrakes
                    if (controls.Gamepad.G_EBrake.ReadValue<float>() > GetDeadzone() ||
                        controls.Keyboard.K_EBrake.ReadValue<float>() > 0.1f)
                    {
                        float keyboardBrakes = controls.Keyboard.K_EBrake.ReadValue<float>() > 0.1f ? 1 : 0;
                        float gamepadBrakes = controls.Gamepad.G_EBrake.ReadValue<float>() > GetDeadzone() ? (controls.Gamepad.G_EBrake.ReadValue<float>() - GetDeadzone()) / (1 - GetDeadzone()) : 0;

                        if (gamepadBrakes > keyboardBrakes)
                        {
                            ebrake = gamepadBrakes;
                        }

                        else
                            ebrake = keyboardBrakes;
                    }
                    else
                        ebrake = 0;

                    //boost
                    if (controls.Gamepad.G_Boost.ReadValue<float>() > GetDeadzone() ||
                        controls.Keyboard.K_Boost.ReadValue<float>() > GetDeadzone())
                    {
                        if (boost)
                            isBoost = true;
                        else
                            boost = true;
                    }
                    else
                    {
                        boost = false;
                        isBoost = false;
                    }


                    if (controls.Gamepad.G_Respawn.ReadValue<float>() > GetDeadzone() ||
                        controls.Keyboard.K_Respawn.ReadValue<float>() > GetDeadzone())
                    {
                        if (respawn)
                            isRespawnPressed = true;
                        else
                            respawn = true;
                    }
                    else
                    {
                        respawn = false;
                        isRespawnPressed = false;
                    }

                    if (controls.Gamepad.G_Start.ReadValue<float>() > GetDeadzone() ||
                        controls.Keyboard.K_Start.ReadValue<float>() > GetDeadzone())
                    {
                        if (start)
                            isStart = true;
                        else
                            start = true;
                    }
                    else
                    {
                        start = false;
                        isStart = false;
                    }

                    if (isGrounded)
                    {
                        if (scanning)
                        {
                            float assistance = 0.01f;
                            rb.velocity = Vector3.RotateTowards(rb.velocity, Vector3.Project(rb.velocity, correctDirection[0]), assistance, 0);
                            //rb.rotation = Quaternion.SlerpUnclamped(rb.rotation, Quaternion.LookRotation(correctDirection[0], rb.rotation * Vector3.up), 0.9f);
                            rb.velocity = boost ? rb.velocity.normalized * 275 : Vector3.ClampMagnitude(rb.velocity, 150);
                        }
                    }

                    //data collection
                    //    if (collectingData)
                    //    {
                    //        if (carControl.isGrounded())
                    //        {
                    //            Vector3 rotationDif = Quaternion.Inverse(Quaternion.LookRotation(rb.rotation * Vector3.forward, rb.rotation * Vector3.up)) * (correctDirection[1].normalized - rb.rotation * Vector3.forward);
                    //            float rotationDifAngle;
                    //            if (rotationDif.x != 0 && rotationDif.z != 0)
                    //            {
                    //                rotationDifAngle = Mathf.Rad2Deg * Mathf.Atan2(rotationDif.x, rotationDif.z) - 90;
                    //                if (rotationDif.x < 0)
                    //                    rotationDifAngle += 180;
                    //            }
                    //            else
                    //            {
                    //                rotationDifAngle = 0;
                    //            }

                    //            Vector3 speedDif = Quaternion.Inverse(Quaternion.LookRotation(rb.velocity, rb.rotation * Vector3.up)) * (correctDirection[1].normalized - rb.velocity.normalized);
                    //            float speedDifAngle;
                    //            if (speedDif.x != 0 && speedDif.z != 0)
                    //            {
                    //                speedDifAngle = Mathf.Rad2Deg * Mathf.Atan2(speedDif.x, speedDif.z) - 90;
                    //                if (speedDif.x < 0)
                    //                    speedDifAngle += 180;
                    //            }
                    //            else
                    //            {
                    //                speedDifAngle = 0;
                    //            }


                    //            Vector3 trackCurve = (Quaternion.Inverse(Quaternion.LookRotation(correctDirection[0], correctNormal[0])) * (correctDirection[1].normalized - correctDirection[0])).normalized;
                    //            Vector3 trackCurve2 = Vector3.ProjectOnPlane(Quaternion.Inverse(Quaternion.LookRotation(correctDirection[2], correctNormal[2])) * (correctDirection[3].normalized - correctDirection[2].normalized), Vector3.up).normalized;
                    //            Vector3 speedCurve = (Quaternion.Inverse(Quaternion.LookRotation(rb.velocity.normalized, rb.rotation * Vector3.up)) * (rb.velocity.normalized - lastV.normalized)).normalized;



                    //            if (trackCurve.x != 0 && trackCurve.z != 0)
                    //            {
                    //                trackCurveAngle = Mathf.Rad2Deg * Mathf.Atan2(trackCurve.x, trackCurve.z) - 90;
                    //                if (trackCurve.x < 0)
                    //                    trackCurveAngle += 180;
                    //            }
                    //            else
                    //            {
                    //                trackCurveAngle = 0;
                    //            }
                    //            if (trackCurve2.x != 0 && trackCurve2.z != 0)
                    //            {
                    //                trackCurveAngle2 = Mathf.Rad2Deg * Mathf.Atan2(trackCurve2.x, trackCurve2.z) - 90;
                    //                if (trackCurve2.x < 0)
                    //                    trackCurveAngle2 += 180;
                    //            }
                    //            else
                    //            {
                    //                trackCurveAngle2 = 0;
                    //            }
                    //            if (speedCurve.x != 0 && speedCurve.z != 0)
                    //            {
                    //                speedCurveAngle = Mathf.Rad2Deg * Mathf.Atan2(speedCurve.x, speedCurve.z) - 90;
                    //                if (speedCurve.x < 0)
                    //                    speedCurveAngle += 180;
                    //            }
                    //            else
                    //            {
                    //                speedCurveAngle = 0;
                    //            }

                    //            List<float> temp;
                    //            if (Time.time > timestamp)
                    //            {
                    //                timestamp = Time.time + 0.25f;

                    //                temp = new List<float>();
                    //                temp.Add(trackCurveAngle);
                    //                temp.Add(trackCurveAngle2);
                    //                temp.Add(rotationDifAngle);
                    //                temp.Add(speedDifAngle);
                    //                temp.Add(rb.velocity.magnitude);
                    //                temp.Add(carControl.GetDriftAngle());
                    //                temp.Add(car.GetTraction());
                    //                temp.Add(car.GetHandling());
                    //                temp.Add(car.GetDHandling());
                    //                temp.Add(GetSteering());

                    //                drivingData.Add(temp);
                    //            }
                    //        }

                    //        if (Keyboard.current[Key.P].wasPressedThisFrame)
                    //        {
                    //            WriteDrivingDataToFile();
                    //            Debug.Log("Wrote data to file");
                    //        }
                    //    }

                }
                else if (inputType == InputType.COMPUTER)
                {
                    //first of all, let me exit
                    if ((controls.Keyboard.K_Start.ReadValue<float>() > GetDeadzone()) || (controls.Gamepad.G_Start.ReadValue<float>() > GetDeadzone()))
                    {
                        if (start)
                            isStart = true;
                        else
                            start = true;
                    }
                    else
                    {
                        start = false;
                        isStart = false;
                    }

                    //and let it respawn when it finds itself out of bounds
                    //if (carAI.isOutOfBounds())
                    //{
                    //    if (respawn)
                    //        isRespawnPressed = true;
                    //    else
                    //        respawn = true;
                    //}
                    //else
                    //{
                    //    respawn = false;
                    //    isRespawnPressed = false;
                    //}



                    //accelerating and backing up
                    if (rb.velocity.magnitude > 15)
                    {
                        if (timestamp > 120)
                        {
                            backingUp = true;
                        }
                        else if (timestamp <= 0)
                        {
                            timestamp = 0;
                            backingUp = false;
                        }

                    }
                    else if (carControl.isEnabled())
                    {
                        timestamp += 60 * Time.deltaTime;
                    }

                    if (backingUp)
                    {
                        acceleration = 0;
                        brakes = 1;
                        timestamp -= 180 * Time.deltaTime;
                    }
                    else
                    {
                        acceleration = 1;
                        brakes = 0;
                    }


                    //steering
                    if (carControl.isEnabled())
                    {
                        //Debug.Log("Correct Direction: " + correctDirection[0]);

                        //perfect pathing
                        if (isGrounded)
                        {
                            if (scanning)
                            {
                                float assistance = 15f;
                                float speed = 100;
                                rb.velocity = Vector3.RotateTowards(rb.velocity, Vector3.Project(rb.velocity, correctDirection[0]), assistance, 0);
                                rb.rotation = Quaternion.SlerpUnclamped(rb.rotation, Quaternion.LookRotation(correctDirection[0], rb.rotation * Vector3.up), 0.9f);


                                if (isGrounded)
                                {
                                    if (Input.GetKey(KeyCode.UpArrow) || (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().y > 0.1f))
                                        speed += 50;
                                    else if (Input.GetKey(KeyCode.DownArrow) || (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().y < -0.1f))
                                        speed -= 50;

                                    if (Vector3.Angle(rb.velocity, rb.rotation * Vector3.forward) < 120)
                                    {
                                        rb.velocity = rb.velocity.normalized * speed;
                                    }
                                    else
                                    {
                                        rb.velocity = correctDirection[0] * speed;
                                    }
                                }
                            }
                            else
                            {

                                float assistance = rubberband / 100;
                                if (0.5f + assistance * 0.5f > 0)
                                {
                                    rb.velocity = Vector3.RotateTowards(rb.velocity, Vector3.Project(rb.velocity, correctDirection[0]), (0.5f + assistance * 0.5f) * Mathf.Deg2Rad * 60 * Time.deltaTime, 0);
                                }

                                //rubberbanding
                                if (carControl.isEnabled())
                                {
                                    //rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(correctDirection[0], rb.rotation * Vector3.up), 0.1f * 60 * Time.deltaTime));

                                    //if (assistance > 0)
                                    //{
                                    //    rb.velocity += correctDirection[1].normalized * (assistance) * 60 * Time.deltaTime;
                                    //}
                                    //else
                                    //{
                                    //    rb.velocity += correctDirection[1].normalized * (assistance * 0.5f) * 60 * Time.deltaTime;
                                    //}

                                    carControl.SetMaxSpeedModifier(rubberband * 0.5f);
                                    if (rubberband > 0)
                                    {
                                        carControl.SetAccModifier(rubberband * 0.25f);
                                    }

                                }
                            }
                        }

                        Vector3 projectedVelocity = Vector3.ProjectOnPlane(rb.velocity, rb.rotation * Vector3.up);
                        //drift support
                        if (Mathf.Abs(Vector3.Angle(correctDirection[0], projectedVelocity)) < 89f)
                        {
                            //Debug.Log("difference: " + (Quaternion.Inverse(rb.rotation) * (rb.velocity.normalized - correctDirection[1].normalized)));
                            //Vector3 trackCurve = Quaternion.Inverse(rb.rotation) * (correctDirection[1].normalized - correctDirection[0].normalized);

                            Quaternion difference = Quaternion.AngleAxis(-Vector3.Angle(rb.rotation * Vector3.up, correctNormal[0]), Vector3.Cross(rb.rotation * Vector3.up, correctNormal[0]));
                            Vector3 rotationDif = Quaternion.Inverse(Quaternion.LookRotation(rb.rotation * Vector3.forward, rb.rotation * Vector3.up)) * (difference * correctDirection[0].normalized - rb.rotation * Vector3.forward);

                            float rotationDifAngle;
                            if (rotationDif.x != 0 && rotationDif.z != 0)
                            {
                                rotationDifAngle = Mathf.Rad2Deg * Mathf.Atan2(rotationDif.x, rotationDif.z) - 90;
                                if (rotationDif.x < 0)
                                    rotationDifAngle += 180;
                            }
                            else
                            {
                                rotationDifAngle = 0;
                            }

                            difference = Quaternion.AngleAxis(-Vector3.Angle(rb.rotation * Vector3.up, correctNormal[1]), Vector3.Cross(rb.rotation * Vector3.up, correctNormal[1]));
                            Vector3 speedDif = Quaternion.Inverse(Quaternion.LookRotation(projectedVelocity, rb.rotation * Vector3.up)) * (difference * correctDirection[1].normalized - projectedVelocity.normalized);

                            float speedDifAngle;
                            if (speedDif.x != 0 && speedDif.z != 0)
                            {
                                speedDifAngle = Mathf.Rad2Deg * Mathf.Atan2(speedDif.x, speedDif.z) - 90;
                                if (speedDif.x < 0)
                                    speedDifAngle += 180;
                            }
                            else
                            {
                                speedDifAngle = 0;
                            }

                            difference = Quaternion.AngleAxis(-Vector3.Angle(correctNormal[0], correctNormal[1]), Vector3.Cross(correctNormal[0], correctNormal[1]));

                            Vector3 trackCurve = (Quaternion.Inverse(Quaternion.LookRotation(correctDirection[0], correctNormal[0])) * (difference * correctDirection[1].normalized - correctDirection[0].normalized)).normalized;

                            difference = Quaternion.AngleAxis(-Vector3.Angle(correctNormal[2], correctNormal[3]), Vector3.Cross(correctNormal[2], correctNormal[3]));
                            Vector3 trackCurve2 = (Quaternion.Inverse(Quaternion.LookRotation(correctDirection[2], correctNormal[2])) * (difference * correctDirection[3].normalized - correctDirection[2].normalized)).normalized;
                            Vector3 speedCurve = (Quaternion.Inverse(Quaternion.LookRotation(projectedVelocity.normalized, rb.rotation * Vector3.up)) * (rb.velocity.normalized - lastV.normalized)).normalized;


                            float tempAngle = 0;
                            if (trackCurve.x != 0 && trackCurve.z != 0)
                            {
                                tempAngle = Mathf.Rad2Deg * Mathf.Atan2(trackCurve.x, trackCurve.z) - 90;
                                if (trackCurve.x < 0)
                                    tempAngle += 180;
                            }
                            else
                            {
                                tempAngle = 0;
                            }
                            trackCurveAngle = Mathf.Lerp(trackCurveAngle, tempAngle, 0.2f * 60 * Time.deltaTime);

                            if (trackCurve2.x != 0 && trackCurve2.z != 0)
                            {
                                tempAngle = Mathf.Rad2Deg * Mathf.Atan2(trackCurve2.x, trackCurve2.z) - 90;
                                if (trackCurve2.x < 0)
                                    tempAngle += 180;
                            }
                            else
                            {
                                tempAngle = 0;
                            }
                            trackCurveAngle2 = Mathf.Lerp(trackCurveAngle2, tempAngle, 0.2f * 60 * Time.deltaTime);

                            if (speedCurve.x != 0 && speedCurve.z != 0)
                            {
                                tempAngle = Mathf.Rad2Deg * Mathf.Atan2(speedCurve.x, speedCurve.z) - 90;
                                if (speedCurve.x < 0)
                                    tempAngle += 180;
                            }
                            else
                            {
                                tempAngle = 0;
                            }
                            speedCurveAngle = Mathf.Lerp(speedCurveAngle, tempAngle, 0.2f * 60 * Time.deltaTime);



                            //trackCurveAngle *= 10;
                            //speedCurveAngle *= 10;
                            //Debug.Log("trackCurveCurve: " + trackCurveAngle + " " + trackCurveAngle2);
                            //drifting with perfect pathing
                            if (ebrake <= 0.1f)
                            {
                                //rb.rotation = Quaternion.AngleAxis(trackCurveAngle * Mathf.Clamp01(rb.velocity.magnitude / 300), rb.rotation * Vector3.up) * rb.rotation;
                            }

                            float speedScale = Mathf.Max(projectedVelocity.magnitude / 325, 0.1f);
                            bool exitingCorner = false;
                            rawSteering = 0;

                            //ebraking at the ends of corners (no longer effective
                            //if (Vector3.Angle(projectedVelocity, correctDirection[0]) < 5f &&
                            //    Vector3.Angle(rb.rotation * Vector3.forward, projectedVelocity) > 15f &&
                            //    trackCurveAngle < 0.1f)
                            //{
                            //    ebrake = true;
                            //    exitingCorner = true;
                            //}

                            bool incomingTurn = false;

                            ebrake = 0;
                            brakes = 0;

                            //braking at  corners
                            if (GameSettings.difficulty > 0)
                            {

                                if (Mathf.Abs(trackCurveAngle) * speedScale / car.GetHandling() < 2.5f && !(Vector3.Angle(projectedVelocity, rb.rotation * Vector3.forward) > 15f && Mathf.Abs(speedCurveAngle - trackCurveAngle) < 1f)
                                /*|| transform.InverseTransformVector(correctDirection[1]).x < transform.InverseTransformVector(correctDirection[0]).x - 0.04f*/)
                                {

                                    //stable handling

                                    //if there's a turn coming and is extremely tight, brake and stop accelerating
                                    if (Mathf.Abs(trackCurveAngle2) * speedScale / car.GetHandling() > 8f)
                                    {
                                        ebrake = 1;
                                        brakes = 1f;
                                        acceleration = 0;
                                        incomingTurn = true;
                                    }
                                    else if (Mathf.Abs(trackCurveAngle2) * speedScale / car.GetHandling() > 6.5f)
                                    {
                                        ebrake = 1f;
                                        brakes = 1f;
                                        incomingTurn = true;
                                    }
                                    //if there's a turn coming and is tight enough, brake
                                    else if (Mathf.Abs(trackCurveAngle2) * speedScale / car.GetHandling() > 4.5f)
                                    {

                                        ebrake = 1;
                                        incomingTurn = true;
                                    }
                                }
                            }


                            //after ebraking conditions, then steer (steering checks if you're ebraking
                            //if (!carControl.isGrounded())
                            //{
                            //    if (Vector3.Angle(Vector3.ProjectOnPlane(rb.velocity, transform.up), transform.forward) > 15f)
                            //    {
                            //        rawSteering += Mathf.Lerp(speedDifAngle, rotationDifAngle, 1f) / car.GetDHandling() * speedScale;
                            //    }
                            //    else
                            //    {
                            //        rawSteering += Mathf.Lerp(speedDifAngle, rotationDifAngle, 1f) / car.GetHandling() * speedScale;
                            //    }
                            //}
                            //else 
                            if (Vector3.Angle(projectedVelocity, rb.rotation * Vector3.forward) > 15f)
                            {

                                //drift handling
                                //on corners
                                if (Mathf.Abs(trackCurveAngle) > 0.5f && Mathf.Abs(car.GetOversteer()) < 0.2f)
                                {
                                    rawSteering += Mathf.Lerp(speedDifAngle, rotationDifAngle, 0.25f) * speedScale * 0.66f / car.GetHandling();
                                }
                                else if (Mathf.Abs(trackCurveAngle) > 0.5f && !exitingCorner)
                                {
                                    rawSteering += Mathf.Lerp(speedDifAngle, rotationDifAngle, 0.2f) * speedScale * 0.66f / car.GetHandling();
                                }
                                //on straightaways
                                else
                                {
                                    rawSteering += Mathf.Lerp(speedDifAngle, rotationDifAngle, 0.5f) / car.GetHandling() * speedScale * 0.66f;

                                    if (Vector3.Angle(rb.rotation * Vector3.forward, correctDirection[0]) < Mathf.Log(0.1f / rb.angularVelocity.magnitude, 0.82f) && car.GetOversteer() < 0.1f)
                                    {
                                        rawSteering = 0;
                                    }

                                }
                                rawSteering += (trackCurveAngle + trackCurveAngle2) * 0.5f * speedScale * 0.33f / car.GetHandling();

                                //rawSteering += Mathf.Pow((trackCurveAngle + trackCurveAngle2) * 0.5f, 2f) / Mathf.Clamp(car.GetDHandling() * carControl.GetFinalTraction() * 10, 0.25f, 5f) * speedScale * 0.5f;
                            }
                            else
                            {
                                //grip
                                if (Mathf.Abs(trackCurveAngle2) > 0.5f && Mathf.Abs(car.GetOversteer()) < 0.2f)
                                {
                                    rawSteering += Mathf.Lerp(speedDifAngle, rotationDifAngle, 0.5f) * speedScale / car.GetHandling();
                                }
                                else if (Mathf.Abs(trackCurveAngle2) > 0.5f)
                                {
                                    rawSteering += Mathf.Lerp(speedDifAngle, rotationDifAngle, 0.5f) * speedScale / car.GetHandling();
                                }
                                else
                                {
                                    rawSteering += Mathf.Lerp(speedDifAngle, rotationDifAngle, 0.75f) * speedScale / car.GetHandling();

                                    if (Vector3.Angle(rb.rotation * Vector3.forward, correctDirection[0]) < Mathf.Log(0.1f / (Mathf.Rad2Deg * rb.angularVelocity.magnitude), 0.82f) && Mathf.Abs(trackCurveAngle) < 0.5f)
                                    {
                                        rawSteering = 0;
                                    }
                                }
                            }
                            if (incomingTurn)
                            {
                                rawSteering = rawSteering * 0.75f + trackCurveAngle2 / (car.GetHandling()) * speedScale * 0.4f;
                            }
                        }
                        else
                        {
                            rawSteering = transform.InverseTransformVector(correctDirection[0]).x / Mathf.Abs(transform.InverseTransformVector(correctDirection[0]).x);
                        }
                    }

                    Debug.DrawRay(rb.position + rb.rotation * Vector3.up * 1f, correctDirection[0], Color.magenta, Time.deltaTime * 2f);
                    Debug.DrawRay(rb.position + rb.rotation * Vector3.up * 1f + correctDirection[0], correctDirection[1], Color.white, Time.deltaTime * 2f);
                    Debug.DrawRay(rb.position + rb.rotation * Vector3.up * 1f + correctDirection[0] + correctDirection[1], correctDirection[2], Color.white, Time.deltaTime * 2f);

                    if (GameSettings.difficulty >= 2)
                    {
                        if (boosting)
                        {
                            if (carControl.GetBoostMeter() > 0 && brakes == 0)
                            {
                                boost = true;
                            }
                            else
                            {
                                boosting = false;
                                boost = false;
                            }
                        }
                        else
                        {
                            if ((carControl.GetBoostMeter() > 0.8f - (GameSettings.difficulty - 2) * 0.3f && brakes < 0.01f) ||
                                (carControl.GetBoostMeter() > 0.5f && !carControl.isDrifting()))
                            {
                                boosting = true;
                                boost = true;
                            }
                            else
                            {
                                boost = false;
                            }
                        }
                    }
                    else
                        boost = false;



                    if (scanning)
                        rawSteering = 0;

                    steering = Mathf.Clamp(rawSteering, -1, 1);

                    lastV = rb.velocity;

                }
                else if (inputType == InputType.ONLINE)
                {
                    Debug.LogWarning("ONLINE not implemented yet");
                }
                else if (inputType == InputType.REPLAY)
                {

                    if (GameSettings.gamemode == GameSettings.GameMode.REPLAY)
                    {
                        //you can press start when you want
                        if ((controls.Keyboard.K_Start.ReadValue<float>() > GetDeadzone()) || (controls.Gamepad.G_Start.ReadValue<float>() > GetDeadzone()))
                        {
                            if (start)
                                isStart = true;
                            else
                                start = true;
                        }
                        else
                        {
                            start = false;
                            isStart = false;
                        }
                    }

                }
                else if (inputType == InputType.NONE)
                {
                    Debug.Log("No inputs being accepted");
                }
            }
        }

        private void Reset()
        {
            if (inputType == InputType.COMPUTER)
            {
                correctDirection[0] = Vector3.zero;
                correctDirection[1] = Vector3.zero;
                car = GetComponent<CarGetter>().GetCar();
            }
        }

        private IEnumerator AIBackup()
        {
            Debug.Log("Backing Up");
            float tempstamp = Time.time;
            while (Time.time - tempstamp < 0.5f)
            {
                acceleration = 0;
                brakes = 1;
                ebrake = 0f;
                if (!carControl.isGoingForward())
                    steering = -steering;
                yield return null;
            }
            tempstamp = Time.time;
            while (Time.time - tempstamp < 0.5f)
            {
                acceleration = 1;
                brakes = 0;
                yield return null;
            }
            //Debug.Log("Done Backing Up");
        }

        public void Finish()
        {
            if (inputType == InputType.PLAYER)
            {
                inputType = InputType.COMPUTER;
            }
        }

        private void WriteDrivingDataToFile()
        {
            string directory = "/DrivingData/";
            string dir = Application.streamingAssetsPath + directory;
            string fullDir = dir + "drivingData.csv";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string output;
            if (File.Exists(fullDir))
            {
                StreamReader sr = File.OpenText(fullDir);
                output = sr.ReadToEnd();
                sr.Close();
            }
            else
            {
                output = "trackCurveAngle,trackCurveAngle2,rotationDifAngle,speedDifAngle,velocity,driftAngle,traction,handling,d-handling,steering\n";
            }
            StreamWriter stream = File.CreateText(fullDir);

            for (int i = 0; i < drivingData.Count; i++)
            {
                for (int o = 0; o < drivingData[i].Count; o++)
                {
                    output += drivingData[i][o].ToString();

                    if (o < drivingData[i].Count - 1)
                        output += ",";
                }
                output += "\n";
            }

            stream.Write(output);
            stream.Close();
        }
    }
}
