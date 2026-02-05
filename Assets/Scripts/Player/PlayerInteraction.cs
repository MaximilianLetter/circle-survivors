using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private InputActionReference _interact;

    private void OnEnable()
    {
        _interact.action.started += Interact;
    }

    private void OnDisable()
    {
        _interact.action.started -= Interact;
    }

    private void Interact(InputAction.CallbackContext obj)
    {
        Debug.Log("Trying to interact");
    }
}
