using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UISystems.Settings
{
    public class SfxVolumeSettings : MonoBehaviour
    {
        private Scrollbar scrollbar;

        public ControlsScriptableObject controls;

        // Start is called before the first frame update
        void Start()
        {
            scrollbar = GetComponent<Scrollbar>();
            scrollbar.onValueChanged.AddListener((value) => { UpdateVolume(); });

            if (PlayerPrefs.HasKey("sfxVolume"))
            {
                GameSettings.cameraStiffness = PlayerPrefs.GetFloat("sfxVolume");
                scrollbar.value = GameSettings.sfxVolume;
            }
            else
            {
                UpdateVolume();
            }
        }

        public void UpdateVolume()
        {
            GameSettings.sfxVolume = scrollbar.value;

            PlayerPrefs.SetFloat("sfxVolume", GameSettings.sfxVolume);
        }
    }
}
