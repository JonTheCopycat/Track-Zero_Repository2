using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public abstract class SelectableOption : Selectable
{
    private AllInputActions controls;
    private int horizontalHeld = 0;
    private int verticalHeld = 0;
    private bool submitHeld = false;
    private bool clickHeld = false;

    bool wasSelected = false;
    public event Action Selected;
    public event Action Deselected;

    GameObject lastSelected;

    // Start is called before the first frame update
    new void Start()
    {
        controls = new AllInputActions();
        controls.Enable();
        PostStart();

        if (EventSystem.current != null)
            lastSelected = EventSystem.current.currentSelectedGameObject;
    }

    protected abstract void PostStart();

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current != null && lastSelected == this.gameObject)
        {

            if (wasSelected == false)
                Selected?.Invoke();

            wasSelected = true;

            //Debug.Log("Selected");
            if (controls.UI.Navigate.ReadValue<Vector2>() != null)
            {
                //Debug.Log("Navigation properly detected");
                Vector2 navigation = controls.UI.Navigate.ReadValue<Vector2>();
                if (navigation.x < -0.2f && horizontalHeld != -1)
                {
                    PressLeft();
                    horizontalHeld = -1;
                }
                if (navigation.x > 0.2f && horizontalHeld != 1)
                {
                    PressRight();
                    horizontalHeld = 1;
                }
                if (Mathf.Abs(navigation.x) < 0.2f)
                {
                    horizontalHeld = 0;
                }

                if (navigation.y < -0.2f && verticalHeld != -1)
                {
                    PressDown();
                    verticalHeld = -1;
                }
                if (navigation.y > 0.2f && verticalHeld != 1)
                {
                    PressUp();
                    verticalHeld = 1;
                }
                if (Mathf.Abs(navigation.y) < 0.2f)
                {
                    verticalHeld = 0;
                }
            }

            if (controls.UI.Cancel.ReadValue<float>() > 0.2f)
            {
                PressCancel();
            }
        }
        else
        {
            if (wasSelected)
                Deselected?.Invoke();

            wasSelected = false;
        }

        if (controls != null)
        {
            if ((controls.UI.Submit.ReadValue<float>() > 0.2f) && !submitHeld)
            {
                if (lastSelected == this.gameObject)
                    PressSubmit();
                submitHeld = true;
            }
            if (!(controls.UI.Submit.ReadValue<float>() > 0.2f) && submitHeld)
            {
                submitHeld = false;
            }

            if ((controls.UI.Click.IsPressed()) && !clickHeld)
            {
                if (lastSelected == this.gameObject)
                    PressClick(controls.UI.Point.ReadValue<Vector2>());
                clickHeld = true;
            }
            if (!(controls.UI.Click.IsPressed()) && clickHeld)
            {
                clickHeld = false;
            }

        }

        lastSelected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
    }

    protected virtual void PressLeft()
    {

    }

    protected virtual void PressRight()
    {

    }

    protected virtual void PressUp()
    {

    }

    protected virtual void PressDown()
    {

    }

    protected virtual void PressSubmit()
    {

    }

    protected virtual void PressClick(Vector2 position)
    {

    }

    protected virtual void PressCancel()
    {

    }
}
