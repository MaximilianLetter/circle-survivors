using UnityEngine;

public class ToggleActiveMenuItemOnPlayerNearby : MonoBehaviour
{
    [SerializeField] private BaseMenuItem _item;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        MenuManager.Instance.SetActiveMenuItem(
            _item.GetMenuItemType(),
            _item.transform
        );
        _item.ToggleActiveState(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        MenuManager.Instance.SetActiveMenuItem(MenuItemType.None, null);
        _item.ToggleActiveState(false);
    }
}
