using System.Collections;
using UnityEngine;

public class DefensiveStance : MonoBehaviour
{
    [SerializeField] private GameObject _stanceModel;
    [SerializeField] private GameObject _blockVfx;
    [SerializeField] private SFXEntry _getHitSound;
    [SerializeField] private SFXEntry _stanceActivateSound;
    [SerializeField] private SFXEntry _stanceBreakSound;

    public float DmgReduce => _dmgReduce;
    [SerializeField] private float _dmgReduce = 0.2f;
    [SerializeField] private float _stanceBreakThreshold = 1000;
    [SerializeField] private float _stanceBreakDuration = 5f;

    private BaseEnemy _enemy;

    public bool StanceActive => _stanceActive;
    private bool _stanceActive;
    private float _stanceValue;
    private bool _initialStanceActivated;

    private void Start()
    {
        _enemy = GetComponent<BaseEnemy>();
        _enemy.SetBaseModel(_stanceModel);

        ActivateDefensiveStance();

        _initialStanceActivated = true;
    }

    public void TakeStanceDamage(float knockBackAmount)
    {
        _stanceValue -= knockBackAmount;
        SoundManager.PlaySound(_getHitSound);

        if (_stanceValue <= 0)
        {
            BreakDefense();
        } else
        {
            _blockVfx.SetActive(true);
        }
    }

    private void BreakDefense()
    {
        if (!_stanceActive) return;

        SoundManager.PlaySound(_stanceBreakSound);
        _enemy.SetBaseModel(null);
        _stanceActive = false;

        StartCoroutine(ReactivateDefensiveStance());
    }

    private IEnumerator ReactivateDefensiveStance()
    {
        yield return new WaitForSeconds(_stanceBreakDuration);

        ActivateDefensiveStance();
    }

    private void ActivateDefensiveStance()
    {
        if (_stanceActive) return;

        _enemy.SetBaseModel(_stanceModel);
        _stanceValue = _stanceBreakThreshold; 
        _stanceActive = true;

        // Avoid playing make ready sound on spawn
        if (!_initialStanceActivated) return;

        SoundManager.PlaySound(_stanceActivateSound);
    }
}
