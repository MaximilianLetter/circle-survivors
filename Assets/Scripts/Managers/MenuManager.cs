using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MenuItemType
{
    None,
    ChangeZone,
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
    [SerializeField] private PlaceObjectByHand _theHand;

    [SerializeField] private AudioClip _ambientSound;

    [Header("Zone Transition Values")]
    [SerializeField] private float _transitionTime;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private GameObject _firstZoneBounds;
    [SerializeField] private GameObject _secondZoneBounds;
    [SerializeField] private Vector3 _firstZoneTargetPos;
    [SerializeField] private Vector3 _secondZoneTargetPos;

    private MenuItemType _activeItem;
    private Transform _activeItemTransform;

    private bool _inSecondZone;

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

        // NOTE: menu player ui is not in use, instructions on use are on menu items themselves
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

        _secondZoneBounds.SetActive(false);
        _firstZoneBounds.SetActive(true);
    }

    public void SetActiveMenuItem(MenuItemType item, Transform itemTransform)
    {
        _activeItem = item;
        _activeItemTransform = itemTransform;
    }

    private void Interact(InputAction.CallbackContext obj)
    {
        if (_activeItem == MenuItemType.None) return;

        // NOTE: this is not especially clean, but works on demo scale
        if (UiManager.Instance.SettingsMenuOpen) return;

        switch (_activeItem)
        {
            case MenuItemType.None:
                break;

            case MenuItemType.ChangeZone:
                MoveScreenTowardsNextZone();
                break;

            case MenuItemType.StartGame:
                CharacterType startType = _activeItemTransform.GetComponent<BaseMenuItem>().GetCharacterType();
                StartCoroutine(LiftCharacterToStart(_activeItemTransform, startType));
                break;

            case MenuItemType.StartTutorial:
                GameManager.Instance.StartGameFromMenu(playTutorial: true);
                break;

            case MenuItemType.GoToSettings:
                UiManager.Instance.ShowSettingsMenu();
                break;
        }

        SoundManager.PlaySound(SoundManager.Instance.Library.Unpause);
    }

    private IEnumerator LiftCharacterToStart(Transform charTransform, CharacterType charType)
    {
        var playerMovement = _theHand.GetComponent<MenuMovement>();
        playerMovement.enabled = false;

        _theHand.ChangeToPickupModel();

        yield return StartCoroutine(_theHand.LiftObjectCoroutine(charTransform, 1f));

        yield return null;
        GameManager.Instance.StartGameFromMenu(charType: charType);
    }

    private void MoveScreenTowardsNextZone()
    {
        if (_inSecondZone)
        {
            StartCoroutine(MoveToMenuZone(false));
        }
        else
        {
            StartCoroutine(MoveToMenuZone(true));
        }
    }

    private IEnumerator MoveToMenuZone(bool secondZone)
    {
        _firstZoneBounds.SetActive(false);
        _secondZoneBounds.SetActive(false);

        Vector3 startPos = secondZone ? _firstZoneTargetPos : _secondZoneTargetPos;
        Vector3 targetPos = secondZone ? _secondZoneTargetPos : _firstZoneTargetPos;

        float t = 0f;

        while (t < _transitionTime)
        {
            t += Time.deltaTime;
            float progress = t / _transitionTime;

            _cameraTarget.position = Vector3.Lerp(startPos, targetPos, progress);

            yield return null;
        }

        _cameraTarget.position = targetPos;

        if (secondZone)
            _secondZoneBounds.SetActive(true);
        else
            _firstZoneBounds.SetActive(true);

        _inSecondZone = secondZone;
    }

    private void ExitGame(InputAction.CallbackContext obj)
    {
        if (UiManager.Instance.SettingsMenuOpen)
        {
            UiManager.Instance.HideSettingsMenu();
            return;
        }

        GameManager.Instance.ExitGame();
    }
}
