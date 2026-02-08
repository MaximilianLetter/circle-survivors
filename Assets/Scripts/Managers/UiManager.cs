using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    private static UiManager _instance;

    [SerializeField] private UITextData _uiTextData;

    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _bottomInstructions;
    [SerializeField] private TextMeshProUGUI _bigCenterInstructions;

    [SerializeField] private float _statusHideTimer = 3f;

    // NOTE: this manager is a bit junk, needs refactor at some point
    // texts can break if going in pause mode overlaps

    public static UiManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("UiManager is NULL");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        HideAll();
    }

    public void ShowInstructionsOnGameStart()
    {
        _statusText.text = _uiTextData.demoTitle + " " + Application.version;
        _statusText.enabled = true;

        _bigCenterInstructions.text = _uiTextData.gameStartGameplayInstructions;
        _bigCenterInstructions.enabled = true;

        _bottomInstructions.text = _uiTextData.gameStartResumeInstructions;
        _bottomInstructions.enabled = true;
    }

    public void ShowNewWaveText(bool boss = false)
    {
        string text = boss ? _uiTextData.newBossWaveText : _uiTextData.newWaveText;

        _statusText.text = text;
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

    public void HideAll()
    {
        _statusText.enabled = false;
        _bottomInstructions.enabled = false;
        _bigCenterInstructions.enabled = false;
    }
}
