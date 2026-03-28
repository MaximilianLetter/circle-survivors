using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Global Audio Library")]
public class GlobalAudioLibrary : ScriptableObject
{
    [Header("UI")]
    //public SFXEntry TextPlop;
    public SFXEntry WriteShort;
    public SFXEntry WriteLong;

    [Header("Gameplay")]
    public SFXEntry CollectPickUp;
    public SFXEntry CollectCharacter;
    public SFXEntry NewWave;
    public SFXEntry HandPlaceObject;

    [Header("GameState")]
    public SFXEntry Win;
    public SFXEntry Lose;

    [Header("System")]
    public SFXEntry Pause;
    public SFXEntry Unpause;
}