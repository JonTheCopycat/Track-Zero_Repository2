using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UISystems.Settings
{
    public class RebingUI : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference inputActionReference;

        [SerializeField]
        private bool excludeMouse = true;
        [Range(0, 10)]
        [SerializeField]
        private int selectedBinding;
        [SerializeField]
        private InputBinding.DisplayStringOptions displayStringOptions;
        [Header("Binding Info - DO NOT EDIT")]
        [SerializeField]
        private InputBinding inputBinding;
        private int bindingIndex;

        private string actionName;

        [Header("UI fields")]
        [SerializeField]
        private Text actionText;
        [SerializeField]
        private Button rebindButton;
        [SerializeField]
        private Text rebindText;
        [SerializeField]
        private Text extraText;
        [SerializeField]
        private Button resetButton; //don't use yet

        private void OnEnable()
        {
            rebindButton.onClick.AddListener(() => DoRebind());
            resetButton.onClick.AddListener(() => ResetBinding());

            GetBindingInfo();
            if (inputActionReference != null)
            {
                InputManager.LoadBindingOverride(actionName);
                GetBindingInfo();
                UpdateUI();
            }
            else
            {
                UpdateUI();
            }

            InputManager.rebindComplete += UpdateUI;
            InputManager.rebindCanceled += UpdateUI;
        }

        private void OnDisable()
        {
            InputManager.rebindComplete -= UpdateUI;
            InputManager.rebindCanceled -= UpdateUI;
        }

        //this is called everytime something in the editor is changed
        private void OnValidate()
        {
            if (inputActionReference == null)
                return;
            GetBindingInfo();
            UpdateUI();
        }

        private void GetBindingInfo()
        {
            if (inputActionReference.action != null)
            {
                actionName = inputActionReference.action.name;
            }

            if (inputActionReference.action.bindings.Count > selectedBinding)
            {
                inputBinding = inputActionReference.action.bindings[selectedBinding];
            }
        }

        private void UpdateUI()
        {
            if (actionText != null)
                actionText.text = actionName;

            if (rebindText != null)
            {
                if (Application.isPlaying)
                {
                    rebindText.text = InputManager.GetBindingName(actionName, bindingIndex);
                }
                else
                {
                    rebindText.text = inputActionReference.action.GetBindingDisplayString(bindingIndex);
                }
            }
        }

        private void DoRebind()
        {

            InputManager.StartRebind(actionName, bindingIndex, extraText, excludeMouse);

        }

        private void ResetBinding()
        {
            InputManager.ResetBinding(actionName, bindingIndex);
            UpdateUI();
        }

        private void ClearExtraText()
        {
            extraText.text = string.Empty;
        }
    }
}
