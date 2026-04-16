using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShowLastRunStats : MonoBehaviour
{
    [SerializeField] private ToggleTextOnPlayerNearby _textInteraction;
    [SerializeField] private Transform _activeContainer;
    [SerializeField] private Transform _passiveContainer;
    [SerializeField] private MenuObjectMappings _mappings;

    private void Start()
    {
        // Fallback
        var records = TrackRecordManager.Instance.GetRecords();
        if (records == null || records.Count == 0)
        {
            Debug.Log("NO RECORDS FOUND");
            string displayText = "No playthroughs completed yet.";
            _textInteraction.SetTextContent(displayText);
            return;
        }

        // Working entry
        RunRecord lastRun = records.Last();
        SetText(lastRun);

        SetCharacters(lastRun.survivingCharacters);

        ArrangeChildrenInCircle(_activeContainer);
        ArrangeChildrenInCircle(_passiveContainer);
    }

    private void SetCharacters(SurvivingCharacter[] survivors)
    {
        if (survivors == null || survivors.Length == 0)
        {
            Debug.Log("NO SURVIVING CHARACTERS FOUND");
            return;
        }

        foreach (SurvivingCharacter character in survivors)
        {
            // Active State
            var activeChar = Instantiate(_mappings.GetActiveCharacter(
                character.characterType),
                _activeContainer
            );

            if (character.rank > 0)
            {
                Instantiate(_mappings.GetRankIndicator(character.rank), activeChar.transform);
            }

            // Passive State
            var passiveChar = Instantiate(_mappings.GetPassiveCharacter(
                character.characterType),
                _passiveContainer
            );

            if (character.rank > 0)
            {
                Instantiate(_mappings.GetRankIndicator(character.rank), passiveChar.transform);
            }
        }
    }

    public void ArrangeChildrenInCircle(Transform parent)
    {
        int count = parent.childCount;
        if (count == 0) return;

        float angleStep = 360f / count;
        float radius = 2f + Mathf.Max(0, (count - 3)) * 0.25f;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 localPos = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            Transform t = parent.GetChild(i);

            // Set local position (relative to parent)
            t.localPosition = localPos;

            // Make it face the center (local zero)
            Vector3 dirToCenter = (-localPos).normalized;
            t.localRotation = Quaternion.LookRotation(dirToCenter, Vector3.up);
        }
    }

    private void SetText(RunRecord record)
    {
        // Invalid run
        if (record == null || record.duration == 0)
        {
            Debug.Log("INVALID RECORD FOUND");
            return;
        }
            
        string text = "Last Run" +
                "\nDuration: " + SecondsToReadableTime(Mathf.RoundToInt(record.duration)) +
                "\nEnemies defeated: " + record.enemiesKilled +
                "\nCharacters lost: " + record.charactersLost;

        _textInteraction.SetTextContent(text);
    }

    private string SecondsToReadableTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        return $"{minutes}:{seconds:D2}";
    }
}
