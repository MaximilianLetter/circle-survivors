using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "UITextData", menuName = "UI/Text Data")]
public class UITextData : ScriptableObject
{
    // States
    public string demoTitle;
    public string pauseText;
    public string gameOverText;

    // Instructions
    [TextArea(3, 10)]
    public string gameStartGameplayInstructions;
    [TextArea(3, 5)]
    public string gameStartResumeInstructions;
    [TextArea(3, 5)]
    public string pauseInstructions;
}
