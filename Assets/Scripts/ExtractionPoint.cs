using UnityEngine;

public class ExtractionPoint : MonoBehaviour
{
    private bool _extractionPointReached;

    private GuideTowardsTarget _guidance;

    private void OnTriggerEnter(Collider other)
    {
        if (_extractionPointReached) return;

        if (other.CompareTag("Player"))
        {
            EnemyManager.Instance.StopContinousWave();

            _guidance.StopGuidance();
            _extractionPointReached = true;
        }
    }

    public void SetGuidanceRef(GuideTowardsTarget guidance)
    {
        _guidance = guidance;

        _guidance.StartGuidance();
    }
}
