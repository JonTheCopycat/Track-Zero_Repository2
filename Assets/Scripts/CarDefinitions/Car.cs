using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Car
{
    string name;
    float maxSpeed;
    float handling;
    float dHandling;
    float acceleration;
    float boostStrength;
    float traction;
    float driftAcceleration;
    float brakes;
    float oversteer;
    int tier;

    string model;
    //Color color;
    float[] color;

    public Car(string name, float maxSpeed, float handling, float dHandling, float acceleration, float boostStrength, float driftAcceleration, float traction, string carBody)
    {
        this.name = name;
        this.maxSpeed = maxSpeed;
        this.handling = handling;
        this.dHandling = dHandling;
        this.acceleration = acceleration;
        this.boostStrength = boostStrength;
        this.driftAcceleration = driftAcceleration;
        this.traction = traction;
        this.brakes = 150;
        this.oversteer = 0.5f;
        this.model = carBody;
        this.color = new float[3];
        this.color[0] = Color.white.r;
        this.color[1] = Color.white.g;
        this.color[2] = Color.white.b;
        this.tier = 2;
    }

    public Car(string name, float maxSpeed, float handling, float dHandling, float acceleration, float boostStrength, float driftAcceleration, float traction, string carBody, Color color) : this(name, maxSpeed, handling, dHandling, acceleration, boostStrength, driftAcceleration, traction, carBody)
    {
        this.color = new float[3];
        this.color[0] = color.r;
        this.color[1] = color.g;
        this.color[2] = color.b;
    }

    public Car(string name, float maxSpeed, float handling, float dHandling, float acceleration, float boostStrength, float driftAcceleration, float traction, float oversteer, int tier, string carBody) : this(name, maxSpeed, handling, dHandling, acceleration, boostStrength, driftAcceleration, traction, carBody)
    {
        this.tier = tier;
        this.oversteer = oversteer;
    }

    public Car(string name, float maxSpeed, float handling, float dHandling, float acceleration, float boostStrength, float driftAcceleration, float traction, float oversteer, int tier, string carBody, Color color) : this(name, maxSpeed, handling, dHandling, acceleration, boostStrength, driftAcceleration, traction, carBody)
    {
        this.color = new float[3];
        this.color[0] = color.r;
        this.color[1] = color.g;
        this.color[2] = color.b;
        this.tier = tier;
        this.oversteer = oversteer;
    }

    public Car(string name, float maxSpeed, float handling, float dHandling, float acceleration, float brakes, float boostStrength, float driftAcceleration, float traction, float oversteer, int tier, string carBody) : this(name, maxSpeed, handling, dHandling, acceleration, boostStrength, driftAcceleration, traction, carBody)
    {
        this.tier = tier;
        this.oversteer = oversteer;
        this.brakes = brakes;
    }

    public Car(string name, float maxSpeed, float handling, float dHandling, float acceleration, float brakes, float boostStrength, float driftAcceleration, float traction, float oversteer, int tier, string carBody, Color color) : this(name, maxSpeed, handling, dHandling, acceleration, boostStrength, driftAcceleration, traction, carBody)
    {
        this.color = new float[3];
        this.color[0] = color.r;
        this.color[1] = color.g;
        this.color[2] = color.b;
        this.tier = tier;
        this.oversteer = oversteer;
        this.brakes = brakes;
    }

    public string GetName()
    {
        return name;
    }
    public float GetMaxSpeed()
    {
        return maxSpeed;
    }
    public float GetHandling()
    {
        return handling;
    }
    public float GetDHandling()
    {
        return dHandling;
    }
    public float GetAcceleration()
    {
        return acceleration;
    }

    public float GetBrakes()
    {
        return brakes;
    }

    public float GetBoostStrength()
    {
        return boostStrength;
    }

    public float GetDriftAcceleration()
    {
        return driftAcceleration;
    }
    public float GetTraction()
    {
        return traction;
    }

    public float GetOversteer()
    {
        return oversteer;
    }

    public int GetTier()
    {
        return tier;
    }

    public string GetModel()
    {
        return model;
    }

    public Color GetColor()
    {
        return new Color(color[0], color[1], color[2]);
    }
}
