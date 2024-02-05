using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Utilities
{
    public static int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public static IEnumerator ColorPulse(Image image, Color color, float fadeTime)
    {
        Color baseColor = image.color;
        float startTime = Time.time;
        float timestamp = Time.time + fadeTime;
        while (Time.time < timestamp)
        {
            image.color = Color.Lerp(color, baseColor, (Time.time - startTime) / fadeTime);
            yield return null;
        }
    }
}
