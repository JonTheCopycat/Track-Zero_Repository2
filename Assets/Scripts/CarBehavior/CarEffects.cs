using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cars;

namespace CarBehaviour
{
    public class CarEffects : MonoBehaviour
    {
        [SerializeField]
        private TrailRenderer[] WheelTrails;
        [SerializeField]
        private ParticleSystem[] WheelSparks;

        private AudioScriptableObject soundProfile;
        public AudioSource carSource;
        public AudioSource otherSource;

        private BoosterBehavior[] Boosters;
        private GameObject[] FrontTires;
        private GameObject[] RearTires;
        private GameObject[] AllTires;
        private MeshRenderer CarBodyRenderer;

        [SerializeField]
        private ParticleSystem SpeedParticles;

        [SerializeField]
        private Material GhostMaterial;

        CarControl carControl;
        CarInputHandler inputHandler;
        CarGetter carGetter;
        PlayerInfo playerInfo;

        Car car;
        GearBox gearBox;
        float wheelVelocity;
        float baseWheelPosition;
        float[] baseBoosterSize;
        private float sizeOnScreen;

        bool started = false;

        // Start is called before the first frame update
        void OnEnable()
        {
            carControl = GetComponent<CarControl>();
            carGetter = GetComponent<CarGetter>();
            inputHandler = GetComponent<CarInputHandler>();
            playerInfo = GetComponent<PlayerInfo>();

            carSource.Stop();
            otherSource.Stop();
        }

        public void CustomStart()
        {
            car = carGetter.GetCar();

            FrontTires = carGetter.GetCarModel().FrontTires;
            RearTires = carGetter.GetCarModel().RearTires;

            AllTires = new GameObject[FrontTires.Length + RearTires.Length];
            for (int i = 0; i < RearTires.Length && i < WheelTrails.Length; i++)
            {
                AllTires[i] = RearTires[i];
            }
            for (int i = 0; i < FrontTires.Length && RearTires.Length - 1 + i < WheelTrails.Length; i++)
            {
                AllTires[RearTires.Length + i] = FrontTires[i];
            }

            Boosters = carGetter.GetCarModel().Boosters;
            CarBodyRenderer = carGetter.GetCarModel().CarBodyRenderer;
            soundProfile = carGetter.GetSoundProfile();

            for (int i = 0; i < AllTires.Length && i < WheelTrails.Length; i++)
            {
                WheelTrails[i].transform.position = AllTires[i].transform.position + AllTires[i].transform.rotation *
                    new Vector3(-AllTires[i].transform.lossyScale.x * 0.75f, -AllTires[i].transform.lossyScale.x * 0.5f);
                WheelTrails[i].transform.localScale = AllTires[i].transform.lossyScale * 2.5f;
            }

            WheelSparks[0].transform.position = RearTires[0].transform.position + RearTires[0].transform.rotation * new Vector3(-RearTires[0].transform.localScale.x * 0.75f, -RearTires[0].transform.localScale.y);
            WheelSparks[0].transform.rotation = RearTires[0].transform.rotation;

            WheelSparks[1].transform.position = RearTires[1].transform.position + RearTires[1].transform.rotation * new Vector3(-RearTires[1].transform.localScale.x * 0.75f, -RearTires[1].transform.localScale.y);
            WheelSparks[1].transform.rotation = RearTires[1].transform.rotation;

            StopSparks();

            baseWheelPosition = FrontTires[0].transform.localRotation.eulerAngles.y;

            if (inputHandler.inputType == CarInputHandler.InputType.REPLAY)
            {
                //ghost color
                Material[] mats = new Material[CarBodyRenderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = GhostMaterial;
                    mats[i].color = new Color(1, 1, 1, 0.25f);
                }
                CarBodyRenderer.materials = mats;

            }
            else
            {
                CarBodyRenderer.material.color = car.GetColor();
            }

            gearBox = new GearBox(carControl.maxSpeed, 6, 8500);
            sizeOnScreen = 16;
            Debug.Log("CarEffects");
            started = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (started)
            {
                //color per terrain
                if (carControl.isDrivingOnDirt())
                {
                    ChangeTiresToColor(new Color(1, 0.6f, 0) * 5);
                }
                else if (carControl.isDrivingOnIce())
                {
                    ChangeTiresToColor(new Color(0, 1, 1) * 5);

                }
                else if (carControl.isDrivingOnTurbo())
                {
                    ChangeTiresToColor(new Color(0.9f, 0.5f, 1f) * 5);
                }
                else
                {
                    ChangeTiresToColor(Color.white * 0.8f);
                }

                //wheel trail color
                if (carControl.isBoosting())
                {
                    ChangeLightsToColor(new Color(1f, 0.8f, 0.3f), true);
                    ChangeBoosterSize(1.5f);

                    otherSource.clip = soundProfile.boostOn;
                    otherSource.loop = true;
                    otherSource.pitch = 1;
                    if (playerInfo.main)
                        otherSource.volume = 0.1f * GameSettings.masterVolume * GameSettings.sfxVolume;
                    else
                        otherSource.volume = 0.1f * GameSettings.masterVolume * GameSettings.sfxVolume;
                    carSource.reverbZoneMix = 2;
                    if (carControl.isBoostStart())
                    {
                        otherSource.Stop();
                        otherSource.PlayOneShot(soundProfile.boostStart);
                        otherSource.PlayScheduled(AudioSettings.dspTime + soundProfile.boostStart.length);
                    }

                }
                else if (carControl.isDrifting() || carControl.isSliding() || carControl.isBraking() || carControl.isOnTurbo())
                {
                    Color temp;
                    if (carControl.isDrifting())
                    {
                        if (carControl.GetDriftAngle() > CarControl.fullDriftAngle)
                            temp = Color.green * 2;
                        else
                            temp = new Color(0f, 0.5f, 0f, 0.5f);
                    }
                    else
                    {
                        temp = new Color(0f, 0f, 0f, 0f);
                    }


                    //ChangeLightsToColor(Color.green * 10, true);
                    //this is basically a 4d lerp, shifting between the four colors of drifting, sliding, braking, and turbo
                    
                    Color brakeLerp = Color.Lerp(temp, new Color(1, 0, 0), inputHandler.GetBrakes());
                    Color turboLerp = Color.Lerp(brakeLerp, new Color(0, 0, 1), inputHandler.GetTurboDown() ? 1 : 0);
                    Color driftLerp = Color.Lerp(turboLerp, new Color(0.8f, 0.8f, 0.8f), inputHandler.GetEBrakes() * 0.66f);
                    ChangeLightsToColor(driftLerp, true); 


                    if (carControl.GetDriftAngle() > 90)
                    {
                        StartSparks();
                    }
                    else if (carControl.GetDriftAngle() > 75)
                    {
                        StartSparks((carControl.GetDriftAngle() - 75) / (90 - 75));
                    }
                    else
                    {
                        StopSparks();
                    }
                }
                else if (!carControl.isGrounded())
                {
                    ChangeLightsToColor(new Color(0.05f, 0.05f, 0.05f), false);
                }
                else if (carControl.isDrivingOnDirt())
                {
                    ChangeLightsToColor(new Color(139, 69, 19) / 255, false);
                }
                else
                {
                    ChangeLightsToColor(Color.white, false);
                }

                if (!carControl.isDrifting())
                {
                    StopSparks();
                }

                //boosting transition to looping audio
                //if (otherSource.time >= otherSource.clip.length - 0.01f)
                //{
                //    otherSource.clip = soundProfile.boostOn;
                //    otherSource.time = 0;
                //    otherSource.loop = true;
                //    otherSource.pitch = 1;
                //    if (playerInfo.main)
                //        otherSource.volume = 0.75f;
                //    else
                //        otherSource.volume = 0.25f;
                //    carSource.reverbZoneMix = 2;
                //    otherSource.Stop();
                //    otherSource.Play();
                //}
                //boosting falloff
                if (!carControl.isBoosting())
                {
                    ChangeBoosterSize(1f);

                    if (otherSource.isPlaying)
                        otherSource.volume -= 1 / 0.4f * Time.deltaTime;

                    if (otherSource.volume <= 0.1f)
                    {
                        otherSource.Stop();
                        otherSource.reverbZoneMix = 1;
                    }
                }

                carSource.clip = soundProfile.carEngine;
                gearBox.UpdateGear(carControl.GetVelocity().magnitude);

                float gearPitch = Mathf.Lerp(0f, 2.5f, gearBox.GetRPM() / gearBox.GetMaxRPM());
                float targetPitch;
                if (carControl.isGrounded())
                {
                    targetPitch = Mathf.Clamp(Mathf.Max(gearPitch, carControl.GetDriftAngle() / 30), 0f, 2.5f);
                }
                else
                {
                    targetPitch = 2.5f;
                }

                if (Time.timeScale >= 0.01f)
                {
                    carSource.loop = true;
                    if (inputHandler.GetAcceleration() > 0)
                    {
                        carSource.volume = playerInfo.main ? GameSettings.masterVolume * GameSettings.engineVolume : 0.5f * GameSettings.masterVolume * GameSettings.engineVolume;
                        carSource.pitch = Mathf.Lerp(carSource.pitch, targetPitch, 0.5f);
                    }
                    else
                    {
                        carSource.volume = 0.8f * (playerInfo.main ? GameSettings.masterVolume * GameSettings.engineVolume : 0.5f * GameSettings.masterVolume * GameSettings.engineVolume);
                        carSource.pitch = Mathf.Lerp(carSource.pitch, 0.5f, 0.5f);
                    }
                    if (!carSource.isPlaying)
                        carSource.Play();
                }
                else
                {
                    carSource.Stop();
                }


                //wheelspin
                if (carControl.isGoingForward())
                    wheelVelocity = carControl.GetVelocity().magnitude / transform.localScale.z;
                else
                    wheelVelocity = -carControl.GetVelocity().magnitude / transform.localScale.z;

                if (carControl.isSliding())
                {
                    SpinWheels(0, wheelVelocity / 2);
                }
                else
                {
                    SpinWheels(wheelVelocity);
                }

                //wheel steering rotation
                float counterSteer = Mathf.Clamp(carControl.GetDriftAngle(), 0, 45);
                if (carControl.isGrounded() && carControl.isDrifting())
                {
                    Steer(inputHandler.GetSteering() * 5);
                }
                else
                {
                    Steer(carControl.GetAngularVelocity() * 10);
                }


                //boosters
                if (inputHandler.GetAcceleration() > 0.01f)
                {
                    for (int i = 0; i < Boosters.Length; i++)
                    {
                        if (!Boosters[i].isOn())
                            Boosters[i].BoostStart();
                    }
                }
                else
                {
                    for (int i = 0; i < Boosters.Length; i++)
                    {
                        if (Boosters[i].isOn())
                            Boosters[i].BoostStop();
                    }
                }
            }
        }

        //private void OnCollisionEnter(Collision collision)
        //{
        //    if (started && Vector3.Angle(transform.up, collision.GetContact(0).normal) > 30f)
        //    {
        //        otherSource.Stop();

        //        otherSource.clip = soundProfile.crash;
        //        otherSource.pitch = 1;
        //        otherSource.volume = 2;
        //        otherSource.Play();
        //    }
        //}

        void ChangeLightsToColor(Color color, bool emit)
        {
            for (int i = 0; i < WheelTrails.Length; i++)
            {
                if (emit)
                {
                    if (playerInfo.main)
                        WheelTrails[i].material.color = new Color(color.r, color.g, color.b, color.a * 0.5f);
                    else
                        WheelTrails[i].material.color = new Color(color.r, color.g, color.b, color.a * 0.25f);
                }
                WheelTrails[i].emitting = emit;
            }

            for (int i = 0; i < FrontTires.Length; i++)
            {
                FrontTires[i].GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
            }

            for (int i = 0; i < RearTires.Length; i++)
            {
                RearTires[i].GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color);
            }
        }

        void ChangeLightsToColor(bool emit)
        {
            for (int i = 0; i < WheelTrails.Length; i++)
            {
                WheelTrails[i].emitting = emit;
            }
        }

        void ChangeTiresToColor(Color color)
        {
            for (int i = 0; i < FrontTires.Length; i++)
            {
                FrontTires[i].GetComponent<MeshRenderer>().material.color = color;
            }

            for (int i = 0; i < RearTires.Length; i++)
            {
                RearTires[i].GetComponent<MeshRenderer>().material.color = color;
            }
        }

        void ChangeBoosterSize(float size)
        {
            Vector3 a;
            Vector3 b;
            Vector3 c;
            Vector3 aa;
            Vector3 bb;

            for (int i = 0; i < Boosters.Length; i++)
            {
                a = Camera.main.WorldToScreenPoint(Boosters[i].transform.position);
                b = new Vector3(a.x, a.y + sizeOnScreen * size, a.z);
                c = Camera.main.WorldToScreenPoint(Boosters[i].transform.position + Camera.main.transform.up * Boosters[i].transform.localScale.y);

                aa = Camera.main.ScreenToWorldPoint(a);
                bb = Camera.main.ScreenToWorldPoint(b);

                Boosters[i].sizeMultiplier = Mathf.Max(size, size * (aa - bb).magnitude);
            }
        }

        void SpinWheels(float angularVelocity)
        {
            SpinWheels(angularVelocity, angularVelocity);
        }

        void SpinWheels(float angularVelocityFront, float angularVelocityRear)
        {
            float wheelRotation = 0;
            for (int i = 0; i < FrontTires.Length; i++)
            {
                wheelRotation = FrontTires[i].transform.eulerAngles.z;
                wheelRotation = (wheelRotation - angularVelocityFront * 60 * Time.deltaTime) % 360;
                FrontTires[i].transform.eulerAngles = /*FrontTires[i].transform.rotation **/
                    new Vector3(FrontTires[i].transform.eulerAngles.x,
                    FrontTires[i].transform.eulerAngles.y,
                    wheelRotation);
            }

            for (int i = 0; i < RearTires.Length; i++)
            {
                wheelRotation = RearTires[i].transform.eulerAngles.z;
                wheelRotation = (wheelRotation - angularVelocityRear * 60 * Time.deltaTime) % 360;
                RearTires[i].transform.eulerAngles = /*RearTires[i].transform.rotation **/
                    new Vector3(RearTires[i].transform.eulerAngles.x,
                    RearTires[i].transform.eulerAngles.y,
                    wheelRotation);
            }
        }

        void StartSparks()
        {
            for (int i = 0; i < WheelSparks.Length; i++)
            {
                WheelSparks[i].transform.localScale = Vector3.one;
                WheelSparks[i].Play();
            }
        }

        void StartSparks(float size)
        {
            for (int i = 0; i < WheelSparks.Length; i++)
            {
                WheelSparks[i].transform.localScale = Vector3.one * size;
                WheelSparks[i].Play();
            }
        }

        void StopSparks()
        {
            for (int i = 0; i < WheelSparks.Length; i++)
            {
                WheelSparks[i].Stop();
            }
        }

        void Steer(float degrees)
        {
            for (int i = 0; i < FrontTires.Length; i++)
            {
                Quaternion newRotation = Quaternion.Euler(
                    FrontTires[i].transform.localRotation.eulerAngles.x,
                    baseWheelPosition + degrees,
                    FrontTires[i].transform.localRotation.eulerAngles.z);
                FrontTires[i].transform.localRotation = Quaternion.Lerp(FrontTires[i].transform.localRotation, newRotation, 0.1f);
            }
        }
    }
}
