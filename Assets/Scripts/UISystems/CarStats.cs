using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cars;

namespace UISystems
{
    public class CarStats : MonoBehaviour
    {
        public GameObject SpeedBar;
        public GameObject AccBar;
        public GameObject DriftAccBar;
        public GameObject HandBar;
        public GameObject StabilityBar;
        public GameObject BoostBar;
        public GameObject TractionBar;

        float displaySpeed;
        float displayAcc;
        float displayDriftAcc;
        float displayHandling;
        float displayStability;
        float displayBoost;
        float displayTraction;

        // Start is called before the first frame update
        void Start()
        {
            CarCollection.DisplayStats(CarCollection.GetCarList().Length - 1);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateDisplaySmooth();
        }

        public void SetStats(float speed, float acc, float driftAcc, float handling, float oversteer, float boost, float traction)
        {

            displaySpeed = Mathf.Clamp((speed - 250f) / 100, -0.2f, 1.2f);
            displayAcc = Mathf.Clamp(acc / 250, -0.2f, 1.2f);
            displayDriftAcc = Mathf.Clamp(driftAcc / 80, -0.2f, 1.2f);
            displayHandling = Mathf.Clamp(handling / 3.5f, -0.2f, 1.2f);
            displayStability = Mathf.Clamp(1 - (oversteer + 0.5f), -0.2f, 1.2f);
            displayBoost = Mathf.Clamp(boost / 100f, -0.2f, 1.2f);
            displayTraction = Mathf.Clamp(traction, -0.2f, 1.2f);
        }

        void UpdateDisplaySmooth()
        {
            SpeedBar.transform.localScale = Vector3.MoveTowards(
                SpeedBar.transform.localScale,
                new Vector3(displaySpeed, 1, 1),
                Time.deltaTime);

            AccBar.transform.localScale = Vector3.MoveTowards(
                AccBar.transform.localScale,
                new Vector3(displayAcc, 1, 1),
                Time.deltaTime * 2.1f);

            DriftAccBar.transform.localScale = Vector3.MoveTowards(
                DriftAccBar.transform.localScale,
                new Vector3(displayDriftAcc, 1, 1),
                Time.deltaTime * 2);

            HandBar.transform.localScale = Vector3.MoveTowards(
                HandBar.transform.localScale,
                new Vector3(displayHandling, 1, 1),
                Time.deltaTime * 1.9f);

            StabilityBar.transform.localScale = Vector3.MoveTowards(
                StabilityBar.transform.localScale,
                new Vector3(displayStability, 1, 1),
                Time.deltaTime * 1.8f);

            BoostBar.transform.localScale = Vector3.MoveTowards(
                BoostBar.transform.localScale,
                new Vector3(displayBoost, 1, 1),
                Time.deltaTime * 1.7f);

            TractionBar.transform.localScale = Vector3.MoveTowards(
                TractionBar.transform.localScale,
                new Vector3(displayTraction, 1, 1),
                Time.deltaTime * 1.6f);
        }
    }
}
