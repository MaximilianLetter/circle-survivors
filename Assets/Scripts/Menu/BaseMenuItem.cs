using UnityEngine;

public class BaseMenuItem : MonoBehaviour
{
    [SerializeField] private MenuItemType _type;
    [SerializeField] private CharacterType _charType;

    [SerializeField] private bool _showPlayerUI;
    [SerializeField] private ToggleTextOnPlayerNearby _toggleText;

    [SerializeField] private GameObject _defaultModel;
    [SerializeField] private GameObject _activeModel;
    [SerializeField] private SFXEntry _stateChangeSound;

    public MenuItemType GetMenuItemType()
    {
        return _type;
    }

    public CharacterType GetCharacterType()
    {
        return _charType;
    }

    private void Start()
    {
        _toggleText.SetFlagPlayerUI(_showPlayerUI);
        _toggleText.DetachFromParentAndRealign(null);
    }

    public void ToggleActiveState(bool state)
    {
        // Only play sound on going to active
        if (state) SoundManager.PlaySound(_stateChangeSound);

        if (_defaultModel == null || _activeModel == null) return;

        _activeModel.SetActive(state);
        _defaultModel.SetActive(!state);
    }
}
