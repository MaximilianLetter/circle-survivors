using UnityEngine;

public enum CharacterType
{
    // * First to be developed prototypes
    None,       // - PickUps are no characters and do not have a character type
    Bow,        // * Shoots fast, single target
    Crossbow,   // Shoots slow, pierce through enemies, pushes back harder
    Sword,      // * Hits fast, cone in front
    Axe,        // Hits slow, from top to bottom
    Spear,      // Hits fast, pierce forward, pushes back to end of attack
    Shield      // * Pushes enemies back far, blocks attacks completely
}
