using System;
using System.Collections;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance => _instance;
    private static TransitionManager _instance;

    [SerializeField] private PlaceObjectByHand _theHand;
    [SerializeField] private PartyOfCharacters _party;
    [SerializeField] private PlayerMovement _player;

    [SerializeField] private FadeFullscreenColor _fadeFullscreenColor;
    [SerializeField] private Vector3 _camTransitionOffset = new Vector3(0, 0.01f, 0);

    [Header("Transition duration values")]
    [SerializeField] private float _totalDropDuration = 0.66f; // Adjust for multiple characters being dropped
    [SerializeField] private float _totalLiftDuration = 1f;
    [SerializeField] private float _fadeAndCamShiftDuration = 1f;

    private float _dropDuration;
    private float _liftDuration;
    private bool _initialTransition = true;

    private SmoothTargetFollow _camFollow;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _camFollow = Camera.main.GetComponent<SmoothTargetFollow>();
    }

    public IEnumerator PlayWorldTransition(LevelConfig config, Action onHiddenPhase = null)
    {
        GameStateManager.Instance.SetMovementLocked(true);

        if (_initialTransition)
        {
            SetCharacterAirPositions();
            _fadeFullscreenColor.SetAlpha(1f);
            _camFollow.SetEffectOffset(_camTransitionOffset);
        }
        else
        {
            yield return LiftParty();

            Coroutine fadeIn = StartCoroutine(_fadeFullscreenColor.FadeIn(_fadeAndCamShiftDuration));
            Coroutine camShiftUp = StartCoroutine(ShiftCameraCoroutine(_fadeAndCamShiftDuration, true));
            yield return fadeIn;
            yield return camShiftUp;
        }

        // Hook to trigger "invisible" effects during transition
        // This is used for resetting the party after tutorial level
        onHiddenPhase?.Invoke();

        WorldManager.Instance.ClearWorld();
        WorldManager.Instance.GenerateWorld(config);

        // Center player, set every character to correct "in-air" position
        _player.transform.position = Vector3.zero;
        _camFollow.JumpToTarget();
        SetCharacterAirPositions();
        UiManager.Instance.HideStatusText();

        Coroutine fadeOut = StartCoroutine(_fadeFullscreenColor.FadeOut(_fadeAndCamShiftDuration));
        Coroutine camShiftDown = StartCoroutine(ShiftCameraCoroutine(_fadeAndCamShiftDuration, false));
        yield return fadeOut;
        yield return camShiftDown;

        // Do transiton down
        yield return DropParty();

        GameStateManager.Instance.SetMovementLocked(false);

        _initialTransition = false;
    }

    private IEnumerator LiftParty()
    {
        _theHand.gameObject.SetActive(true);

        var party = _party.GetAllCharacters();
        _liftDuration = _totalLiftDuration / party.Count;

        foreach (var character in party)
        {
            yield return StartCoroutine(_theHand.LiftObjectCoroutine(character.transform, _liftDuration));
        }

        _theHand.gameObject.SetActive(false);
    }

    private IEnumerator DropParty()
    {
        _theHand.gameObject.SetActive(true);

        var party = _party.GetAllCharacters();
        _dropDuration = _totalDropDuration / party.Count;

        foreach (var character in party)
        {
            yield return StartCoroutine(_theHand.DropObjectCoroutine(character.transform, _dropDuration));
        }

        _theHand.gameObject.SetActive(false);
    }

    private void SetCharacterAirPositions()
    {
        foreach (var character in _party.GetAllCharacters())
        {
            var pos = character.transform.localPosition;
            pos.y = _theHand.GetDropHeight();

            character.transform.localPosition = pos;
        }
    }

    private IEnumerator ShiftCameraCoroutine(float duration, bool goingUp)
    {
        Vector3 startOffset = goingUp ? Vector3.zero : _camTransitionOffset;
        Vector3 targetOffset = goingUp ? _camTransitionOffset : Vector3.zero;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            Vector3 offset = Vector3.Lerp(startOffset, targetOffset, t);
            _camFollow.SetEffectOffset(offset);

            yield return null;
        }

        _camFollow.SetEffectOffset(targetOffset);
    }
}
