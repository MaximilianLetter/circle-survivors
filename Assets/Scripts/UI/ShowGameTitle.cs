using System.Collections;
using UnityEngine;

public class ShowGameTitle : MonoBehaviour
{
    [SerializeField] private string _title = "The Parchment";
    [SerializeField] private string _subTitle = "Circle Survivors Demo";
    [SerializeField] private float _initialWait = 0.5f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_initialWait);

        StartCoroutine(WorldTextManager.Instance.DisplayGameTitle(
            _title, _subTitle + " " + Application.version, transform.position)
        );
    }
}
