using UnityEngine;

public class BlockProjectiles : MonoBehaviour
{
    [SerializeField] private SFXEntry _blockSound;
    [SerializeField] private GameObject _blockFx;

    public bool TryToBlock(Transform incomingProjectile)
    {
        var dotProd = Vector3.Dot(transform.forward, incomingProjectile.forward);
        if (dotProd > 0) return false;

        SoundManager.PlaySound(_blockSound);
        _blockFx.SetActive(true);

        Destroy(incomingProjectile.gameObject);
        return true;
    }
}
