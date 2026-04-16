using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class RunRecordCollection
{
    public List<RunRecord> runs = new List<RunRecord>();
}

[System.Serializable]
public struct SurvivingCharacter
{
    public CharacterType characterType;
    public int rank;
}

[System.Serializable]
public class RunRecord
{
    public float duration;
    public int enemiesKilled;
    public int charactersLost;
    public SurvivingCharacter[] survivingCharacters;
}

public class TrackRecordManager : MonoBehaviour
{
    private static TrackRecordManager _instance;
    public static TrackRecordManager Instance => _instance;

    // Storage
    private string SavePath => Path.Combine(Application.persistentDataPath, "runs.json");

    public IReadOnlyList<RunRecord> GetRecords() => _records.runs;
    private RunRecordCollection _records = new RunRecordCollection();

    // Tracking data
    private float _timeStartOfRun;
    private int _enemiesDefeated;
    private int _charactersLost;

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
        LoadFromFile();
    }

    private void OnDestroy()
    {
        OmitTracking();
    }

    public void StartTracking()
    {
        _timeStartOfRun = Time.time;
        _enemiesDefeated = 0;
        _charactersLost = 0;

        BaseEnemy.OnEnemyDied += OnEnemyDied;
        BaseCharacter.OnCharacterDied += OnChracterDied;
    }

    private void OnEnemyDied(BaseEnemy enemy)
    {
        _enemiesDefeated++;
    }

    private void OnChracterDied()
    {
        _charactersLost++;
    }

    public void OmitTracking()
    {
        _enemiesDefeated = 0;
        _charactersLost = 0;

        BaseEnemy.OnEnemyDied -= OnEnemyDied;
        BaseCharacter.OnCharacterDied -= OnChracterDied;
    }

    public void StopAndSaveTracking()
    {
        float runDuration = Time.time - _timeStartOfRun;

        // Get party stats
        SurvivingCharacter[] survivingCharacters = GetSurvivingCharacters();

        RunRecord record = new RunRecord
        {
            duration = runDuration,
            enemiesKilled = _enemiesDefeated,
            charactersLost = _charactersLost,
            survivingCharacters = survivingCharacters
        };

        _records.runs.Add(record);

        // Keep only last 10 runs
        int maxRuns = 10;
        if (_records.runs.Count > maxRuns)
        {
            _records.runs.RemoveAt(0); // remove oldest
        }

        SaveToFile();

        BaseEnemy.OnEnemyDied -= OnEnemyDied;
        BaseCharacter.OnCharacterDied -= OnChracterDied;
    }

    private SurvivingCharacter[] GetSurvivingCharacters()
    {
        PartyOfCharacters party = FindFirstObjectByType<PartyOfCharacters>();
        var characters = party.GetAllCharacters();
        int count = characters.Count;
        Debug.Log("survivng chars: " + count);

        SurvivingCharacter[] charStats = new SurvivingCharacter[count];

        for (int i = 0; i < count; i++)
        {
            BaseCharacter character = characters[i].GetComponent<BaseCharacter>();

            charStats[i] = new SurvivingCharacter
            {
                characterType = character.CharacterType,
                rank = character.Rank.IndicatorLevel
            };
            Debug.Log(charStats[i]);
        }

        return charStats;
    }

    // Save & Load

    private void SaveToFile()
    {
        string json = JsonUtility.ToJson(_records, true);
        File.WriteAllText(SavePath, json);
    }

    private void LoadFromFile()
    {
        if (!File.Exists(SavePath))
        {
            _records = new RunRecordCollection();
            return;
        }

        string json = File.ReadAllText(SavePath);
        _records = JsonUtility.FromJson<RunRecordCollection>(json);
    }
}
