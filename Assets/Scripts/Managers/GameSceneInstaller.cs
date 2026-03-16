using UnityEngine;

public class GameSceneInstaller : MonoBehaviour
{
    [SerializeField] private PlaceObjectByHand _hand;
    [SerializeField] private PlayerMovement _player;
    [SerializeField] private PartyOfCharacters _party;
    [SerializeField] private SmoothTargetFollow _camFollow;

    private void Start()
    {
        TransitionManager.Instance.RegisterGameplayReferences(
            _hand,
            _party,
            _player,
            _camFollow
        );
    }

    private void OnDestroy()
    {
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.ClearGameplayReferences();
    }
}