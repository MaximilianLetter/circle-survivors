using UnityEngine;

[CreateAssetMenu(menuName = "World/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Header("World")]
    public Vector2 mapSize = new Vector2(200, 200);

    public float minDistance = 15f;
    public int characterAmount = 30;
    public int pickUpAmount = 30;

    public Vector3 obstacleWeights = new Vector3(0.6f, 0.35f, 0.05f);

    [Header("Enemies")]
    public WaveSet waveSet;
}
