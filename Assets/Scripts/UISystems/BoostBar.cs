using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostBar : MonoBehaviour
{
    public Image bar1;
    public Image bar2;
    private float boostMeter;
    private float maxBoost = 0;

    RectTransform rect;
    Vector2 baseSize;

    public void SetBoostMeter(float meter1, float meter2)
    {

        if (boostMeter > 0.5f)
        {
            bar1.rectTransform.localScale = new Vector3(1, 1, 1);
            bar2.rectTransform.localScale = new Vector3(Mathf.Clamp01((boostMeter - 0.5f) * 2), 1, 1);
        }
        else
        {
            bar1.rectTransform.localScale = new Vector3(Mathf.Clamp01(boostMeter * 2), 1, 1);
            bar2.rectTransform.localScale = new Vector3(0, 1, 1);
        }

        bar1.rectTransform.localScale = new Vector3(Mathf.Clamp01(meter1), 1, 1);
        bar2.rectTransform.localScale = new Vector3(Mathf.Clamp01(meter2), 1, 1);
    }

    public void SetMaxBoost(float boost)
    {
        maxBoost = boost;
    }

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        baseSize = new Vector2(rect.rect.width, rect.rect.height);
    }

    // Update is called once per frame
    void Update()
    {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseSize.y * Mathf.Lerp(0.5f, 1, maxBoost / 100));
    }
}
