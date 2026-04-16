using TMPro;
using UnityEngine;

public class SetPickUpText : MonoBehaviour
{
    [SerializeField] private ToggleTextOnPlayerNearby _textInteraction;
    [SerializeField] private string _customReadableText;

    private BaseCollectable _collectable;

    private void Start()
    {
        _collectable = GetComponent<BaseCollectable>();
        CollectableType type = _collectable.GetCollectableType();

        string finishedText = "";
        if (_customReadableText != string.Empty)
        {
            finishedText = _customReadableText;
        }
        else if (type == CollectableType.HealthPickUp)
        {
            finishedText = "Restores Full Health";
        }
        else if (type == CollectableType.Character)
        {
            var character = _collectable.GetCharacterType();

            finishedText = character.ToString();
        }
        else if (type == CollectableType.StatModifier)
        {
            var targetedAt = _collectable.GetAttackType();

            if (targetedAt == AttackType.None)
                finishedText = "Party Size + 1";
            else if (targetedAt == AttackType.Melee)
                finishedText = "Improves Melee Character";
            else if (targetedAt == AttackType.Ranged)
                finishedText = "Improves Ranged Character";
            //var mod = _collectable.GetStatModifier();

            //if (mod.StatType == StatType.PartySize)
            //{
            //    finishedText = $"Party Size +{mod.Value}";
            //}
            //else
            //{
            //    string operationString = "plus";
            //    string val = mod.Value.ToString();
            //    if (mod.Operation == ModifierOperation.Multiply)
            //    {
            //        operationString = "times";
            //    }

            //    finishedText = $"" +
            //        $"{mod.TargetCharacterType} Upgrade" +
            //        $"\n" +
            //        $"{mod.StatType} {operationString} {val}";
            //}
        }

        _textInteraction.SetTextContent(finishedText);
    }
}
