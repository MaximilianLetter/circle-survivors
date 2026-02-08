using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    private static PauseManager _instance;

    [SerializeField] private float _pauseGrayFadeDuration = 0.25f;

    [Header("Inputs")]
    [SerializeField] private InputActionReference _exit;
    [SerializeField] private InputActionReference _anyKeyPress;
    [SerializeField] private bool _showInstructionsOnStart;

    private GrayScaleEffect _grayScaleEffect;
    private bool _isPlaying = true;

    private void Awake()
    {
        _instance = this;

        // Inputs
        _exit.action.started += ExitPress;
    }

    public static PauseManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("GameManger is NULL");

            return _instance;
        }
    }

    private void Start()
    {
        _grayScaleEffect = GetComponent<GrayScaleEffect>();

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

    private void AnyKeyPress(InputAction.CallbackContext obj)
    {
        ResumeGameWithFade();
    }

    public void ExitPress(InputAction.CallbackContext obj)
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
}
