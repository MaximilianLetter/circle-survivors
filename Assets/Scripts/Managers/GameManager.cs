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

    [SerializeField] private float _gameOverGrayFadeDuration = 2f;

    private GrayScaleEffect _grayScaleEffect;

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
    }

    private void OnEnable()
    {
        BossEnemy.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        BossEnemy.OnBossDefeated -= HandleBossDefeated;
    }

    private void Start()
    {
        _cameraAdjustment = Camera.main.GetComponent<SmoothCameraSizeAdjustment>();

        _grayScaleEffect = GetComponent<GrayScaleEffect>();
    }

    private void HandleBossDefeated()
    {
        Debug.Log("Boss defeated!");
        WinGame();
    }

    private void WinGame()
    {
        SoundManager.PlaySound(SoundType.GAME_WIN);
        UiManager.Instance.ShowTextOnGameWin();

        Debug.Log("Game won");
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
