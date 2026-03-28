using UnityEngine;

public enum LevelType
{
    Waves,
    Extraction
}

[CreateAssetMenu(menuName = "World/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public LevelType type;
    public string title = "Title";
    [TextArea(3, 5)]
    public string subTitle = "A big adventure.";
    [TextArea(3, 5)]
    public string bossText = "A giant beast approaches";
    [TextArea(3, 5)]
    public string completeText = "Enemies were shattered.";

    [Header("World")]
    public Vector2 mapSize = new Vector2(200, 200);
    public Vector2 playerStartPos = Vector2.zero;

    public float minDistance = 15f;
    public int characterAmount = 5;
    public int characterModifierAmount = 5;
    public int pickUpHealthAmount = 10;
    public int partyIncreaseModifierAmount = 1;

    [Header("Biome & Conditions")]
    public BiomeConfig biomeConfig;
    public Vector3 obstacleWeights = new Vector3(0.6f, 0.35f, 0.05f);
    public GameObject weatherEffectPrefab;

    [Header("WavesType Properties")]
    public WaveSet waveSet;

    [Header("ExtractionType Properties")]
    public EnemyWave constantEnemyWave;
    public BossWave extractionBossWave;
    public GameObject extractionPoint;
    public Vector3 extractionPointPosition;
    public GameObject extractionGuidance;

    [Header("Ambient")]
    public AudioClip[] ambientTracks;

    [Header("Music")]
    public AudioClip[] musicTracks;
}
