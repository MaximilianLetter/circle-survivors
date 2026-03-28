using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    Playing,
    Paused,
    Lost,
    Won
}

public class GameStateManager : MonoBehaviour
{
    private static GameStateManager _instance;

    [SerializeField] private float _gameOverGrayFadeDuration = 2f;
    [SerializeField] private float _pauseGrayFadeDuration = 0.25f;
    private GrayScaleEffect _grayScaleEffect;

    [SerializeField] private PlayerMovement _playerMovement;

    [Header("Inputs")]
    [SerializeField] private InputActionReference _exit;
    [SerializeField] private InputActionReference _anyKeyPress;

    private GameState _state;
    private GameState _lastState;
    private bool _movementLocked;

    private void Awake()
    {
        _instance = this;

        // Inputs
        _exit.action.started += ExitPress;
    }

    public static GameStateManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("GameStateManger is NULL");

            return _instance;
        }
    }

    private void Start()
    {
        _grayScaleEffect = GetComponent<GrayScaleEffect>();

        UiManager.Instance.HideAll();
    }

    private void OnDestroy()
    {
        _anyKeyPress.action.started -= AnyKeyPress;
        _exit.action.started -= ExitPress;
    }

    public void SetGameState(GameState newState)
    {
        if (newState == _state) return;

        _lastState = _state;
        _state = newState;

        ApplyState();
        UpdatePlayerMovement();
    }

    private void ApplyState()
    {
        switch (_state)
        {
            case GameState.Playing:
                _playerMovement.enabled = true;
                break;

            case GameState.Paused:
                break;

            case GameState.Lost:
                StartCoroutine(HandleLostState());
                break;

            case GameState.Won:
                HandleWonState();
                break;
        }
    }

    public void SetMovementLocked(bool locked)
    {
        _movementLocked = locked;
        UpdatePlayerMovement();
    }

    private void UpdatePlayerMovement()
    {
        bool canMove = _state == GameState.Playing && !_movementLocked;

        _playerMovement.ToggleMovement(canMove);
    }

    private void AnyKeyPress(InputAction.CallbackContext obj)
    {
        ResumeGameWithFade();
    }

    public void ExitPress(InputAction.CallbackContext obj)
    {
        if (_state != GameState.Paused) StartCoroutine(PauseGameAfterFade());
        else
        {
            GameManager.Instance.ReturnToMenuFromGame();
        }
    }

    public void RestartGame(InputAction.CallbackContext obj)
    {
        _anyKeyPress.action.started -= RestartGame;

        GameManager.Instance.RestartCurrentScene();
    }

    private IEnumerator PauseGameAfterFade()
    {
        _grayScaleEffect.FadeToGray(_pauseGrayFadeDuration);
        SoundManager.PlaySound(SoundManager.Instance.Library.Pause);

        yield return new WaitForSecondsRealtime(_pauseGrayFadeDuration);

        PauseGame();
        UiManager.Instance.ShowTextOnGamePause();
    }

    private void ResumeGameWithFade()
    {
        ResumeGame();
        UiManager.Instance.HideAll();
        SoundManager.PlaySound(SoundManager.Instance.Library.Unpause);

        _grayScaleEffect.FadeToColor(_pauseGrayFadeDuration);
    }

    private void PauseGame()
    {
        Time.timeScale = 0;

        _lastState = _state;
        _state = GameState.Paused;

        _anyKeyPress.action.started += AnyKeyPress;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
        _state = _lastState;

        _anyKeyPress.action.started -= AnyKeyPress;
    }

    private IEnumerator HandleLostState()
    {
        _grayScaleEffect.FadeToGray(_gameOverGrayFadeDuration);

        //UiManager.Instance.ShowTextOnGameOver();
        WorldTextManager.Instance.ShowWorldText(
            WorldTextManager.Instance.TextData.gameOverText, null
        );
        SoundManager.PlaySound(SoundManager.Instance.Library.Lose);

        EnemyManager.Instance.MakeEnemiesWalkAwayAndDie();

        yield return new WaitForSeconds(_gameOverGrayFadeDuration);

        //UiManager.Instance.ShowRestartInstructions();

        WorldTextManager.Instance.ShowPersistentText(WorldTextManager.Instance.TextData.restartInstructions);
        _anyKeyPress.action.started += RestartGame;
    }

    private void HandleWonState()
    {
        SoundManager.PlaySound(SoundManager.Instance.Library.Win);
        //UiManager.Instance.ShowTextOnGameWin();
        WorldTextManager.Instance.ShowWorldText(
            WorldTextManager.Instance.TextData.gameWonText, null
        );

        GameManager.Instance.ReturnToMenuAfterWin();
    }
}
