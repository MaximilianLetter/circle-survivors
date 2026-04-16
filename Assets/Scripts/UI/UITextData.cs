using System.IO.Enumeration;
using UnityEngine;

[CreateAssetMenu(fileName = "UITextData", menuName = "UI/Text Data")]
public class UITextData : ScriptableObject
{
    // States
    public string demoTitle;
    public string pauseText;

    public string newWaveText;
    public string newBossWaveText;
    public string levelDoneText;

    public string extractionStartText;
    public string extractionPressureText;
    public string extractionDoneText;

    public string gameOverText;
    public string gameWonText;

    // Instructions
    [TextArea(3, 5)]
    public string gameStartResumeInstructions;
    [TextArea(3, 5)]
    public string pauseInstructions;
    [TextArea(3, 5)]
    public string restartInstructions; // NOT IN USE
    [TextArea(3, 5)]
    public string backToMenuInstructions;
}
