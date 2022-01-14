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
    public Color colorUnselected;       // the color when the level is not sleected
    public Color colorSelected;         // the color when the level is selected
    public GameObject Panel;            // the panel UI element holding everything
    public TMP_Text TextLevelName;      // the text element that shows the name
    public TMP_Text TextAuthorName;     // the text element that shows the author name
    public TMP_Text TextLength;         // the text element that shows the length of the level
    public Image DifficultySliderFill;  // the image element that controls the difficulty slider
    public Image Warning;               // the image element showing if a warning occured
    public GameObject Tooltip;          // the tooltip element showing the warning message
    public UILevelLister LevelFinder;   // The parent that creates these objects

    int parserResult;                   // the result of the level parsing
    float timeSinceLastClick;           // used for double click


    // Start is called before the first frame update
    void Start()
    {
        // by default warning and tooltip should not be visible
        Warning.gameObject.SetActive(false);
        Tooltip.gameObject.SetActive(false);
        Panel.GetComponent<Image>().color = colorUnselected;
        timeSinceLastClick = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastClick += Time.deltaTime;
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
     * Sets the length of the level
     */
    public void SetLength(int length)
    {
        TextLength.SetText(length.ToString());
    }

    /**
     * When this UILevel should no longer be selected
     */
    public void Unselect()
    {
        Panel.GetComponent<Image>().color = colorUnselected;
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
        // if no error select level
        if(parserResult == 0)
        {
            // select this level
            Panel.GetComponent<Image>().color = colorSelected;
            GameStarter.Instance.SetLevelByName(TextLevelName.text);
            LevelFinder.FlagLevelAsSelected(gameObject);
            // double click
            if (timeSinceLastClick < 0.5f)
            {
                GameStarter.Instance.StartLevel();
            }
            // reset time since last click
            timeSinceLastClick = 0;
        }
    }
}
