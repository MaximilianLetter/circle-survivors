using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance => _instance;
    private static LevelManager _instance;

    [SerializeField] private bool _debug = false;
    [SerializeField] private TutorialConfig _tutorialLevel;
    [SerializeField] private LevelConfig _debugLevel;
    [SerializeField] private LevelConfig[] _levels;

    private int _currentLevel;

    private void Awake()
    {
        _instance = this;
    }

    public bool TryGetNextLevel(out LevelConfig config)
    {
        if (_debug)
        {
            config = _debugLevel;
            return true;
        }

        if (_currentLevel >= _levels.Length)
        {
            config = null;
            return false;
        }

        config = _levels[_currentLevel];
        _currentLevel++;
        return true;
    }

    public bool TryGetTutorialLevel(out TutorialConfig config)
    {
        if (_tutorialLevel != null)
        {
            config = _tutorialLevel;
            return true;
        }

        config = null;
        return false;
    }
}
