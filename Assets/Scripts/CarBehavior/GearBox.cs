using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearBox
{

    float[] maxSpeedPerGear;

    int gear;
    int gearCount;

    float rpm;
    float maxRPM;

    public GearBox(float maxSpeed, int gearCount, float maxRPM)
    {
        maxSpeedPerGear = new float[gearCount];
        for (int i = 1; i <= gearCount; i++)
        {
            maxSpeedPerGear[i - 1] = maxSpeed * i / gearCount;
        }

        this.gearCount = gearCount;

        gear = 1;

        rpm = 0;
        this.maxRPM = maxRPM;
    }

    public int GetGear()
    {
        return gear;
    }

    public float GetRPM()
    {
        return rpm;
    }

    public float GetMaxRPM()
    {
        return maxRPM;
    }

    public void UpdateGear(float velocity)
    {
        //initial calculation of rpm
        rpm = (velocity / maxSpeedPerGear[gear - 1]) * maxRPM;

        if (rpm > maxRPM * 0.85f && gear < gearCount)
            gear++;
        if (gear > 1 && (velocity / maxSpeedPerGear[gear - 2]) * maxRPM < maxRPM * 0.75f)
            gear--;

        //final calculation of rpm
        rpm = (velocity / maxSpeedPerGear[gear - 1]) * maxRPM;

        if (gear > gearCount)
            gear = gearCount;
        if (gear < 1)
            gear = 1;
    }
}
