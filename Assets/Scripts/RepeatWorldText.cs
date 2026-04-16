using UnityEngine;

public class RepeatWorldText : MonoBehaviour
{
    [SerializeField] private string text;
    [SerializeField] private float interval;

    private void Start()
    {
        InvokeRepeating(nameof(WriteLine), interval * 0.5f, interval);
    }

    private void WriteLine()
    {
        WorldTextManager.Instance.ShowWorldText(text, transform.position);
    }
}
