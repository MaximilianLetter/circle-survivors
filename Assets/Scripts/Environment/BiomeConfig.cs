using UnityEngine;

[CreateAssetMenu(menuName = "World/Biome Config")]
public class BiomeConfig : ScriptableObject
{
    public GameObject[] smallObstacles;
    public GameObject[] mediumObstacles;
    public GameObject[] largeObstacles;

    // Could in future also hold something like ground color
}
