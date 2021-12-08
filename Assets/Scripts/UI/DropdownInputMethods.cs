using System.Collections.Generic;
using UnityEngine;
using TMPro;

/**
 * Handles the dropdown menu where the input method can be chooses
 */
public class DropdownInputMethods : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Fill dropdown with options from the loaded method input files
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (string file in InputFileHandler.GetFileNames())
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            optionData.text = file;
            options.Add(optionData);
        }
        GetComponent<TMP_Dropdown>().AddOptions(options);
    }
}
