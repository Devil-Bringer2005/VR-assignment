using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerUIToggle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject uiPrefab;

    [Header("Input")]
    [SerializeField] private InputActionReference toggleUIAction;

    private bool uiIsOpen = false;

    private void Awake()
    {
        // UI starts disabled
        if (uiPrefab != null)
        {
            uiPrefab.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (toggleUIAction != null)
        {
            toggleUIAction.action.performed += OnToggleUI;
            toggleUIAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (toggleUIAction != null)
        {
            toggleUIAction.action.performed -= OnToggleUI;
            toggleUIAction.action.Disable();
        }
    }

    private void OnToggleUI(InputAction.CallbackContext context)
    {
        ToggleUI();
    }

    private void ToggleUI()
    {
        uiIsOpen = !uiIsOpen;

        if (uiPrefab != null)
        {
            uiPrefab.SetActive(uiIsOpen);
        }
    }
}