using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    [SerializeField] private PartyOfCharacters circle;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Collectable")) return;

        BaseCollectable collectable = other.GetComponent<BaseCollectable>();
        CollectableType type = collectable.GetCollectableType();

        if (type == CollectableType.Character)
        {
            circle.AddCharacter(collectable.GetCharacterType());
            SoundManager.PlaySound(SoundType.COLLECT_CHARACTER);
        }
        else if (type == CollectableType.HealthPickUp)
        {
            circle.RestoreHpForAllCharacters();
            SoundManager.PlaySound(SoundType.COLLECT_BOX);
        }
        else if (type == CollectableType.StatModifier)
        {
            var mod = collectable.GetStatModifier();
            GlobalModifierSystem.Instance.AddModifier(mod.CreateRuntimeInstance());
            SoundManager.PlaySound(SoundType.COLLECT_BOX);
        }

        Destroy(other.gameObject);
    }
}
