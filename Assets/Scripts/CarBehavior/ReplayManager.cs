using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReplayManager : MonoBehaviour
{
    CarInputHandler inputHandler;
    CarControl carControl;
    CarGetter carGetter;
    Rigidbody rb;
    PlayerInfo playerInfo;

    //state of the manager
    public bool observing = false;
    public bool replaying = false;

    //Replay
    List<ReplayFrame> recording;
    public Replay currentReplay;
    string track;
    float startTime;
    float bestTime;

    //replay accessibles
    public ReplayFrame currentFrame;
    Vector3 lastPosition;

    //event to say that the replay is done
    public event Action<GameObject, float, float[]> FinishedReplay;

    bool started = false;
    bool finished = false;

    // Start is called before the first frame update
    public void CustomStart()
    {
        inputHandler = GetComponent<CarInputHandler>();
        carControl = GetComponent<CarControl>();
        carGetter = GetComponent<CarGetter>();
        rb = GetComponent<Rigidbody>();
        playerInfo = GetComponent<PlayerInfo>();

        startTime = Time.time;

        if (replaying == true)
        {
            inputHandler.inputType = CarInputHandler.InputType.REPLAY;
            //currentReplay = ReplayStorage.lastReplay; //this will now be given to the manager by the spawner
            observing = false;
        }
        if (observing == true)
        {
            recording = new List<ReplayFrame>();
            track = ScreenManager.current.GetSceneName();
        }

        //load the best time
        
        if (PlayerPrefs.HasKey(Application.version + "-" + ScreenManager.current.GetSceneName() + "-" + "tier" + carGetter.GetCar().GetTier() + "-bestTime"))
        {
            bestTime = PlayerPrefs.GetFloat(Application.version + "-" + ScreenManager.current.GetSceneName() + "-" + "tier" + carGetter.GetCar().GetTier() + "-bestTime", -1);
        }

        lastPosition = rb.position;

        started = false;

        PlayerInfo.FinishedRace += StopAndSaveReplay;
    }

    public void OnDestroy()
    {
        PlayerInfo.FinishedRace -= StopAndSaveReplay;
    }

    private void FixedUpdate()
    {
        if (replaying == true && currentReplay != null)
        {
            observing = false;

            ReplayFrame offsetFrame;
            if (!started)
            {
                currentFrame = currentReplay.GetFrameAtTime(0);
                offsetFrame = currentReplay.GetFrameAtTime(0);
            }
            else
            {
                currentFrame = currentReplay.GetFrameAtTime(Time.time - startTime);
                offsetFrame = currentReplay.GetFrameAtTime((Time.time + 0.01f) - startTime);
            }

            rb.MovePosition(currentFrame.GetPosition());
            rb.MoveRotation(currentFrame.GetRotation().normalized);
            //rb.velocity = (rb.position - lastPosition) / Time.deltaTime;
            rb.velocity = (offsetFrame.GetPosition() - currentFrame.GetPosition()) / 0.01f;
            //rb.velocity = Vector3.zero;

            if (Time.time - startTime > currentReplay.GetTotalTime() && !finished)
            {
                //Debug.Log($"Time: {currentReplay.GetTotalTime()}, All Laps: {currentReplay.GetAllLaps()}");
                FinishedReplay?.Invoke(gameObject, currentReplay.GetTotalTime(), currentReplay.GetAllLaps());
                finished = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (carControl.isEnabled() && !started)
        {
            startTime = Time.time;
            started = true;

            Debug.Log("Replay Manager Started");
        }

        if (observing)
        {

            if (started)
            {
                if (Time.timeScale != 0)
                {
                    recording.Add(new ReplayFrame(Time.time - startTime, transform.position, transform.rotation, inputHandler.GetAcceleration(), inputHandler.GetSteering(), inputHandler.GetBrakes(), inputHandler.GetEBrakes(), carControl.isBoosting(), carControl.isDrifting(), inputHandler.GetReset()));
                }
            }
        }

        if (replaying)
        {
            inputHandler.SetAcceleration(currentFrame.GetAcceleration());
            inputHandler.SetSteering(currentFrame.GetSteering());
            inputHandler.SetBrakes(currentFrame.GetBrakes());
            inputHandler.SetEBrakes(currentFrame.GetEBrake());
            inputHandler.SetBoost(currentFrame.GetBoost());
            inputHandler.SetReset(currentFrame.GetRespawn());
        }
    }

    void StopAndSaveReplay(GameObject who, float totalTime, float[] allLaps)
    {
        if (who == this.gameObject && observing == true)
        {
            observing = false;

            currentReplay = new Replay(recording, ScreenManager.current.GetSceneName(), carGetter.GetCar(), totalTime, allLaps);

            //here's where I would save onto file, but i need to test it first
            ReplayStorage.lastReplay = currentReplay;
            Debug.Log("Replay Temporarily Saved");

            //save if it is better than the playerpref best time
            if ((bestTime <= 0.1f || totalTime < bestTime) && !(carGetter.GetCar().GetName() == "random"))
            {
                ReplayStorage.SaveReplay(Application.version + "-" + ScreenManager.current.GetSceneName() + "-" + "tier" + carGetter.GetCar().GetTier() + "-bestTime", currentReplay);
                Debug.Log("Replay Saved Persistently");
            }
        }
    }
}
