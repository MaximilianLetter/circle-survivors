using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool Fullscreen { get; private set; }

    private const string WIDTH_KEY = "res_width";
    private const string HEIGHT_KEY = "res_height";
    private const string FULLSCREEN_KEY = "fullscreen";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplySettings();
    }

    public void SetResolution(int width, int height, bool fullscreen)
    {
        Width = width;
        Height = height;
        Fullscreen = fullscreen;

        ApplySettings();
        SaveSettings();
    }

    private void ApplySettings()
    {
        Screen.SetResolution(Width, Height, Fullscreen);
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey(WIDTH_KEY))
        {
            Width = PlayerPrefs.GetInt(WIDTH_KEY);
            Height = PlayerPrefs.GetInt(HEIGHT_KEY);
            Fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        }
        else
        {
            Width = Screen.width;
            Height = Screen.height;
            Fullscreen = Screen.fullScreen;
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(WIDTH_KEY, Width);
        PlayerPrefs.SetInt(HEIGHT_KEY, Height);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, Fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}