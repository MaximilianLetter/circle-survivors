using UnityEngine;

public class ExtractionPoint : MonoBehaviour
{
    private bool _extractionPointReached;

    private void OnTriggerEnter(Collider other)
    {
        if (_extractionPointReached) return;

        if (other.CompareTag("Player"))
        {
            EnemyManager.Instance.StopContinousWave();
            _extractionPointReached = true;
        }
    }
}
