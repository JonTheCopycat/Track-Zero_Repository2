using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cars
{
    public class CarModel : MonoBehaviour
    {
        public MeshRenderer CarBodyRenderer;
        public GameObject[] FrontTires = new GameObject[2];
        public GameObject[] RearTires = new GameObject[2];
        public BoosterBehavior[] Boosters;
        public AudioScriptableObject soundProfile;

        public Vector2 GetHitboxSize()
        {
            float width = Mathf.Abs(FrontTires[0].transform.localPosition.x - FrontTires[1].transform.localPosition.x);
            float length = Mathf.Abs(FrontTires[0].transform.localPosition.z - RearTires[0].transform.localPosition.z) * 0.75f;

            return new Vector2(width, length);
        }
    }
}
