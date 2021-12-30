using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/**
 * This script does the control for a level as a UI element
 */
public class UILevel : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text TextLevelName;      // the text element that shows the name
    public TMP_Text TextAuthorName;     // the text element that shows the author name
    public Image DifficultySliderFill;  // the image element that controls the difficulty slider
    public Image Warning;               // the image element showing if a warning occured
    public GameObject Tooltip;          // the tooltip element showing the warning message
    int parserResult;                   // the result of the level parsing
    public UILevelLister LevelFinder;   // The parent that creates these objects

    // Start is called before the first frame update
    void Start()
    {
        // by default warning and tooltip should not be visible
        Warning.gameObject.SetActive(false);
        Tooltip.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // show error if the error occured and set the text
        if (parserResult > 0)
        { 
            Warning.gameObject.SetActive(true);
            switch(parserResult)
            {
                case 2:
                    Tooltip.GetComponentInChildren<TMPro.TMP_Text>().SetText("Kanji missing furigana solution!");
                    break;
                default:
                    Tooltip.GetComponentInChildren<TMPro.TMP_Text>().SetText("One of the sentences is too long!");
                    break;
            }
            
        }
        else
        {
            Warning.gameObject.SetActive(false);
        }
    }

    /**
     * Flags this UI element that it has the sentence too long warning
     */
    public void SetParserResult(int result)
    {
        parserResult = result;
    }

    /**
     * Sets the author of the UI element
     */
    public void SetAuthorName(string name)
    {
        TextAuthorName.SetText(name);
    }

    /**
     * Sets the name of the level's UI element
     */
    public void SetLevelName(string name)
    {
        TextLevelName.SetText(name);
    }

    /**
     * Sets the difficulty of the level's UI element
     */
    public void SetDifficulty(float difficulty)
    {
        DifficultySliderFill.fillAmount = difficulty / 10;
    }

    /**
     * Reloads the level and updates the list of all levels given the new meta file
     * Used to attempt to fix warnings
     */
    public void ReloadLevel()
    {
        LevelLoader.ReloadLevelByName(TextLevelName.text.ToString());
        LevelFinder.ReloadLevelList();
    }

    /**
     * If there is no warning start the level when clicked
     */
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        // if no error start level
        if(parserResult == 0)
        {
            // start level
            GameStarter.Instance.StartLevel(TextLevelName.text);
        }
    }
}
