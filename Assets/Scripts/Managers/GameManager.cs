using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the overall logic for running the game flow.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    private bool _levelCompleted;

    [SerializeField] private PartyOfCharacters _partyOfCharacters;

    private void Awake()
    {
        _instance = this;
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
        StartCoroutine(RunGameLoop());
    }

    private IEnumerator RunGameLoop()
    {
        // Tutorial
        bool tutorialRan = false;
        if (LevelManager.Instance.TryGetTutorialLevel(out var tutorial))
        {
            tutorialRan = true;
            yield return RunTutorial(tutorial);
        }

        // First real level
        if (LevelManager.Instance.TryGetNextLevel(out var firstLevel))
        {
            yield return RunLevel(firstLevel, resetParty: tutorialRan);
        }

        // All levels afterwards
        while (true)
        {
            if (!LevelManager.Instance.TryGetNextLevel(out var config))
                break;

            yield return RunLevel(config);
        }

        // Victory delay
        yield return new WaitForSeconds(2f);
        WinGame();
    }

    private IEnumerator RunLevel(LevelConfig config, bool resetParty = false)
    {
        _levelCompleted = false;
        EnemyManager.Instance.KillAllRemainingEnemies();

        yield return StartCoroutine(
            TransitionManager.Instance.PlayWorldTransition(
                config,
                onHiddenPhase: resetParty ? () => { _partyOfCharacters.ResetParty(); } : null
            )
        );

        EnemyManager.Instance.StartWaveSet(config.waveSet);

        yield return new WaitUntil(() => _levelCompleted);

        // Level done delay
        yield return new WaitForSeconds(2f);
        UiManager.Instance.ShowTextOnLevelDone();
    }

    private IEnumerator RunTutorial(TutorialConfig config)
    {
        yield return StartCoroutine(
            TransitionManager.Instance.PlayWorldTransition(config)
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
    }
    
    public void LoseGame()
    {
        GameStateManager.Instance.SetGameState(GameState.Lost);
    }
}