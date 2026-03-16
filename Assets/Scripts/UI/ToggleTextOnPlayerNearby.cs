using UnityEngine;

public class ToggleTextOnPlayerNearby : MonoBehaviour
{
    [SerializeField] private FadeCanvasGroup _fade;

    private FadeCanvasGroup _playerUIFade;
    private bool _showPlayerUI;

    private void Start()
    {
        // NOTE: could be made into singleton
        // used for always showing player information if required
        _playerUIFade = FindFirstObjectByType<PlayerUI>()?.GetFadeGroup();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _fade.FadeIn();

        if (_showPlayerUI && _playerUIFade != null) _playerUIFade.FadeIn();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _fade.FadeOut();

        if (_showPlayerUI && _playerUIFade != null) _playerUIFade.FadeOut();
    }

    private void OnDestroy()
    {
        if (_showPlayerUI && _playerUIFade != null) _playerUIFade.FadeOut();
    }

    public void SetFlagPlayerUI(bool flag)
    {
        _showPlayerUI = flag;
    }
}
