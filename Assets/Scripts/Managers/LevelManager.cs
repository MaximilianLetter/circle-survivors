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

    private LevelConfig _configInUse;
    public LevelConfig ConfigInUse => _configInUse;

    private void Awake()
    {
        _instance = this;
    }

    public bool TryGetNextLevel(out LevelConfig config)
    {
        if (_debug)
        {
            config = _debugLevel;
            _configInUse = config;
            return true;
        }

        if (_currentLevel >= _levels.Length)
        {
            config = null;
            _configInUse = null;
            return false;
        }

        config = _levels[_currentLevel];
        _configInUse = config;

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
