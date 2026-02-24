using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ranged Attack Stats")]
public class RangedAttackStats : ScriptableObject
{
    public GameObject ProjectilePrefab;
    public float Damage = 9;
    public float KnockBack = 100f;

    public int TotalAmmo = 5;
    public float ReloadTime = 3f;
}
