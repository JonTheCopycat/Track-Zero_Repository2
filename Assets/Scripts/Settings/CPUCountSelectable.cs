using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPUCountSelectable : SelectableOption
{
    public Text text;

    [SerializeField]
    private Image LeftImage;
    private Color leftColor;
    Coroutine leftPulse;
    [SerializeField]
    private Image RightImage;
    private Color rightColor;
    Coroutine rightPulse;

    // Start is called before the first frame update
    protected override void PostStart()
    {
        if (LeftImage != null)
        {
            leftColor = LeftImage.color;
        }
        if (RightImage != null)
        {
            rightColor = RightImage.color;
        }

        text.text = GameSettings.cpuCount.ToString();
    }

    protected override void PressLeft()
    {
        if (GameSettings.cpuCount > 0)
            GameSettings.cpuCount--;
        if (LeftImage != null)
        {
            if (leftPulse != null)
            {
                StopCoroutine(leftPulse);
                LeftImage.color = leftColor;
            }
            leftPulse = StartCoroutine(Utilities.ColorPulse(LeftImage, Color.green, 0.5f));
        }
        

        text.text = GameSettings.cpuCount.ToString();
    }

    protected override void PressRight()
    {
        if (GameSettings.cpuCount < 32)
            GameSettings.cpuCount++;
        if (RightImage != null)
        {
            if (rightPulse != null)
            {
                StopCoroutine(rightPulse);
                RightImage.color = rightColor;
            }
            rightPulse = StartCoroutine(Utilities.ColorPulse(RightImage, Color.green, 0.5f));

            text.text = GameSettings.laps.ToString();
        }

        text.text = GameSettings.cpuCount.ToString();
    }
}
