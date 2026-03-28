using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private PlaceObjectByHand _theHand;
    [SerializeField] private Transform _player;

    [SerializeField] private float _timeBetweenSteps = 1f;
    [SerializeField] private Vector2 _closeDistance = new Vector2(5, 6);
    [SerializeField] private Vector2 _farDistance = new Vector2(20, 24);

    private static TutorialManager _instance;
    public static TutorialManager Instance => _instance;

    private List<GameObject> _objectsToDestroyLater;

    private PartyOfCharacters _partyOfCharacters;

    private bool _fightingStep = false;

    private void Awake()
    {
        _instance = this;

        _objectsToDestroyLater = new List<GameObject>();
    }

    private void Start()
    {
        _partyOfCharacters = FindFirstObjectByType<PartyOfCharacters>();
    }

    public IEnumerator RunTutorial(TutorialConfig config)
    {
        // Initial wait during level description being present
        yield return new WaitForSeconds(2f);

        foreach (var step in config.steps)
        {
            yield return new WaitForSeconds(0.5f);

            WorldTextManager.Instance.ShowPersistentText(step.instructionText);

            CheckForFightStateTrigger(step);

            yield return WaitForCondition(step);

            WorldTextManager.Instance.HidePersistentText();
        }

        // End of tutorial
        WorldTextManager.Instance.ShowPersistentText(config.goodbyeMessage);

        yield return new WaitForSeconds(2f);

        WorldTextManager.Instance.HidePersistentText();
    }

    private IEnumerator WaitForCondition(TutorialStep step)
    {
        bool completed = false;
        int counter = 0;

        void Complete()
        {
            completed = true;
        }

        void Increment()
        {
            counter++;
            if (counter >= step.requiredAmountOfActions)
                completed = true;
        }

        switch (step.conditionType)
        {
            case TutorialConditionType.Move:
                PlayerMovement.OnPlayerMoved += Complete;
                break;

            case TutorialConditionType.Turn:
                PlayerMovement.OnPlayerTurned += Complete;
                break;

            case TutorialConditionType.Attack:
                TargetedAttackAbility.OnCharacterAttacked += Increment;
                break;

            case TutorialConditionType.EnemiesDefeated:
                EnemyManager.OnEnemyKilled += Increment;
                break;

            case TutorialConditionType.CharacterCollected:
                PartyOfCharacters.OnCharacterAdded += Complete;
                break;

            case TutorialConditionType.CollectableCollected:
                BaseCharacter.OnCollectablePickedUp += Increment;
                break;
        }

        // Spawn objects if necessary for this step
        yield return SpawnObjectsForStep(step);

        yield return new WaitUntil(() => completed);

        // Short delay after each step
        yield return new WaitForSeconds(_timeBetweenSteps);

        yield return TakeAwayStepObjects();
        WorldTextManager.Instance.HidePersistentText();

        // Unsubscribe
        PlayerMovement.OnPlayerMoved -= Complete;
        PlayerMovement.OnPlayerTurned -= Complete;
        TargetedAttackAbility.OnCharacterAttacked -= Increment;
        EnemyManager.OnEnemyKilled -= Increment;
        PartyOfCharacters.OnCharacterAdded -= Complete;
        BaseCharacter.OnCollectablePickedUp -= Complete;
    }

    private IEnumerator SpawnObjectsForStep(TutorialStep step)
    {
        if (step.objectToSpawn != null)
        {
            for (int i = 0; i < step.amountOfObjects; i++)
            {
                // NOTE: enemies (except target dummy) are placed without hand
                bool putByHand = step.conditionType != TutorialConditionType.EnemiesDefeated;
                GameObject tutorialObj = SpawnTutorialObject(step.objectToSpawn, putByHand);

                _objectsToDestroyLater.Add(tutorialObj);

                // NOTE: this time should be aligned with time to drop
                // needs to be fixed eventually
                yield return new WaitForSeconds(2f);
            }
        }
    }

    private IEnumerator TakeAwayStepObjects()
    {
        _theHand.gameObject.SetActive(true);

        // Lift every object of the step that was not destroyed already
        // NOTE: currently only applies to the target dummy
        foreach (var obj in _objectsToDestroyLater)
        {
            if (obj == null) continue;

            yield return StartCoroutine(_theHand.LiftObjectCoroutine(obj.transform, 2f));
            Destroy(obj);
        }

        _theHand.gameObject.SetActive(false);

        _objectsToDestroyLater.Clear();
    }

    private GameObject SpawnTutorialObject(GameObject prefab, bool placeByHand = true)
    {
        Vector2 distance = placeByHand ? _closeDistance : _farDistance;

        // Spawn everything close to the player
        GameObject obj = SpawnHelper.SpawnEnemyAroundTarget(
            prefab,
            _player,
            distance.x,
            distance.y,
            WorldManager.Instance.GetWorldBounds(),
            WorldManager.Instance.GetObstacleLayer(),
            out _, // out circular direction around player, here omitted
            maxAttempts: 50
        );

        // Turn towards player
        Vector3 dir = _player.transform.position - obj.transform.position;
        obj.transform.rotation = Quaternion.LookRotation(dir.normalized);

        if (placeByHand) _theHand.DropObject(obj, true);

        return obj;
    }

    private bool IsTutorialStepWithFighting(TutorialStep step)
    {
        return
            step.conditionType == TutorialConditionType.Attack ||
            step.conditionType == TutorialConditionType.EnemiesDefeated;
    }

    private void CheckForFightStateTrigger(TutorialStep step)
    {
        bool newStepFighting = IsTutorialStepWithFighting(step);

        if (newStepFighting && !_fightingStep)
        {
            _partyOfCharacters.SetPartyFightState(true);
        }
        else if (!newStepFighting && _fightingStep)
        {
            _partyOfCharacters.SetPartyFightState(false);
        }

        _fightingStep = newStepFighting;
    }
}
