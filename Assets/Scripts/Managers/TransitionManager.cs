using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance => _instance;
    private static TransitionManager _instance;

    [SerializeField] private FadeFullscreenColor _fadeFullscreenColor;
    [SerializeField] private Vector3 _camTransitionOffset = new Vector3(0, 0.01f, 0);

    [Header("Transition duration values")]
    [SerializeField] private float _totalDropDuration = 0.66f; // Adjust for multiple characters being dropped
    [SerializeField] private float _totalLiftDuration = 1f;
    [SerializeField] private float _fadeAndCamShiftDuration = 1f;

    private float _dropDuration;
    private float _liftDuration;
    private bool _initialTransition = true;

    private PlaceObjectByHand _theHand;
    private PartyOfCharacters _party;
    private PlayerMovement _player;
    private SmoothTargetFollow _camFollow;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
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
            SoundManager.Instance.FadeOutAmbient(2);
            SoundManager.Instance.FadeOutMusic(2);
            yield return TransitionOut(liftParty: true);
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

        // Start playing ambient from the level config 
        // TODO shuffle different tracks and more
        if (config.ambientTracks.Length > 0) 
            SoundManager.Instance.PlayAmbient(config.ambientTracks[0]);

        // TODO check again
        //if (config.musicTracks.Length > 0)
        //    SoundManager.Instance.PlayMusic(config.musicTracks[0]);

        yield return TransitionIn(dropParty: true);

        GameStateManager.Instance.SetMovementLocked(false);

        _initialTransition = false;
    }

    private IEnumerator TransitionOut(bool liftParty)
    {
        if (liftParty)
            yield return LiftParty();

        Coroutine fadeIn = StartCoroutine(_fadeFullscreenColor.FadeIn(_fadeAndCamShiftDuration));


        if (_camFollow != null)
            yield return StartCoroutine(ShiftCameraCoroutine(_fadeAndCamShiftDuration, true));
        else
            yield return fadeIn;
    }

    private IEnumerator TransitionIn(bool dropParty)
    {
        Coroutine fadeOut = StartCoroutine(_fadeFullscreenColor.FadeOut(_fadeAndCamShiftDuration));

        if (_camFollow != null)
            yield return StartCoroutine(ShiftCameraCoroutine(_fadeAndCamShiftDuration, false));
        else
            yield return fadeOut;

        if (dropParty)
            yield return DropParty();
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

    public IEnumerator TransitionToScene(
        string sceneName,
        bool fadeBeforeLoad,
        bool liftBeforeLoad,
        bool fadeAfterLoad,
        bool dropAfterLoad,
        Action onHiddenPhase = null)
    {
        if (fadeBeforeLoad)
        {
            SoundManager.Instance.FadeOutAmbient(2);
            SoundManager.Instance.FadeOutMusic(2);
            yield return TransitionOut(liftBeforeLoad);
        }

        onHiddenPhase?.Invoke();

        yield return SceneManager.LoadSceneAsync(sceneName);

        // Wait for new references to register themselves
        yield return null;

        if (fadeAfterLoad)
            yield return TransitionIn(dropAfterLoad);
    }

    public void RegisterGameplayReferences(
        PlaceObjectByHand hand,
        PartyOfCharacters party,
        PlayerMovement player,
        SmoothTargetFollow camFollow
    ) {
        _theHand = hand;
        _party = party;
        _player = player;
        _camFollow = camFollow;
    }

    public void ClearGameplayReferences()
    {
        _theHand = null;
        _party = null;
        _player = null;
        _camFollow = null;
    }

    public void SetFadeAlphaImmediate(float alpha)
    {
        _fadeFullscreenColor.SetAlpha(alpha);
    }
}
