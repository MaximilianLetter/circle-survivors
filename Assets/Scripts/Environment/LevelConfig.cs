using UnityEngine;

[CreateAssetMenu(menuName = "World/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public string title;
    public string subTitle;

    [Header("World")]
    public Vector2 mapSize = new Vector2(200, 200);

    public float minDistance = 15f;
    public int characterAmount = 5;
    public int characterModifierAmount = 5;
    public int pickUpHealthAmount = 10;
    public int partyIncreaseModifierAmount = 1;

    [Header("Biome & Conditions")]
    public BiomeConfig biomeConfig;
    public Vector3 obstacleWeights = new Vector3(0.6f, 0.35f, 0.05f);
    public GameObject weatherEffectPrefab;

    [Header("Enemies")]
    public WaveSet waveSet;

    [Header("Ambient")]
    public AudioClip[] ambientTracks;

    [Header("Music")]
    public AudioClip[] musicTracks;
}
