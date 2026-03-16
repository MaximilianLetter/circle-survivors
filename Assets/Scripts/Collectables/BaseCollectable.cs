using UnityEngine;

public class BaseCollectable : MonoBehaviour
{
    [SerializeField] private CollectableType _type;
    [SerializeField] private CharacterType _characterType;
    [SerializeField] private StatModifierSO _statModifier;

    [SerializeField] private bool _showPlayerUI;
    [SerializeField] private ToggleTextOnPlayerNearby _toggleText;

    private void Start()
    {
        _toggleText.SetFlagPlayerUI(_showPlayerUI);
    }

    public void ToggleCollectableColliders(bool state)
    {
        Collider coll = GetComponent<Collider>();
        coll.enabled = state;

        Collider textColl = _toggleText.GetComponent<Collider>();
        textColl.enabled = state;
    }

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
