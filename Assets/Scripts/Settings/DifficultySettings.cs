using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifficultySettings : SelectableOption
{
    public Text DifficultyText;

    [SerializeField]
    private Image LeftImage;
    private Color leftColor;
    Coroutine leftPulse;
    [SerializeField]
    private Image RightImage;
    private Color rightColor;
    Coroutine rightPulse;

    protected override void PostStart()
    {
        leftColor = LeftImage.color;
        rightColor = RightImage.color;

        try
        {
            if (PlayerPrefs.HasKey("difficulty"))
            {
                GameSettings.difficulty = PlayerPrefs.GetInt("difficulty");
                DifficultyText.text = GameSettings.allDifficultyNames[GameSettings.difficulty];
            }
            else
            {
                PlayerPrefs.SetInt("difficulty", 0);
                GameSettings.difficulty = 0;
                DifficultyText.text = GameSettings.allDifficultyNames[GameSettings.difficulty];
            }

        }
        catch
        {
            PlayerPrefs.DeleteKey("difficulty");
            GameSettings.difficulty = 0;
            DifficultyText.text = GameSettings.allDifficultyNames[GameSettings.difficulty];
        }
    }

    protected override void PressLeft()
    {
        GameSettings.difficulty = Utilities.mod(GameSettings.difficulty - 1, GameSettings.allDifficultyNames.Length);
        DifficultyText.text = GameSettings.allDifficultyNames[GameSettings.difficulty];
        if (leftPulse != null)
        {
            StopCoroutine(leftPulse);
            LeftImage.color = leftColor;
        }
        leftPulse = StartCoroutine(Utilities.ColorPulse(LeftImage, Color.green, 0.5f));

        PlayerPrefs.SetInt("difficulty", GameSettings.difficulty);
    }

    protected override void PressRight()
    {
        GameSettings.difficulty = Utilities.mod(GameSettings.difficulty + 1, GameSettings.allDifficultyNames.Length);
        DifficultyText.text = GameSettings.allDifficultyNames[GameSettings.difficulty];
        if (rightPulse != null)
        {
            StopCoroutine(rightPulse);
            RightImage.color = rightColor;
        }
        rightPulse = StartCoroutine(Utilities.ColorPulse(RightImage, Color.green, 0.5f));

        PlayerPrefs.SetInt("difficulty", GameSettings.difficulty);
    }
}
