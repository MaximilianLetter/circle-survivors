using UnityEngine;

public enum TutorialConditionType
{
    Move,
    Turn,
    Attack,
    EnemiesDefeated,
    CharacterCollected,
    CollectableCollected
}

[System.Serializable]
public class TutorialStep
{
    public string instructionText;
    public TutorialConditionType conditionType;
    public int requiredAmountOfActions;
    public GameObject objectToSpawn;
    public int amountOfObjects;
}

[CreateAssetMenu(menuName = "World/TutorialConfig")]
public class TutorialConfig : LevelConfig
{
    public TutorialStep[] steps;

    public string goodbyeMessage;
}
