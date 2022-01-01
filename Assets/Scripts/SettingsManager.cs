using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{

    public TMP_Dropdown DropdownWindowMode; // keeps track of what window mode the player wants
    public TMP_Dropdown DropdownResolution; // keeps track of the resolution the player wants when in windowed
    // Start is called before the first frame update
    void Start()
    {
        loadPlayerSettings();
        loadSupportedResolutions();
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    /**
     * Called by the window mode dropdown
     */
    public void OnUpdateWindowMode(int mode)
    {
        switch(mode)
        {
            case 2:
                // windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.fullScreen = false;
                break;
            case 1:
                // fullscreen windowed
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.fullScreen = true;
                break;
            default:
                // fullscreen
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.fullScreen = true;
                break;
        }
        PlayerPrefs.SetInt("settings_resolution_windowmode", mode);
    }

    /**
     * Updates the resolution given the resolution option that has been selected
     */
    public void OnUpdateResolution(int resolution)
    {
        string resStr = DropdownResolution.options[resolution].text;
        int width = int.Parse(resStr.Substring(0, resStr.IndexOf('x')));
        int height = int.Parse(resStr.Substring(resStr.IndexOf('x') + 1));
        Screen.SetResolution(width, height, Screen.fullScreen);
        PlayerPrefs.SetInt("settings_resolution_resolution_width", width);
        PlayerPrefs.SetInt("settings_resolution_resolution_height", height);
    }

    // loads the stored settings
    void loadPlayerSettings()
    {
        if(PlayerPrefs.HasKey("settings_resolution_windowmode"))
        {
            switch(PlayerPrefs.GetInt("settings_resolution_windowmode", 0))
            {
                case 2:
                    // windowed
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    Screen.fullScreen = false;
                    DropdownWindowMode.SetValueWithoutNotify(2);
                    break;
                case 1:
                    // fullscreen windowed
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    Screen.fullScreen = true;
                    DropdownWindowMode.SetValueWithoutNotify(1);
                    break;
                default:
                    // fullscreen
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    Screen.fullScreen = true;
                    DropdownWindowMode.SetValueWithoutNotify(0);
                    break;
            }
        }
        if(PlayerPrefs.HasKey("settings_resolution_resolution"))
        {
            int height = PlayerPrefs.GetInt("settings_resolution_resolution_height", Screen.currentResolution.height);
            int width = PlayerPrefs.GetInt("settings_resolution_resolution_width", Screen.currentResolution.width);
            Screen.SetResolution(width, height, Screen.fullScreen);
        }
    }

    // loads the supported resolution options for the resolution dropdown
    void loadSupportedResolutions()
    {
        // get all resolutions supported by the screen
        List<Resolution> supportedResolutions = new List<Resolution>(Screen.resolutions);
        // filter out all resolutions that don't support the sentence length of 105
        supportedResolutions.RemoveAll((res) => { return res.height < 960 || res.width < 600; });
        // create the options for the dropdown
        var optionDatas = new List<TMP_Dropdown.OptionData>();
        foreach(Resolution res in supportedResolutions)
        {
            var optionData = new TMP_Dropdown.OptionData();
            optionData.text = res.width.ToString() + "x" + res.height.ToString();
            // do not add duplicates
            if(!optionDatas.Exists((od) => { return od.text == optionData.text; }))
            {
                optionDatas.Add(optionData);
            }
        }
        DropdownResolution.AddOptions(optionDatas);
    }
}
