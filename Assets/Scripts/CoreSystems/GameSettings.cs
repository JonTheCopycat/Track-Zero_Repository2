using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    //player settings
    public static bool usingKeyboard = true;
    public static int cameraPosition = 0;
    public static CameraPosition[] allCameraPositions =
    {
        new CameraPosition("Near Cam", new Vector3(0, 0.4f, -2.75f), false, 18f),
        new CameraPosition("Far Cam", new Vector3(0, 0.75f, -3.5f), false, 20f),
        new CameraPosition("Hood Cam", new Vector3(0,0.6f,0.2f -0.25f), true, 2.5f),
        new CameraPosition("Low Cam", new Vector3(0, 0.3f, -2.25f), false, 15f)
    };

    public static string[] allDifficultyNames =
    {
        "easy",
        "standard",
        "hard",
        "expert",
        "unfair"
    };

    public static float cameraStiffness = 0.5f;
    public static float deadzone = 0.1f;

    //volume
    public static float masterVolume = 1;
    public static float engineVolume = 0.5f;
    public static float sfxVolume = 1;
    public static float musicVolume = 1;

    //pre-game settings
    public enum GameMode
    {
        NULL,
        PRACTICE,
        TIMETRIAL,
        GRANDPRIX,
        VS,
        CPUVS,
        ONLINE,
        REPLAY
    }

    public static GameMode gamemode = GameMode.PRACTICE;

    public static int difficulty = 1;
    public static int cpuCount = 7;
    public static int laps = 2;
    public static bool speedlines = true;
}
