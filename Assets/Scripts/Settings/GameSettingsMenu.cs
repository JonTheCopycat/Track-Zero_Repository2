using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsMenu : MonoBehaviour
{
    public Text CPUPickerText, LapsPickerText;
    
    // Start is called before the first frame update
    void Start()
    {
        CPUPickerText.text = GameSettings.cpuCount.ToString();
        LapsPickerText.text = GameSettings.laps.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseCPUs()
    {
        
    }

    public void DecreaseCPUs()
    {
        
    }

    public void IncreaseLaps()
    {
        GameSettings.laps++;

        LapsPickerText.text= GameSettings.laps.ToString();
    }

    public void DecreaseLaps()
    {
        if (GameSettings.laps > 1)
            GameSettings.laps--;

        LapsPickerText.text = GameSettings.laps.ToString();
    }
}
