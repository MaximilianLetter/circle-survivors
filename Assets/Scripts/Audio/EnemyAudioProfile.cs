using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Enemy Audio Profile")]
public class EnemyAudioProfile : ScriptableObject
{
    public SFXEntry GetHit;
    public SFXEntry Die;
    public SFXEntry WallImpact;
    public SFXEntry Attack;     // Currently only in use for ranged
}