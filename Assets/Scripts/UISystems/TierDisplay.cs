using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UISystems
{
    public class TierDisplay : MonoBehaviour
    {
        [SerializeField]
        GameObject Star1;
        [SerializeField]
        GameObject Star2;
        [SerializeField]
        GameObject Star3;

        public void DisplayTier(int tier)
        {
            switch (tier)
            {
                case 1:
                    Star1.SetActive(true);
                    Star2.SetActive(false);
                    Star3.SetActive(false);
                    break;
                case 2:
                    Star1.SetActive(true);
                    Star2.SetActive(true);
                    Star3.SetActive(false);
                    break;
                case 3:
                    Star1.SetActive(true);
                    Star2.SetActive(true);
                    Star3.SetActive(true);
                    break;
                default:
                    Star1.SetActive(false);
                    Star2.SetActive(false);
                    Star3.SetActive(false);
                    break;
            }
        }
    }
}
