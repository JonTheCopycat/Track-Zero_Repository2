using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Replays;

namespace UISystems.Settings
{
    public class Settings : MonoBehaviour
    {
        public Text ExtraText;
        public Text DifficultyText;
        public Scrollbar stiffnessScrollBar;
        public Scrollbar deadzoneScrollBar;
        public Image ClearDataButton;
        private bool clearingData;
        public Text DetectButtonText;

        public ControlsScriptableObject controls;

        // Start is called before the first frame update
        void Start()
        {



            if (PlayerPrefs.HasKey("cameraStiffness"))
            {
                GameSettings.cameraStiffness = PlayerPrefs.GetFloat("cameraStiffness");
                stiffnessScrollBar.value = (GameSettings.cameraStiffness - 0.4f) / 0.5f;
            }
            else
            {
                UpdateStiffness();
            }

            if (PlayerPrefs.HasKey("deadzone"))
            {
                deadzoneScrollBar.value = GameSettings.deadzone / 0.9f;
            }
            else
            {
                UpdateDeadzone();
            }
        }



        private void OnDestroy()
        {
            PlayerPrefs.Save();
        }

        private void OnApplicationQuit()
        {
            OnDestroy();
        }

        // Update is called once per frame
        void Update()
        {

            if (EventSystem.current.currentSelectedGameObject != null)
            {
                if (!EventSystem.current.currentSelectedGameObject.Equals(ClearDataButton.gameObject) && clearingData)
                {
                    clearingData = false;
                    ClearDataButton.color = Color.white;
                    ExtraText.text = "Nevermind";
                }
            }
        }

        public void UpdateStiffness()
        {
            GameSettings.cameraStiffness = 0.4f + stiffnessScrollBar.value * 0.5f;

            PlayerPrefs.SetFloat("cameraStiffness", GameSettings.cameraStiffness);
        }

        public void UpdateDeadzone()
        {
            GameSettings.deadzone = deadzoneScrollBar.value * 0.9f;

            PlayerPrefs.SetFloat("deadzone", GameSettings.deadzone);
        }

        public void UpdateMasterVolume(Scrollbar scrollbar)
        {
            GameSettings.masterVolume = scrollbar.value;

            PlayerPrefs.SetFloat("masterVolume", GameSettings.masterVolume);
        }

        public void UpdateEngineVolume(Scrollbar scrollbar)
        {
            GameSettings.engineVolume = scrollbar.value;

            PlayerPrefs.SetFloat("engineVolume", GameSettings.engineVolume);
        }

        public void UpdateSfxVolume(Scrollbar scrollbar)
        {
            GameSettings.sfxVolume = scrollbar.value;

            PlayerPrefs.SetFloat("sfxVolume", GameSettings.sfxVolume);
        }

        public void ClearPlayerPrefs()
        {
            if (clearingData)
            {
                PlayerPrefs.DeleteAll();
                ReplayStorage.ClearAllReplays();
                ClearDataButton.color = Color.white;
                clearingData = false;
                ExtraText.text = "Cleared all data";
            }
            else
            {
                clearingData = true;
                ClearDataButton.color = Color.red;
                ExtraText.text = "Are you sure? This will delete all your best times and your ghost data. Press again to confirm";
            }
        }

        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }


    }
}
