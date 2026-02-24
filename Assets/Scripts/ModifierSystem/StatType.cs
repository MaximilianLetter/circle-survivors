using UnityEngine;

/// <summary>
/// Every stat that can be modified.
/// </summary>
public enum StatType
{
    // Party
    PartySize,
    MovementSpeed,
    TurnSpeed,

    // Character General
    MaxHp,
    //HpReg,

    // Character Attack
    Damage,
    Knockback,
    Cooldown,
    Range,
    // NOTE: potentially OnHitEffects (Slow, bleed, ...)

    // Character Ranged Attack
    ReloadTime,
    //Ammunition,
    //ProjectilePierce,
    // NOTE: potentially Projectile[-Width]

    // Character Melee Attack
    //AttackWidth,
    //SpecialDamage,
    //SpecialKnockback,

    // Enemies
    CollisionDmg,
    //ProjectileSpeed,

    // Character specific
    // NOTE: in future special effects that have to be unlocked
}
