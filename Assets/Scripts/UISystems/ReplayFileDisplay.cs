using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cars;
using Replays;
using UnityEngine.Rendering;

namespace UISystems
{
    public class ReplayFileDisplay : SelectableOption
    {
        [SerializeField]
        TextMeshProUGUI trackNameText;

        [SerializeField]
        TextMeshProUGUI carNameText;

        [SerializeField]
        TextMeshProUGUI timeText;

        [SerializeField]
        GameObject ViewObject;

        [SerializeField]
        public Selectable PreviousFile;
        [SerializeField]
        public Selectable NextFile;

        private string trackName;
        private string carName;
        private float time;

        private Replay replay;

        public void SetData(Replay r)
        {
            replay = r;

            trackName = r.GetTrack();
            carName = r.GetCar().GetName();
            time = r.GetTotalTime();
        }

        public string GetTrack()
        {
            return trackName;
        }

        public string GetCar()
        {
            return carName;
        }

        public float GetTime()
        {
            return time;
        }

        public Replay GetReplay()
        {
            return replay;
        }

        protected override void PostStart()
        {


            Selected += ShowViewText;
            Deselected += DisableViewText;
        }

        protected override void OnDisable()
        {
            Selected += ShowViewText;
            Deselected += DisableViewText;
        }


        protected override void PressUp()
        {
            if (PreviousFile.gameObject != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(PreviousFile.gameObject);
            }
        }

        protected override void PressDown()
        {
            if (NextFile.gameObject != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(NextFile.gameObject);
            }
        }

        protected override void PressSubmit()
        {
            OpenTrack();
        }

        void OpenTrack()
        {
            GameSettings.gamemode = GameSettings.GameMode.REPLAY;

            ReplayStorage.selectedReplay = replay;

            ScreenManager.current.GoToScene(trackName);
        }

        void ShowViewText()
        {
            ViewObject.SetActive(true);
        }

        void DisableViewText()
        {
            ViewObject.SetActive(false);
        }


    }
}

