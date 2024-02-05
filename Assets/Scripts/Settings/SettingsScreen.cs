using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsScreen : SelectableOption
{
    [SerializeField]
    List<GameObject> allScreens;
    private int currentScreen;

    [SerializeField]
    private Text text;
    [SerializeField]
    private Text extraText;

    [SerializeField]
    private Selectable SelectOnDown;

    [SerializeField]
    private Image LeftImage;
    private Color leftColor;
    Coroutine leftPulse;
    [SerializeField]
    private Image RightImage;
    private Color rightColor;
    Coroutine rightPulse;

    protected override void PostStart()
    {
        leftColor = LeftImage.color;
        rightColor = RightImage.color;
        currentScreen = 0;
        text.text = allScreens[currentScreen].name;

        SelectOnDown = allScreens[currentScreen].GetComponentInChildren<Selectable>();

        Selected += ShowDescriptiveText;
        Deselected += ShowNoText;
    }

    protected override void OnDisable()
    {
        Selected -= ShowDescriptiveText;
        Deselected -= ShowNoText;
    }

    protected override void PressLeft()
    {
        //move current screen away
        allScreens[currentScreen].transform.localPosition = new Vector3(1000, 0, 0);

        currentScreen = mod(currentScreen - 1, allScreens.Count);
        text.text = allScreens[currentScreen].name;
        if (leftPulse != null)
        {
            StopCoroutine(leftPulse);
            LeftImage.color = leftColor;
        }
        leftPulse = StartCoroutine(ColorPulse(LeftImage, Color.green, 0.5f));

        //move the new screen in
        allScreens[currentScreen].transform.localPosition = new Vector3(0, 0, 0);
        SelectOnDown = allScreens[currentScreen].GetComponentInChildren<Selectable>();
    }

    protected override void PressRight()
    {
        //move current screen away
        allScreens[currentScreen].transform.localPosition = new Vector3(1000, 0, 0);
        
        currentScreen = mod(currentScreen + 1, allScreens.Count);
        text.text = allScreens[currentScreen].name;
        if (rightPulse != null)
        {
            StopCoroutine(rightPulse);
            RightImage.color = rightColor;
        }
        rightPulse = StartCoroutine(ColorPulse(RightImage, Color.green, 0.5f));

        //move the new screen in
        allScreens[currentScreen].transform.localPosition = new Vector3(0, 0, 0);
        SelectOnDown = allScreens[currentScreen].GetComponentInChildren<Selectable>();
    }

    protected override void PressDown()
    {
        if (SelectOnDown != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(SelectOnDown.gameObject);

            
        }
    }

    private void ShowDescriptiveText()
    {
        if (extraText != null) 
            extraText.text = "Press Left or Right to get access more menu options";
    }

    private void ShowNoText()
    {
        if (extraText != null)
            extraText.text = string.Empty;
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public IEnumerator ColorPulse(Image image, Color color, float fadeTime)
    {
        Color baseColor = image.color;
        float startTime = Time.time;
        float timestamp = Time.time + fadeTime;
        while (Time.time < timestamp)
        {
            image.color = Color.Lerp(color, baseColor, (Time.time - startTime) / fadeTime);
            yield return null;
        }
    }
}
