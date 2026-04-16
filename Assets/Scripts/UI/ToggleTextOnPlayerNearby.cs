using UnityEngine;

public class ToggleTextOnPlayerNearby : MonoBehaviour
{
    [SerializeField] private float _fadeTime = 1f;
    [SerializeField] private Transform _decalParent;

    private WorldTextDecal _decal;
    private Collider _coll;

    private bool _showPlayerUI;
    private PlayerUI _playerUI;
    private WorldTextDecal _playerUIDecal;

    private void Start()
    {
        // NOTE: could be made into singleton
        // used for always showing player information if required
        _playerUI = FindFirstObjectByType<PlayerUI>();
        _playerUIDecal = _playerUI.GetPlayerUIDecal();
        _coll = GetComponent<Collider>();
    }

    public void DetachFromParentAndRealign(Transform container)
    {
        // Detach from parent and put into container (for cleanup later)
        transform.SetParent(container, true);

        Vector3 directionToLookAt = Quaternion.AngleAxis(45, Vector3.up) * -Vector3.forward;
        directionToLookAt.y = 0;

        if (directionToLookAt != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToLookAt);
        }
    }

    public void SetTextContent(string content)
    {
        _decal = WorldTextManager.Instance.Factory.Create(content, Vector3.zero, this, _decalParent);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _decal.FadeIn(_fadeTime);
        SoundManager.PlaySound(SoundManager.Instance.Library.WriteShort);

        if (_showPlayerUI && _playerUI != null) _playerUIDecal?.FadeIn(_fadeTime);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _decal.FadeOut(_fadeTime);

        if (_showPlayerUI && _playerUI != null) _playerUIDecal?.FadeOut(_fadeTime);
    }

    private void OnDestroy()
    {
        _decal?.Destroy();
    }

    public void DeactivateToDestroy()
    {
        _decal.FadeOut(_fadeTime);
        if (_showPlayerUI && _playerUI != null) _playerUIDecal?.FadeOut(_fadeTime);

        _coll.enabled = false;
        Destroy(gameObject, _fadeTime);
    }

    public void SetFlagPlayerUI(bool flag)
    {
        _showPlayerUI = flag;
    }
}
