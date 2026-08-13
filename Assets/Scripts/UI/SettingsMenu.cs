using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;

    private Resolution[] _resolutions;
    private bool _isInitializing = true;

    private void Start()
    {
        PopulateResolutions();
        SyncWithCurrentSettings();

        _isInitializing = false;
    }

    private void PopulateResolutions()
    {
        _resolutions = Screen.resolutions
            .Where(r => r.width >= 1280)                // Filter out tiny resolutions
            .GroupBy(r => new { r.width, r.height })    // Groups different refresh rates together
            .Select(g => g.First())
            .OrderByDescending(r => r.width)
            .ThenByDescending(r => r.height)            // Helps with aspect ratios
            .ToArray();

        _resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < _resolutions.Length; i++)
        {
            options.Add($"{_resolutions[i].width} x {_resolutions[i].height}");
        }

        _resolutionDropdown.AddOptions(options);
    }

    private void SyncWithCurrentSettings()
    {
        var settings = SettingsManager.Instance;

        _fullscreenToggle.isOn = settings.Fullscreen;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (_resolutions[i].width == settings.Width &&
                _resolutions[i].height == settings.Height)
            {
                _resolutionDropdown.value = i;
                break;
            }
        }

        _resolutionDropdown.RefreshShownValue();
    }

    public void OnResolutionChanged(int _)
    {
        if (_isInitializing) return;

        // NOTE: Receive index from dropdown again, event wiring is off otherwise
        int index = _resolutionDropdown.value;

        var res = _resolutions[index];
        SettingsManager.Instance.SetResolution(
            res.width,
            res.height,
            _fullscreenToggle.isOn
        );
    }

    public void OnFullscreenChanged(bool _)
    {
        if (_isInitializing) return;

        // NOTE: Receive index from dropdown again, event wiring is off otherwise
        bool isFullscreen = _fullscreenToggle.isOn;

        var res = _resolutions[_resolutionDropdown.value];
        SettingsManager.Instance.SetResolution(
            res.width,
            res.height,
            isFullscreen
        );
    }

    public void OnDonePressed()
    {
        UiManager.Instance.HideSettingsMenu();
    }
}