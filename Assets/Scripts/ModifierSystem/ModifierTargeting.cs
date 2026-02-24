using System;

[Serializable]
public class ModifierTargeting
{
    public bool affectPlayerCharacters;
    public bool affectEnemies;
    public CharacterType? OnlyCharacterType;
}
