using UnityEngine;

public interface IStatContext
{
    Faction Faction { get; }

    AttackType AttackType { get; }

    CharacterType CharacterType { get; }
}
