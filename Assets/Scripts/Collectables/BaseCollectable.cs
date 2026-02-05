using UnityEditor.Analytics;
using UnityEngine;

public class BaseCollectable : MonoBehaviour
{
    [SerializeField] private CollectableType _type;
    [SerializeField] private CharacterType _characterType;

    public CollectableType GetCollectableType()
    {
        return _type;
    }

    public CharacterType GetCharacterType()
    {
        return _characterType;
    }
}
