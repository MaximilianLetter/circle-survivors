using System.Collections;
using System.Drawing;
using UnityEngine;

public class WorldTextManager : MonoBehaviour
{
    private static WorldTextManager _instance;
    public static WorldTextManager Instance => _instance;


    private WorldTextFactory _factory;
    public WorldTextFactory Factory => _factory;

    [SerializeField] private UITextData _textData;
    public UITextData TextData => _textData;

    [SerializeField] private Transform _player;

    [Header("Text Properties")]
    [SerializeField] private float _fontSizeRegular = 6f;
    [SerializeField] private float _fontSizeBig = 8f;
    [SerializeField] private float _fontSizeGiant = 16f;

    [SerializeField] private float _fadeTime = 1f;

    [SerializeField] private float _shortHoldDuration = 1f;
    [SerializeField] private float _holdDuration = 2.5f;
    [SerializeField] private float _longHoldDuration = 4f;

    private WorldTextDecal _persistentDecal;
    private WorldTextDecal _eventDecal;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        _factory = GetComponent<WorldTextFactory>();
    }

    public IEnumerator DisplayGameTitle(string title, string subtitle, Vector3 position)
    {
        var titleDecal = _factory.Create(title, position, this, fontSize: _fontSizeGiant, width: 10f);
        SoundManager.PlaySound(SoundManager.Instance.Library.WriteLong);
        titleDecal.FadeIn(_fadeTime * 2);

        yield return new WaitForSeconds(_fadeTime * 2);

        var subDecal = _factory.Create(subtitle, position + new Vector3(-1.6f, 0, -1.6f), this, fontSize: _fontSizeBig, width: 10f);
        SoundManager.PlaySound(SoundManager.Instance.Library.WriteLong);
        subDecal.FadeIn(_fadeTime * 2);
    }

    public void ShowWorldText(string text, Vector3? position = null, bool holdLong = false)
    {
        Vector3 spawnPos;
        if (position.HasValue)
            spawnPos = position.Value;
        else
            spawnPos = _player.position;

        Vector3 desired = spawnPos + new Vector3(-2, 0, -2);
        Vector3 freeDecalPos = FindFreeDecalPosition(desired, new Size(12, 8));

        float holdDuration = holdLong ? _holdDuration : _shortHoldDuration;

        _eventDecal = _factory.Create(text, freeDecalPos, this, fontSize: _fontSizeBig);
        StartCoroutine(_eventDecal.FadeInThenHoldThenOut(_fadeTime, holdDuration));
        SoundManager.PlaySound(SoundManager.Instance.Library.WriteLong);
    }

    public void ShowDoubleLineWorldText(string title, string subtitle, Vector3? position = null, bool longHold = false)
    {
        Vector3 spawnPos;
        if (position.HasValue)
            spawnPos = position.Value;
        else
            spawnPos = _player.position;

        float holdDuration = longHold ? _longHoldDuration : _holdDuration;

        StartCoroutine(DoubleLineWorldTextRoutine(title, subtitle, spawnPos, holdDuration));
    }

    private IEnumerator DoubleLineWorldTextRoutine(string title, string subtitle, Vector3 position, float holdDuration)
    {
        Vector3 desired = position + new Vector3(-2, 0, -2);
        Vector3 freeDecalPos = FindFreeDecalPosition(desired, new Size(12, 10));

        var titleDecal = _factory.Create(title, freeDecalPos, this, fontSize: _fontSizeBig);
        SoundManager.PlaySound(SoundManager.Instance.Library.WriteLong);
        StartCoroutine(titleDecal.FadeInThenHoldThenOut(_fadeTime, holdDuration + _fadeTime));

        yield return new WaitForSeconds(_fadeTime);

        Vector3 offset = new Vector3(-1, 0, -1) * subtitle.Length / 48; // TODO: just a guess, magicnumber
        var subDecal = _factory.Create(subtitle, freeDecalPos + offset, this, fontSize: _fontSizeRegular);
        SoundManager.PlaySound(SoundManager.Instance.Library.WriteLong);
        StartCoroutine(subDecal.FadeInThenHoldThenOut(_fadeTime, holdDuration));
    }

    public void ShowPersistentText(string text)
    {
        _persistentDecal = _factory.Create(text, _player.position + new Vector3(-2, 0, -2), this, fontSize: _fontSizeRegular);
        _persistentDecal.FadeIn(_fadeTime);
        SoundManager.PlaySound(SoundManager.Instance.Library.WriteLong);
    }

    public void HidePersistentText()
    {
        if (_persistentDecal == null) return;

        StartCoroutine(_persistentDecal.FadeOutAndDestroy(_fadeTime));
    }

    private Vector3 FindFreeDecalPosition(Vector3 center, Size size, int attempts = 16)
    {
        Vector3 halfExtents = new Vector3(size.Width / 2f, 0.05f, size.Height / 2f);

        LayerMask mask = LayerMask.GetMask("Obstacle");

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, mask);

        // Try original position first
        if (WorldManager.Instance.IsInsideBounds(center, halfExtents) && !Physics.CheckBox(center, halfExtents, Quaternion.identity, mask))
            return center;

        // Spiral / radial search
        for (int i = 0; i < attempts; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = 1.5f + i * 0.5f;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle),
                0,
                Mathf.Sin(angle)
            ) * radius;

            Vector3 candidate = center + offset;

            if (WorldManager.Instance.IsInsideBounds(candidate, halfExtents) && !Physics.CheckBox(candidate, halfExtents, Quaternion.identity, mask))
            {
                return candidate;
            }
        }

        // fallback
        Debug.Log("fallback to center pos");
        return center;
    }
}
