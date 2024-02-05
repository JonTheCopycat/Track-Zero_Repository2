using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerInfo : MonoBehaviour
{
    public CheckpointList CheckpointList;
    private bool started = false;

    public bool main = false;
    private bool finished = false;

    private List<float> previousLapTimes = new List<float>();
    private float previousLapTime;
    private float totalTime;
    private float fullStamp;
    private float lapStamp;
    private Vector3 correctDirection;

    private int resetNum;
    private int recordNum;
    private bool finishTriggered;
    private int checkpointsPassed;
    private int lapCount;
    public int totalLaps;

    private Rigidbody rb;
    private CarGetter carGetter;

    public static event Action<GameObject, float> FinishedLap;
    public static event Action<GameObject, float, float[]> FinishedRace;

    public int GetCheckpointsPassed()
    {
        return checkpointsPassed;
    }


    public float GetFullStamp()
    {
        return fullStamp;
    }

    public float GetLapStamp()
    {
        return lapStamp;
    }

    public int GetCurrentLap()
    {
        return lapCount;
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        //CustomStart();
    }

    public void CustomStart()
    {
        rb = GetComponent<Rigidbody>();
        Reset();

        previousLapTime = -1;
        lapCount = 0;

        started = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            
            //track switching
            //if (Input.GetKeyDown(carControl.reset))
            //{
            //    if (Input.GetKeyDown(carControl.reset))
            //        resetNum = 1;
            //    if (Input.GetKeyDown(KeyCode.Alpha2))
            //        resetNum = 2;
            //    if (Input.GetKeyDown(KeyCode.Alpha3))
            //        resetNum = 3;

            //    recordNum = 0;
            //    firstRun = true;
            //    finishTriggered = false;
            //    return;
            //}
            if (recordNum == 0 && resetNum > 0  /*&& carControl.isGrounded*/)
            {
                recordNum = resetNum;
                resetNum = 0;
                //timestamp = Time.time;
            }
            //else if (recordNum > 0 && resetNum == 0 && tracking)
            //{
            //    timeText.text = "Time: \t" + FormatTime(Mathf.Floor((Time.time - fullStamp) * 100) / 100) + "\n" +
            //        "Lap Time: \t" + FormatTime(Mathf.Floor((Time.time - lapStamp) * 100) / 100) + "\n" +
            //        "Lap " + (lapCount + 1) + " / " + totalLaps + "\n\n";
            //    for (int i = Mathf.Clamp(previousLapTimes.Count - 10, 0, int.MaxValue); i < previousLapTimes.Count; i++)
            //    {
            //        timeText.text += "Lap " + (i + 1) + ": " + FormatTime(previousLapTimes[i]) + "\n";
            //    }
            //}
        }
    }

    private void OnTriggerEnter(Collider collision)
    
    {
        Vector3 velocity = Vector3.Project(rb.velocity, collision.transform.rotation * Vector3.forward);
        if (collision.tag.Equals("Finish"))
        {
            if (velocity.normalized == (collision.transform.rotation * Vector3.forward).normalized)
            {
                if (recordNum != 0 && finishTriggered && checkpointsPassed == CheckpointList.list.Count)
                {

                    previousLapTime = Mathf.Floor((Time.time - lapStamp) * 100) / 100;
                    previousLapTimes.Add(previousLapTime);
                    lapCount++;

                    //ingameUI.SendLapData(previousLapTime, gameObject);
                    FinishedLap?.Invoke(gameObject, previousLapTime);
                    if (totalLaps > 0 && lapCount >= totalLaps && !finished)
                    {
                        //Finish
                        //BroadcastMessage("Finish");
                        totalTime = 0;
                        for (int i = 0; i < previousLapTimes.Count; i++)
                        {
                            totalTime += previousLapTimes[i];
                        }

                        FinishedRace?.Invoke(gameObject, totalTime, previousLapTimes.ToArray());
                        SendMessage("Finish");
                        finished = true;
                    }

                    checkpointsPassed = 0;
                    lapStamp = Time.time;
                }
                else
                {
                    finishTriggered = true;
                    checkpointsPassed = 0;

                    lapStamp = Time.time;
                }
            }
            else
            {
                finishTriggered = false;
                checkpointsPassed = 0;
            }

        }
        else if (collision.tag.Equals("Checkpoint"))
        {
            int indexFound = -1;
            for (int i = 0; i < CheckpointList.list.Count; i++)
            {
                if (CheckpointList.list[i] == collision.gameObject)
                {
                    indexFound = i;
                    break;
                }
            }
            Debug.Log("indexFound: " + indexFound);
            
            if (checkpointsPassed == indexFound)
            {
                checkpointsPassed = indexFound + 1;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Finish"))
        {
            Vector3 velocity = Vector3.Project(rb.velocity, other.transform.rotation * Vector3.forward);

            if (!finishTriggered && velocity.normalized == (other.transform.rotation * Vector3.forward).normalized)
            {
                finishTriggered = true;
                checkpointsPassed = 0;

                lapStamp = Time.time;
            }
            else if(velocity.normalized != (other.transform.rotation * Vector3.forward).normalized)
            {
                finishTriggered = false;
                checkpointsPassed = 0;
            }
        }
    }

    private void Reset()
    {
        resetNum = 1;
        recordNum = 0;
        lapStamp = Time.time + 1.5f;
        fullStamp = lapStamp;
        finishTriggered = false;

        //ScreenManager.Reset();
    }

    private void PlayerFinish()
    {
        //Debug.Log("You finished!");
        totalTime = 0;
        for (int i = 0; i < previousLapTimes.Count; i++)
        {
            totalTime += previousLapTimes[i];
        }
        //timeText.text = FormatTime(totalTime);

        //ingameUI.Finish(this.gameObject, totalTime);
    }

    private string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60);
        float secondsAfterMinutes = Mathf.Floor((seconds - minutes * 60) * 100) / 100;

        //built the formatted string
        string secondsFormat = "";
        if ((int)secondsAfterMinutes / 10 == 0)
        {
            secondsFormat = "0" + secondsAfterMinutes;
        }
        else
        {
            secondsFormat = secondsAfterMinutes.ToString();
        }

        return minutes + ":" + secondsFormat;
    }

    public bool isGoingWrongWay()
    {
        if (Vector3.Angle(correctDirection, rb.velocity) > 120 && Vector3.Angle(correctDirection, transform.forward) > 120 && rb.velocity.magnitude > 100)
        {
            return true;
        }
        return false;
    }

}
