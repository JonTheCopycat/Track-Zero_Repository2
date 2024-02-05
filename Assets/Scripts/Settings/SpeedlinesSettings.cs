using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedlinesSettings : MonoBehaviour
{
    public Toggle toggle;
    
    // Start is called before the first frame update
    void Start()
    {
        //toggle = GetComponent<Toggle>();
        if (toggle != null)
            toggle.onValueChanged.AddListener((value) => { UpdateSpeedlines(); });
        else
            Debug.LogError("Speedlines Toggle not found");

        if (PlayerPrefs.HasKey("speedlines"))
        {
            GameSettings.speedlines = PlayerPrefs.GetInt("speedlines") != 0;
            toggle.isOn = GameSettings.speedlines;
        }
        else
        {
            UpdateSpeedlines();
        }
    }

    // Update is called once per frame
    void UpdateSpeedlines()
    {
        GameSettings.speedlines = toggle.isOn;

        PlayerPrefs.SetInt("speedlines", GameSettings.speedlines ? 1 : 0);
    }
}
