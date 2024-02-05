using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition
{
    string name;
    Vector3 offset;
    bool lockedCam;
    float angle;

    public CameraPosition(string name, Vector3 offset, bool lockedCam, float angle)
    {
        this.name = name;
        this.offset = offset;
        this.lockedCam = lockedCam;
        this.angle = angle;
    }

    public string GetName()
    {
        return name;
    }

    public Vector3 GetOffset()
    {
        return offset;
    }

    public bool IsLockedCam()
    {
        return lockedCam;
    }

    public float GetAngle()
    {
        return angle;
    }
}
