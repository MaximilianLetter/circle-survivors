using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    [SerializeField] private bool _showInstructionsOnStart;

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

        // NOTE: not sure how and when to use the start instructions
        // if there is a starter level, dont use at all
        if (_showInstructionsOnStart)
        {
            _grayScaleEffect.SetToGray();
            UiManager.Instance.ShowInstructionsOnGameStart();

            // Start paused
            PauseGame();
        }
        else
        {
            UiManager.Instance.HideAll();
        }
    }

    private void OnDestroy()
    {
        // NOTE: should be unnecessary as the singleton always exists
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
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public void RestartGame(InputAction.CallbackContext obj)
    {
        _anyKeyPress.action.started -= RestartGame;

        // NOTE: has to be revisited later, quick restart function
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator PauseGameAfterFade()
    {
        _grayScaleEffect.FadeToGray(_pauseGrayFadeDuration);

        yield return new WaitForSecondsRealtime(_pauseGrayFadeDuration);

        PauseGame();
        UiManager.Instance.ShowTextOnGamePause();
    }

    private void ResumeGameWithFade()
    {
        ResumeGame();
        UiManager.Instance.HideAll();

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

        UiManager.Instance.ShowTextOnGameOver();
        SoundManager.PlaySound(SoundType.GAME_OVER);

        EnemyManager.Instance.MakeEnemiesWalkAwayAndDie();

        yield return new WaitForSeconds(_gameOverGrayFadeDuration);

        UiManager.Instance.ShowRestartInstructions();
        _anyKeyPress.action.started += RestartGame;
    }

    private void HandleWonState()
    {
        SoundManager.PlaySound(SoundType.GAME_WIN);
        UiManager.Instance.ShowTextOnGameWin();

        Debug.Log("Game won");
    }
}
