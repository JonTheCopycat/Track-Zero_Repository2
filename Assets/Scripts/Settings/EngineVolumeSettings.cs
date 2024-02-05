using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EngineVolumeSettings : MonoBehaviour
{
    private Scrollbar scrollbar;

    // Start is called before the first frame update
    void Start()
    {
        scrollbar = GetComponent<Scrollbar>();
        scrollbar.onValueChanged.AddListener((value) => { UpdateVolume(); });

        if (PlayerPrefs.HasKey("engineVolume"))
        {
            GameSettings.cameraStiffness = PlayerPrefs.GetFloat("engineVolume");
            scrollbar.value = GameSettings.engineVolume;
        }
        else
        {
            UpdateVolume();
        }
    }

    public void UpdateVolume()
    {
        GameSettings.engineVolume = scrollbar.value;

        PlayerPrefs.SetFloat("engineVolume", GameSettings.engineVolume);
    }
}
