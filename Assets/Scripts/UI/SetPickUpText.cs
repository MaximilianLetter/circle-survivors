using TMPro;
using UnityEngine;

public class SetPickUpText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textObj;

    private BaseCollectable _collectable;

    private void Start()
    {
        _collectable = GetComponent<BaseCollectable>();
        CollectableType type = _collectable.GetCollectableType();

        string finishedText = "";
        if (type == CollectableType.HealthPickUp)
        {
            finishedText = "Restores Full Health to one Character.";
        }
        else if (type == CollectableType.Character)
        {
            var character = _collectable.GetCharacterType();

            finishedText = character.ToString();
        }
        else if (type == CollectableType.StatModifier)
        {
            var mod = _collectable.GetStatModifier();

            if (mod.StatType == StatType.PartySize)
            {
                finishedText = $"Increases Party Size by {mod.Value}.";
            }
            else
            {
                string operationString = "plus";
                string val = mod.Value.ToString();
                if (mod.Operation == ModifierOperation.Multiply)
                {
                    operationString = "times";
                }

                finishedText = $"" +
                    $"Improves a {mod.TargetCharacterType}." +
                    $"\n" +
                    $"{mod.StatType} {operationString} {val}.";
            }
        }

        _textObj.text = finishedText;
    }
}
