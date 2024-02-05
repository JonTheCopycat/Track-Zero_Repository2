using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ReplayFrame
{
    //the time of this frame
    float time;

    //pure inputs
    float acceleration, brakes, steering, ebrake;
    bool boost, drift, respawn;

    //replicating position: interpolated
    float[] position;
    float[] rotation;

    //to be used for interpolation of frames
    public ReplayFrame(float time, Vector3 pos, Quaternion rot, float acc, float st, float br, float ebr, bool boo, bool dr, bool res)
    {
        this.time = time;
        acceleration = acc;
        brakes = br;
        steering = st;
        ebrake = ebr;
        boost = boo;
        drift = dr;
        respawn = res;

        position = new float[3];
        position[0] = pos.x;
        position[1] = pos.y;
        position[2] = pos.z;

        rotation = new float[3];
        rotation[0] = rot.eulerAngles.x;
        rotation[1] = rot.eulerAngles.y;
        rotation[2] = rot.eulerAngles.z;
    }

    public ReplayFrame(float time, Vector3 pos, Quaternion rot)
    {
        this.time = time;
        acceleration = 0;
        brakes = 0;
        steering = 0;
        ebrake = 0;
        boost = false;
        drift = false;
        respawn = false;

        position = new float[3];
        position[0] = pos.x;
        position[1] = pos.y;
        position[2] = pos.z;

        rotation = new float[3];
        rotation[0] = rot.eulerAngles.x;
        rotation[1] = rot.eulerAngles.y;
        rotation[2] = rot.eulerAngles.z;
    }

    public float GetTime()
    {
        return time;
    }

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

    public float GetEBrake()
    {
        return ebrake;
    }

    public bool GetBoost()
    {
        return boost;
    }

    public bool GetDrift()
    {
        return drift;
    }

    public bool GetRespawn()
    {
        return respawn;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(position[0], position[1], position[2]);
    }

    public Quaternion GetRotation()
    {
        return Quaternion.Euler(rotation[0], rotation[1], rotation[2]);
    }
}
