using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    private WorldTextDecal _decal;

    public void SetMenuPlayerUI()
    {
        string text = "Press 'E' to interact";
        Vector3 offset = CalculateOffset(4);

        if (_decal != null)
        {
            WorldTextManager.Instance.Factory.UpdateDecal(_decal, text, offset);
        }
        else
        {
            _decal = WorldTextManager.Instance.Factory.Create(text, offset, this, transform);
        }
    }

    public void SetCharacterAmountText(int current, int limit)
    {
        string text = current.ToString() + "/" + limit.ToString();
        Vector3 offset = CalculateOffset(current);

        if (_decal != null)
        {
            WorldTextManager.Instance.Factory.UpdateDecal(_decal, text, offset);
        } else
        {
            _decal = WorldTextManager.Instance.Factory.Create(text, offset, this, transform);
        }
    }

    private Vector3 CalculateOffset(int amountOfCharacters)
    {
        Vector3 directionToLookAt = Quaternion.AngleAxis(45, Vector3.up) * -Vector3.forward;
        Vector3 directionOffset = directionToLookAt.normalized * (1 + amountOfCharacters * 0.5f);

        return directionOffset;
    }

    public WorldTextDecal GetPlayerUIDecal() {
        return _decal;
    }
}
