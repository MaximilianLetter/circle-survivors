using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MenuItemType
{
    None,
    StartGame,
    StartTutorial,
    GoToSettings,
    // ...
}

public class MenuManager : MonoBehaviour
{
    private static MenuManager _instance;
    public static MenuManager Instance => _instance;

    [SerializeField] private PlayerUI _playerUI;
    [SerializeField] private InputActionReference _exit;
    [SerializeField] private InputActionReference _interact;

    [SerializeField] private AudioClip _ambientSound;

    private MenuItemType _activeItem;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    private void OnEnable()
    {
        _interact.action.Enable();
        _interact.action.started += Interact;

        _exit.action.Enable();
        _exit.action.started += ExitGame;

        //_playerUI.SetMenuPlayerUI();
    }

    private void OnDisable()
    {
        _interact.action.started -= Interact;
        _interact.action.Disable();

        _exit.action.started -= ExitGame;
        _exit.action.Disable();
    }

    private void Start()
    {
        SoundManager.Instance.PlayAmbient(_ambientSound);
    }

    public void SetActiveMenuItem(MenuItemType item)
    {
        _activeItem = item;
    }

    private void Interact(InputAction.CallbackContext obj)
    {
        if (_activeItem == MenuItemType.None) return;

        switch (_activeItem)
        {
            case MenuItemType.None:
                break;

            case MenuItemType.StartGame:
                GameManager.Instance.StartGameFromMenu();
                break;

            case MenuItemType.StartTutorial:
                GameManager.Instance.StartGameFromMenu(playTutorial: true);
                break;

            case MenuItemType.GoToSettings:
                // TODO
                break;
        }

        SoundManager.PlaySound(SoundManager.Instance.Library.Unpause);
    }

    private void ExitGame(InputAction.CallbackContext obj)
    {
        GameManager.Instance.ExitGame();
    }
}
