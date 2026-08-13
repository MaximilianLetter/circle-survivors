using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class UiManager : MonoBehaviour
{
    private static UiManager _instance;
    public static UiManager Instance => _instance;

    [SerializeField] private UITextData _uiTextData;

    [SerializeField] private GameObject _settingsMenu;

    [SerializeField] private TextMeshProUGUI _versionText;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _bottomInstructions;

    [SerializeField] private CanvasGroup _titleCanvasGroup;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _subtitleText;

    [SerializeField] private float _statusHideTimer = 3f;

    public bool SettingsMenuOpen => _settingsMenuOpen;
    private bool _settingsMenuOpen;

    // NOTE: this manager is a bit junk, needs refactor at some point
    // texts can break if going in pause mode overlaps

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        HideAll();
        SetUItoColor(Color.white);
    }

    public void ShowSettingsMenu()
    {
        if (_settingsMenuOpen) return;

        _settingsMenu.SetActive(true);

        _settingsMenuOpen = true;
        Time.timeScale = 0f;
    }

    public void HideSettingsMenu()
    {
        if (!_settingsMenuOpen) return;

        _settingsMenu.SetActive(false);
        SoundManager.PlaySound(SoundManager.Instance.Library.Unpause);

        _settingsMenuOpen = false;
        Time.timeScale = 1f;
    }

    public void ShowLevelTitle(string title, string subtitle)
    {
        _titleText.text = title;
        //_titleText.enabled = true;

        _subtitleText.text = subtitle;
        //_subtitleText.enabled = true;

        _titleCanvasGroup.gameObject.SetActive(true);
    }

    public void ShowNewWaveText(bool boss = false)
    {
        string text = boss ? _uiTextData.newBossWaveText : _uiTextData.newWaveText;

        _statusText.text = text;
        _statusText.enabled = true;

        Invoke(nameof(HideStatusText), _statusHideTimer);
    }

    public void ShowExtractionText(bool start = true)
    {
        _statusText.text = start ? _uiTextData.extractionStartText : _uiTextData.extractionDoneText;
        _statusText.enabled = true;

        Invoke(nameof(HideStatusText), _statusHideTimer);
    }

    public void HideStatusText()
    {
        _statusText.enabled = false;
    }

    public void ShowTextOnGameOver()
    {
        _statusText.text = _uiTextData.gameOverText;
        _statusText.enabled = true;
    }

    public void ShowRestartInstructions()
    {
        _bottomInstructions.text = _uiTextData.restartInstructions;
        _bottomInstructions.enabled = true;
    }

    public void ShowTextOnLevelDone()
    {
        _statusText.text = _uiTextData.levelDoneText;
        _statusText.enabled = true;
    }

    public void ShowTextOnGameWin()
    {
        _statusText.text = _uiTextData.gameWonText;
        _statusText.enabled = true;
    }

    public void ShowTextOnGamePause()
    {
        _statusText.text = _uiTextData.pauseText;
        _statusText.enabled = true;

        _bottomInstructions.text = _uiTextData.pauseInstructions;
        _bottomInstructions.enabled = true;
    }

    // NOTE: currently unused, instead HideAll is used
    public void HideTextOnGameResume()
    {
        _statusText.enabled = false;
        _bottomInstructions.enabled = false;
    }

    public void ShowTutorialText(string tutorialText)
    {
        _bottomInstructions.enabled = true;
        _bottomInstructions.text = tutorialText;
    }

    public void HideTutorialText()
    {
        _bottomInstructions.enabled = false;
    }

    public void HideAll()
    {
        _statusText.enabled = false;
        _bottomInstructions.enabled = false;
        //_titleText.enabled = false;
        //_subtitleText.enabled = false;
    }

    public void SetUItoColor(Color col)
    {
        _versionText.color = col;
        _statusText.color = col;
        _bottomInstructions.color = col;
    }

    public string GetTextData()
    {
        return _uiTextData.pauseText;
    }
}
