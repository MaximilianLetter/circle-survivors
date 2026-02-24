using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Melee Attack Stats")]
public class MeleeAttackStats : ScriptableObject
{
    [Header("BaseAttack")]
    public float Damage = 9f;
    public float KnockBack = 300f;
    public BoxHitShape HitShape;

    [Header("Special")]
    public float SpecialDamage = 9f;
    public float SpecialKnockBack = 600f;
    public BoxHitShape SpecialHitShape;
}
