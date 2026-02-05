using System;
using UnityEngine;

// Soundmanager build after https://www.youtube.com/watch?v=g5WT91Sn3hg

public enum SoundType
{
    NONE,
    // Knight
    KNIGHT_ATTACK,
    KNIGHT_KICK,
    // Sentinel
    SENTINEL_SHIELD_BASH,
    SENTINEL_MACE_ATTACK,
    // Mercenary
    MERCENARY_AXE_ATTACK,
    MERCENARY_AXE_SPECIAL,
    // Archer
    ARCHER_BOW_SHOT,
    ARCHER_ARROW_HIT,
    ARCHER_RELOAD,
    // Crossbow
    CROSSBOW_SHOT,
    CROSSBOW_BOLT_HIT,
    CROSSBOW_RELOAD,
    // Character_general
    CHARACTER_GET_HIT,
    CHARACTER_DEATH,
    // Collectables
    COLLECT_CHARACTER,
    COLLECT_BOX,
    // Enemies
    ENEMY_GET_HIT,
    ENEMY_DEATH,
    ENEMY_WALL_IMPACT,
    // General
    GAME_OVER,
}

[System.Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }

    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    private AudioSource _audioSource;

    [Range(0, 1)]
    [SerializeField] private float _globalVolume = 1f;

    [SerializeField] private SoundList[] _soundList;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref _soundList, names.Length);

        for (int i = 0; i < _soundList.Length; i++)
        {
            _soundList[i].name = names[i];
        }
    }
#endif

    public static SoundManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("SoundManger is NULL");

            return _instance;
        }
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = _instance._soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        _instance._audioSource.PlayOneShot(randomClip, volume * _instance._globalVolume);
    }
}
