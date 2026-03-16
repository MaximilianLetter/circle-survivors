using UnityEngine;

public class BlockProjectiles : MonoBehaviour
{

    [SerializeField] private SFXEntry _blockSound;
    [SerializeField] private GameObject _blockFx;

    public void Block()
    {
        SoundManager.PlaySound(_blockSound);
        _blockFx.SetActive(true);
    }
}
