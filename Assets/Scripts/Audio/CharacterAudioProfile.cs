using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Character Audio Profile")]
public class CharacterAudioProfile : ScriptableObject
{
    public SFXEntry GetHit;
    public SFXEntry Die;
    public SFXEntry Attack;
    public SFXEntry Special;
    public SFXEntry StanceChange;
    //public SFXEntry Hit;
}