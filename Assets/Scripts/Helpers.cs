using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));

    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);

    public static float ApplyModifierList(
        float baseValue,
        List<StatModifier> modifiers,
        IStatContext context
    ) {
        float additiveSum = 0f;
        float multiplicativeProduct = 1f;

        float result = baseValue;

        foreach (var mod in modifiers)
        {
            if (!Applies(mod, context))
                continue;

            switch (mod.operation)
            {
                case ModifierOperation.Add:
                    additiveSum += mod.value;
                    break;

                case ModifierOperation.Multiply:
                    multiplicativeProduct *= mod.value;
                    break;
            }
        }

        // Do addition first, then multiply
        if (additiveSum != 0f)
            result += additiveSum;

        if (multiplicativeProduct != 1f)
            result *= multiplicativeProduct;

        return result;
    }

    private static bool Applies(
        StatModifier mod,
        IStatContext context
    ) {
        if (mod.targetFaction != Faction.None &&
            mod.targetFaction != context.Faction)
            return false;

        if (mod.targetAttackType != AttackType.None &&
            mod.targetAttackType != context.AttackType)
            return false;

        if (mod.targetCharacterType != CharacterType.None &&
            mod.targetCharacterType != context.CharacterType)
            return false;

        return true;
    }
}
