using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraSettings : SelectableOption
{
    [SerializeField]
    private Text Text;

    [SerializeField]
    private Image LeftImage;
    private Color leftColor;
    Coroutine leftPulse;
    [SerializeField]
    private Image RightImage;
    private Color rightColor;
    Coroutine rightPulse;

    // Start is called before the first frame update
    protected override void PostStart()
    {
        leftColor = LeftImage.color;
        rightColor = RightImage.color;

        try
        {
            if (PlayerPrefs.HasKey("cameraPosition"))
            {
                GameSettings.cameraPosition = PlayerPrefs.GetInt("cameraPosition");
                Text.text = GameSettings.allCameraPositions[GameSettings.cameraPosition].GetName();
            }
            else
            {
                PlayerPrefs.SetInt("cameraPosition", 0);
                GameSettings.cameraPosition = 0;
                Text.text = GameSettings.allCameraPositions[GameSettings.cameraPosition].GetName();
            }

        }
        catch
        {
            PlayerPrefs.DeleteKey("cameraPosition");
            GameSettings.cameraPosition = 0;
            Text.text = GameSettings.allCameraPositions[GameSettings.cameraPosition].GetName();
        }
    }



    //// Update is called once per frame
    //void Update()
    //{
    //    if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == this.gameObject)
    //    {
    //        Debug.Log("Selected");
    //        if (controls.UI.Navigate.ReadValueAsObject().GetType() == typeof(Vector2))
    //        {
    //            Debug.Log("Navigation properly detected");
    //            Vector2 navigation = (Vector2)controls.UI.Navigate.ReadValueAsObject();
    //            if (navigation.x < -0.2f && directionPressed != -1)
    //            {
    //                CameraCycleLeft();
    //                directionPressed = -1;
    //            }
    //            if (navigation.x > 0.2f && directionPressed != 1)
    //            {
    //                CameraCycleRight();
    //                directionPressed = 1;
    //            }
    //            if (Mathf.Abs(navigation.x) < 0.2f)
    //            {
    //                directionPressed = 0;
    //            }
    //        }
    //    }
    //}

    

    protected override void PressLeft()
    {
        GameSettings.cameraPosition = mod(GameSettings.cameraPosition - 1, GameSettings.allCameraPositions.Length);
        Text.text = GameSettings.allCameraPositions[GameSettings.cameraPosition].GetName();
        if (leftPulse != null)
        {
            StopCoroutine(leftPulse);
            LeftImage.color = leftColor;
        }
        leftPulse = StartCoroutine(ColorPulse(LeftImage, Color.green, 0.5f));
            

        PlayerPrefs.SetInt("cameraPosition", GameSettings.cameraPosition);
    }

    protected override void PressRight()
    {
        GameSettings.cameraPosition = mod(GameSettings.cameraPosition + 1, GameSettings.allCameraPositions.Length);
        Text.text = GameSettings.allCameraPositions[GameSettings.cameraPosition].GetName();
        if (rightPulse != null)
        {
            StopCoroutine(rightPulse);
            RightImage.color = rightColor;
        }
        rightPulse = StartCoroutine(ColorPulse(RightImage, Color.green, 0.5f));

        PlayerPrefs.SetInt("cameraPosition", GameSettings.cameraPosition);
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
