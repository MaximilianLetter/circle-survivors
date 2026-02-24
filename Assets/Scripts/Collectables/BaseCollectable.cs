using UnityEditor.Analytics;
using UnityEngine;

public class BaseCollectable : MonoBehaviour
{
    [SerializeField] private CollectableType _type;
    [SerializeField] private CharacterType _characterType;
    [SerializeField] private StatModifierSO _statModifier;

    public CollectableType GetCollectableType()
    {
        return _type;
    }

    public CharacterType GetCharacterType()
    {
        return _characterType;
    }

    public StatModifierSO GetStatModifier()
    {
        return _statModifier;
    }
}
