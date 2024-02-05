using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MusicVolumeSettings : MonoBehaviour
{
    private Scrollbar scrollbar;

    public ControlsScriptableObject controls;

    // Start is called before the first frame update
    void Start()
    {
        scrollbar = GetComponent<Scrollbar>();
        scrollbar.onValueChanged.AddListener((value) => { UpdateVolume(); });

        if (PlayerPrefs.HasKey("musicVolume"))
        {
            GameSettings.cameraStiffness = PlayerPrefs.GetFloat("musicVolume");
            scrollbar.value = GameSettings.musicVolume;
        }
        else
        {
            UpdateVolume();
        }
    }

    public void UpdateVolume()
    {
        GameSettings.musicVolume = scrollbar.value;

        PlayerPrefs.SetFloat("musicVolume", GameSettings.musicVolume);
    }
}
