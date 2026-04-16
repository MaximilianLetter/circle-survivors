using UnityEngine;

public class BaseCollectable : MonoBehaviour
{
    [SerializeField] private CollectableType _type;
    [SerializeField] private AttackType _attackType;
    [SerializeField] private CharacterType _characterType;
    [SerializeField] private StatModifierSO _statModifier;

    [SerializeField] private bool _showPlayerUI;
    [SerializeField] private ToggleTextOnPlayerNearby _toggleText;

    private bool _detached;

    private void Start()
    {
        _toggleText.SetFlagPlayerUI(_showPlayerUI);

        _toggleText.DetachFromParentAndRealign(transform.parent);
        _detached = true;
    }

    public void ToggleCollectableColliders(bool state)
    {
        Collider coll = GetComponent<Collider>();
        coll.enabled = state;

        Collider textColl = _toggleText.GetComponent<Collider>();
        textColl.enabled = state;

        // During hand-drop, attach to collectable again
        if (!state && _detached)
            _toggleText.DetachFromParentAndRealign(transform);
        else
            _toggleText.DetachFromParentAndRealign(transform.parent);
    }

    public CollectableType GetCollectableType()
    {
        return _type;
    }

    public AttackType GetAttackType()
    {
        return _attackType;
    }

    public CharacterType GetCharacterType()
    {
        return _characterType;
    }

    public StatModifierSO GetStatModifier()
    {
        return _statModifier;
    }

    public void DestroyCollectable()
    {
        _toggleText.DeactivateToDestroy();

        Destroy(gameObject);
    }
}
