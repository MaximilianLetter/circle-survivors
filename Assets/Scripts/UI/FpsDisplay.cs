using TMPro;
using UnityEngine;

public class FpsDisplay : MonoBehaviour
{
    private int avgFrameRate;
    private TextMeshProUGUI _textField;

    private void Start()
    {
        _textField = GetComponent<TextMeshProUGUI>();
    }

    public void Update()
    {
        // TODO only update once every second or something
        float current = 0;
        //current = Time.frameCount / Time.time;
        current = (int)(1f / Time.unscaledDeltaTime);
        avgFrameRate = (int)current;
        _textField.text = avgFrameRate.ToString() + " FPS";
    }
}