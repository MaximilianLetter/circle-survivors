using System.Collections;
using UnityEngine;

public enum StartupMode
{
    MainMenu,
    Tutorial,
    Game
}

public enum GameMode
{
    None,
    Tutorial,
    Game
}

/// <summary>
/// Manages the overall logic for running the game flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [SerializeField] private StartupMode _startupMode;

    //[SerializeField] private PartyOfCharacters _partyOfCharacters;

    public GameMode Mode => _mode;
    private GameMode _mode;

    public CharacterType StartingCharacter => _startingCharacter;
    private CharacterType _startingCharacter;

    private bool _levelCompleted;

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

    private void OnEnable()
    {
        EnemyManager.OnWaveSetCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        EnemyManager.OnWaveSetCompleted -= HandleLevelCompleted;
    }

    private void Start()
    {
        StartCoroutine(BootstrapFlow());
    }

    // This is the very start of the application
    private IEnumerator BootstrapFlow()
    {
        TransitionManager.Instance.SetFadeAlphaImmediate(1f);

#if UNITY_EDITOR
        switch (_startupMode)
        {
            case StartupMode.MainMenu:
                yield return LoadIntoMenuScene();
                break;

            case StartupMode.Tutorial:
                yield return LoadIntoGameSceneAndStart(playTutorial: true);
                break;

            case StartupMode.Game:
                yield return LoadIntoGameSceneAndStart();
                break;
        }
#else
        yield return LoadIntoMenuScene();
#endif
    }

    public void StartGameFromMenu(bool playTutorial = false, CharacterType charType = CharacterType.None)
    {
        StopAllCoroutines();
        TransitionManager.Instance.StopAllCoroutines();

        StartCoroutine(LoadIntoGameSceneAndStart(playTutorial, charType));
    }

    public void ReturnToMenuAfterWin()
    {
        StopAllCoroutines();
        TransitionManager.Instance.StopAllCoroutines();

        StartCoroutine(LoadIntoMenuAfterWin());
    }

    public void ReturnToMenuFromGame()
    {
        StopAllCoroutines();
        TransitionManager.Instance.StopAllCoroutines();

        StartCoroutine(LoadIntoMenuFromGame());
    }

    private IEnumerator LoadIntoMenuScene()
    {
        yield return StartCoroutine(TransitionManager.Instance.TransitionToScene(
            "MainMenu",
            false,
            false,
            true,
            false)
        );
    }

    private IEnumerator LoadIntoMenuAfterWin()
    {
        yield return StartCoroutine(TransitionManager.Instance.TransitionToScene(
            "MainMenu",
            true,
            true,
            true,
            false,
            () => ResetValues())
        );
    }

    private IEnumerator LoadIntoMenuFromGame()
    {
        yield return StartCoroutine(TransitionManager.Instance.TransitionToScene(
            "MainMenu",
            true,
            false,
            true,
            false,
            () => ResetValues())
        );
    }

    private void ResetValues()
    {
        _levelCompleted = false;

        UiManager.Instance.HideStatusText();
        GameStateManager.Instance.SetGameState(GameState.Playing);
        TransitionManager.Instance.ResetState();

        _startingCharacter = CharacterType.None;
        _mode = GameMode.None;
    }

    private IEnumerator LoadIntoGameSceneAndStart(bool playTutorial = false, CharacterType charType = CharacterType.None)
    {
        yield return StartCoroutine(TransitionManager.Instance.TransitionToScene(
            "MainScene",
            true,
            false,
            false,
            false,
            // Set mid transition so that party initializes correctly
            () => {
                _mode = playTutorial ? GameMode.Tutorial : GameMode.Game;

                if (_mode != GameMode.Tutorial)
                    _startingCharacter = charType;
            })
        );

        if (playTutorial)
            StartTutorial();
        else
            StartGame();
    }

    private void StartTutorial()
    {
        StartCoroutine(RunTutorial());
    }

    private void StartGame()
    {
        StartCoroutine(RunGameLoop());
    }

    private IEnumerator RunTutorial()
    {
        if (LevelManager.Instance.TryGetTutorialLevel(out var tutorial))
        {
            yield return RunTutorial(tutorial);
        }

        // Victory delay
        yield return new WaitForSeconds(2f);
        WinGame();
    }

    private IEnumerator RunGameLoop()
    {
        TrackRecordManager.Instance.StartTracking();
        while (true)
        {
            if (!LevelManager.Instance.TryGetNextLevel(out var config))
                break;

            yield return RunLevel(config);
        }

        // Victory delay
        yield return new WaitForSeconds(4f);
        WinGame();
    }

    private IEnumerator RunLevel(LevelConfig config, bool resetParty = false)
    {
        _levelCompleted = false;
        EnemyManager.Instance.KillAllRemainingEnemies();

        yield return StartCoroutine(
            TransitionManager.Instance.PlayWorldTransition(
                config,
                //onHiddenPhase: resetParty ? () => { _partyOfCharacters.ResetParty(); } : null
                //onHiddenPhase: () => UiManager.Instance.ShowLevelTitle(config.title, config.subTitle)
                onHiddenPhase: () => WorldTextManager.Instance.ShowDoubleLineWorldText(
                    config.title, config.subTitle, new Vector3(config.playerStartPos.x, 0, config.playerStartPos.y), true
                )
            )
        );

        if (config.type == LevelType.Waves)
            EnemyManager.Instance.StartWaveSet(config.waveSet);
        else if (config.type == LevelType.Extraction)
            EnemyManager.Instance.StartContinuousWave(
                config.constantEnemyWave, config.extractionBossWave, config.clearBeforeBoss, config.pressureTimer, config.pressureEnemyPrefab
            );

        yield return new WaitUntil(() => _levelCompleted);

        // Level done delay
        yield return new WaitForSeconds(2f);
        WorldTextManager.Instance.ShowDoubleLineWorldText(
            WorldTextManager.Instance.TextData.levelDoneText,
            config.completeText,
            null
        );
    }

    private IEnumerator RunTutorial(TutorialConfig config)
    {
        yield return StartCoroutine(
            TransitionManager.Instance.PlayWorldTransition(
                config,
                onHiddenPhase: () => WorldTextManager.Instance.ShowDoubleLineWorldText(
                    config.title, config.subTitle, null, true
                )
            )
        );

        yield return TutorialManager.Instance.RunTutorial(config);
    }

    private void HandleLevelCompleted()
    {
        _levelCompleted = true;
    }

    private void WinGame()
    {
        GameStateManager.Instance.SetGameState(GameState.Won);

        if (_mode != GameMode.Tutorial)
            TrackRecordManager.Instance.StopAndSaveTracking();
        else
            TrackRecordManager.Instance.OmitTracking();
    }
    
    public void LoseGame()
    {
        GameStateManager.Instance.SetGameState(GameState.Lost);
        TrackRecordManager.Instance.OmitTracking();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}