using UnityEngine;

public abstract class CharacterAbility : MonoBehaviour
{
    protected BaseCharacter Character;
    protected Quaternion _baseRotation;

    [SerializeField] protected Animator _animator;

    protected virtual void Awake()
    {
        Character = GetComponent<BaseCharacter>();
    }


    // Gets called for all characters after a new character joins the circle
    public void UpdateBaseRotation()
    {
        _baseRotation = transform.localRotation;
    }

}
