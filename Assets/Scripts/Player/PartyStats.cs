using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Party Stats")]
public class PartyStats : ScriptableObject
{
    public int MaxPartySize = 3;

    public float MovementSpeed = 4;
    public float MaxTurnSpeed = 3360;
    public float MinTurnSpeed = 90;
    public float TurnSpeedAdjustPerCharacter = 30;
}
