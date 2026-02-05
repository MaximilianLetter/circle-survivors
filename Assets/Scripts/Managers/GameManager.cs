using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [SerializeField] private GameObject _player;

    [SerializeField] private float _cameraSize = 5f;
    [SerializeField] private float _cameraSizeStep = 0.5f;
    private SmoothCameraSizeAdjustment _cameraAdjustment;

    [SerializeField] private float _pauseGrayFadeDuration = 0.25f;
    [SerializeField] private float _gameOverGrayFadeDuration = 2f;
    private GrayScaleEffect _grayScaleEffect;

    [Header("Inputs")]
    [SerializeField] private InputActionReference _exit;
    [SerializeField] private InputActionReference _anyKeyPress;
    [SerializeField] private bool _showInstructionsOnStart;

    private bool _isPlaying = true;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("GameManger is NULL");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        _grayScaleEffect = GetComponent<GrayScaleEffect>();

        // Inputs
        _exit.action.started += ExitPress;
    }

    private void Start()
    {
        _cameraAdjustment = Camera.main.GetComponent<SmoothCameraSizeAdjustment>();

        if (_showInstructionsOnStart)
        {
            _grayScaleEffect.SetToGray();
            UiManager.Instance.ShowInstructionsOnGameStart();

            // Start paused
            PauseGame();
        } else
        {
            UiManager.Instance.HideAll();
        }
    }

    private void OnDestroy()
    {
        // NOTE: should be unnecessary as game manager always persists
        _anyKeyPress.action.started -= AnyKeyPress;
        _exit.action.started -= ExitPress;
    }

    private void AnyKeyPress(InputAction.CallbackContext obj)
    {
        ResumeGameWithFade();
    }

    private void ExitPress(InputAction.CallbackContext obj)
    {
        if (_isPlaying) StartCoroutine(PauseGameAfterFade());
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
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
        _isPlaying = false;

        _anyKeyPress.action.started += AnyKeyPress;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
        _isPlaying = true;

        _anyKeyPress.action.started -= AnyKeyPress;
    }

    public void EndGame()
    {
        SoundManager.PlaySound(SoundType.GAME_OVER);
        UiManager.Instance.ShowTextOnGameOver();

        _player.GetComponent<Collider>().enabled = false;

        _grayScaleEffect.FadeToGray(_gameOverGrayFadeDuration);

        // TODO stop enemies from clumping on player object
        Debug.Log("Game Over");
    }

    public void AdjustCameraSize(bool increase)
    {
        _cameraSize += _cameraSizeStep * (increase ? 1 : -1);

        _cameraAdjustment.TransitionSizeTo(_cameraSize);
    }
}
