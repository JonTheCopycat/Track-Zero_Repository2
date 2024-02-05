using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public class InputManager : MonoBehaviour
{
    public static AllInputActions inputActions;

    public static event Action rebindComplete;
    public static event Action rebindCanceled;
    public static event Action<InputAction, int> rebindStarted;

    public static InputManager current;

    public static float deadzone;
    public bool isEnabled;

    private void Awake()
    {
        if (inputActions == null)
        {
            inputActions = new AllInputActions();
        }

        current = this;
        isEnabled = true;
    }

    public static void StartRebind(string actionName, int bindingIndex, Text statusText, bool excludeMouse)
    {
        InputAction action = inputActions.asset.FindAction(actionName);
        if(action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.LogWarning("Couldn't find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
            {
                DoRebind(action, firstPartIndex, statusText, true, excludeMouse);
            }
        }
        else
            DoRebind(action, bindingIndex, statusText, false, excludeMouse);
    }

    private static void DoRebind(InputAction actionToRebind, int bindingIndex, Text statusText, bool allCompositeParts, bool excludeMouse)
    {
        current.isEnabled = false;

        if (actionToRebind == null || bindingIndex < 0)
        {
            Debug.Log("Action to rebind not found");
            return;
        }
            
        if (allCompositeParts)
        {
            statusText.text = $"Press a {actionToRebind.expectedControlType} for {actionToRebind.bindings[bindingIndex].name} direction";
        }
        else
        {
            statusText.text = $"Press a {actionToRebind.expectedControlType}";
        }
        

        actionToRebind.Disable();

        var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);

        rebind.OnComplete(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();
            statusText.text = "Success";

            if(allCompositeParts)
            {
                var nextBindingIndex = bindingIndex + 1;
                if (nextBindingIndex < actionToRebind.bindings.Count && actionToRebind.bindings[nextBindingIndex].isPartOfComposite)
                    DoRebind(actionToRebind, nextBindingIndex, statusText, allCompositeParts, excludeMouse);
            }
            
            SaveBindingOverride(actionToRebind);
            current.isEnabled = true;
            rebindComplete?.Invoke();
        });

        rebind.OnCancel(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();

            current.isEnabled = true;
            rebindCanceled?.Invoke();
        });

        rebind.WithTargetBinding(bindingIndex);
        rebind.WithCancelingThrough("<Keyboard>/escape");
        if (excludeMouse)
            rebind.WithControlsExcluding("Mouse");

        //exclude axis versions of the thumbsticks
        rebind.WithControlsExcluding("<Gamepad>/leftStick/x");
        rebind.WithControlsExcluding("<Gamepad>/leftStick/y");
        rebind.WithControlsExcluding("<Gamepad>/dpad/x");
        rebind.WithControlsExcluding("<Gamepad>/dpad/y");
        rebind.WithControlsExcluding("<Gamepad>/rightStick/x");
        rebind.WithControlsExcluding("<Gamepad>/rightStick/y");
        rebind.WithTimeout(10f);

        rebindStarted?.Invoke(actionToRebind, bindingIndex);
        current.StartCoroutine(current.DelayedRebindStart(rebind));
        //rebind.Start(); //actually starts the rebinding process
    }

    private IEnumerator DelayedRebindStart(InputActionRebindingExtensions.RebindingOperation rebind)
    {
        yield return new WaitForSecondsRealtime(0.2f);
        rebind.Start();
    }

    public static string GetBindingName(string actionName, int bindingIndex)
    {
        if (inputActions == null)
            inputActions = new AllInputActions();

        InputAction action = inputActions.asset.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    private static void SaveBindingOverride(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
        }
    }

    public static void LoadBindingOverride(string actionName)
    {
        if (inputActions == null)
            inputActions = new AllInputActions();

        InputAction action = inputActions.asset.FindAction(actionName);

        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i)))
                action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
        }
    }

    public static void ResetBinding(string actionName, int bindingIndex)
    {
        InputAction action = inputActions.asset.FindAction(actionName);

        if (action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.Log("Could not find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
            {
                action.RemoveBindingOverride(i);
            }
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }
        SaveBindingOverride(action);
    }
}
