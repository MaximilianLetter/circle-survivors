using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    [SerializeField] private CircleOfCharacters circle;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Collectable")) return;

        BaseCollectable collectable = other.GetComponent<BaseCollectable>();

        if (collectable.GetCollectableType() == CollectableType.Character)
        {
            circle.AddCharacter(collectable.GetCharacterType());
            SoundManager.PlaySound(SoundType.COLLECT_CHARACTER);
        }
        else
        {
            circle.RestoreHpForAllCharacters();
            SoundManager.PlaySound(SoundType.COLLECT_BOX);
        }

        Destroy(other.gameObject);
    }
}
