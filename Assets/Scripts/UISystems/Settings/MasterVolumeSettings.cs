using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UISystems.Settings
{
    public class MasterVolumeSettings : MonoBehaviour
    {
        private Scrollbar scrollbar;

        public ControlsScriptableObject controls;

        // Start is called before the first frame update
        void Start()
        {
            scrollbar = GetComponent<Scrollbar>();
            scrollbar.onValueChanged.AddListener((value) => { UpdateVolume(); });

            if (PlayerPrefs.HasKey("masterVolume"))
            {
                GameSettings.cameraStiffness = PlayerPrefs.GetFloat("masterVolume");
                scrollbar.value = GameSettings.masterVolume;
            }
            else
            {
                UpdateVolume();
            }
        }

        public void UpdateVolume()
        {
            GameSettings.masterVolume = scrollbar.value;

            PlayerPrefs.SetFloat("masterVolume", GameSettings.masterVolume);
        }
    }
}
