using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Modifiers/Modifier Bundle")]
public class ModifierBundle : ScriptableObject
{
    public List<StatModifierSO> GrantedModifiers;
}
