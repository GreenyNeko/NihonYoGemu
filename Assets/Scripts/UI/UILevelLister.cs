using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Script that handles the list of all UI levels
 */
public class UILevelLister : MonoBehaviour
{ 
    // the manager of the level select scene
    public LevelSelectManager ScriptLevelSelectManager;

    public GameObject NormalMenu;               // the menu that is usually used
    public GameObject LoadingScreen;            // the menu or screen when reloading
    public Transform UILevelParent;             // where to instantiate them to
    public GameObject UILevelPrefab;            // the prefab used to create new elements
    public UILeaderboard ScriptUILeaderboard;   // reference to the leaderboard script to update it
    public GameObject PlayButton;               // reference to the play button to enable/disable it
    
    List<GameObject> uiLevelObjects;    // a reference to all objects created
    int idCurrentlySelected;            // the id of the gameobject that is currently selected
    string filter = "";                 // stores the filter to apply it when creating UI objects
    bool sortAscending = true;          // stores whether or not the sort is ascending or descending
    int sortMode = 0;                   // stores the sort mode so we need no reference
    IEnumerator loadingEnumerator;      // the current coroutine to work on
    bool reloadingLevels = false;       // used to disable input

    // Start is called before the first frame update
    void Start()
    {
        // init list
        uiLevelObjects = new List<GameObject>();
        idCurrentlySelected = -1;
        PlayButton.GetComponent<Button>().interactable = false;
        LoadingScreen.SetActive(false);
        createUILevelObjects();
    }

    void Update()
    {
        // we don't wanna reload if we're already reloading...
        if(!reloadingLevels)
        {
            // handle shortcuts
            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    // Shift-F5, Ctrl-F5
                    StartCoroutine(ReloadLevels(false));
                }
                else
                {
                    // F5
                    StartCoroutine(ReloadLevels(true));
                }
            }
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if(Input.GetKeyDown(KeyCode.R))
                    {
                        // Ctrl-Shift-R
                        StartCoroutine(ReloadLevels(false));
                    }
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    // Ctrl-R
                    StartCoroutine(ReloadLevels(true));
                }
            }
        }
    }

    /**
     * 
     */
    public void FlagLevelAsSelected(GameObject level)
    {
        // enable play button now that a level is active
        PlayButton.GetComponent<Button>().interactable = true;
        int prevSelected = idCurrentlySelected;
        // unselect the previous level
        if (idCurrentlySelected >= 0 && uiLevelObjects[idCurrentlySelected].gameObject != level.gameObject)
        {
            uiLevelObjects[idCurrentlySelected].GetComponent<UILevel>().Unselect();
        }
        // select the new one
        idCurrentlySelected = uiLevelObjects.IndexOf(level);
        // only update if leaderboard is enabled
        if(ScriptUILeaderboard.isActiveAndEnabled)
        {
            // only update if a new level has been selected
            if (prevSelected >= 0 && uiLevelObjects[prevSelected].gameObject != level.gameObject)
            {
                ScriptUILeaderboard.UpdateLeaderboard(level.GetComponent<UILevel>().TextLevelName.text);
            }
            else if (prevSelected < 0)
            {
                ScriptUILeaderboard.UpdateLeaderboard(level.GetComponent<UILevel>().TextLevelName.text);
            }
        }
    }

    /**
     * Changes whether the sort is ascending or descending
     */
    public void ToggleSortOrder()
    {
        sortAscending = !sortAscending;
        SortLevels(sortMode);
    }

    /**
     * sorts the levels given the sort mode
     */
    public void SortLevels(int sortMode)
    {
        this.sortMode = sortMode;
        switch (sortMode)
        {
            case 2:     // sort by difficulty
                uiLevelObjects.Sort((go1, go2) => { return go1.GetComponent<UILevel>().DifficultySliderFill.fillAmount.CompareTo(go2.GetComponent<UILevel>().DifficultySliderFill.fillAmount); });
                break;
            case 1:     // sort by author
                uiLevelObjects.Sort((go1, go2) => { return go1.GetComponent<UILevel>().TextAuthorName.text.CompareTo(go2.GetComponent<UILevel>().TextAuthorName.text); });
                break;
            default:    // sort by name
                uiLevelObjects.Sort((go1, go2) => { return go1.GetComponent<UILevel>().TextLevelName.text.CompareTo(go2.GetComponent<UILevel>().TextLevelName.text); });
                break;
        }
        for (int i = 0; i < uiLevelObjects.Count; i++)
        {
            // determine the order at which we get the objects
            GameObject obj;
            if (sortAscending)
            {
                obj = uiLevelObjects[i];
            }
            else
            {
                obj = uiLevelObjects[uiLevelObjects.Count - i - 1];
            }
            // place the ui object at the right spot in the content view
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, -100 * i, obj.transform.localPosition.z);
        }
    }

    /**
     * filters the levels given the text
     */
    public void FilterLevels(string filterText)
    {
        filter = filterText;
        ReloadLevelList();
    }

    /**
     * Clears all elements and loads them again
     */
    public void ReloadLevelList()
    {
        // clean up game objects
        for(int i = 0; i < uiLevelObjects.Count; i++)
        {
            Destroy(uiLevelObjects[i]);
        }
        // remove all elements
        uiLevelObjects.Clear();
        // load all elements again
        createUILevelObjects();
    }

    // Creates all UIlevel elements given the meta files that have been loaded
    void createUILevelObjects()
    {
        float starsBiggerThan = -1;
        float starsSmallerThan = -1;
        int kanjiMoreThan = -1;
        int kanjiLessThan = -1;
        int charsMoreThan = -1;
        int charsLessThan = -1;
        // init values from filter
        // doing this saves on performance since these stay the same during the creationg of the UI level objects
        {
            if (filter.Contains("stars>"))
            {
                int idx = filter.IndexOf("stars>");
                // if there's a space after the stars>
                if (filter.Substring(idx).Contains(" "))
                {
                    // get the value between the space and the stars>
                    string fullAdvFilterStr = filter.Substring(idx + 6, filter.IndexOf(" ", idx) - (idx + 6));
                    float.TryParse(fullAdvFilterStr, out starsBiggerThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6 + 1);
                }
                else
                {
                    // we assume the string ends after our advanced filter
                    string fullAdvFilterStr = filter.Substring(idx + 6);
                    float.TryParse(fullAdvFilterStr, out starsBiggerThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6);
                }
            }
            if(filter.Contains("stars<"))
            {
                int idx = filter.IndexOf("stars<");
                starsSmallerThan = -1;
                if (filter.Substring(idx).Contains(" "))
                {
                    // get the value between the space and the stars>
                    string fullAdvFilterStr = filter.Substring(idx + 6, filter.IndexOf(" ", idx) - (idx + 6));
                    float.TryParse(fullAdvFilterStr, out starsSmallerThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6 + 1);
                }
                else
                {
                    // we assume the string ends after our advanced filter
                    string fullAdvFilterStr = filter.Substring(idx + 6);
                    float.TryParse(fullAdvFilterStr, out starsSmallerThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6);
                }
            }
            if (filter.Contains("kanji>"))
            {
                int idx = filter.IndexOf("kanji>");
                kanjiMoreThan = -1;
                if (filter.Substring(idx).Contains(" "))
                {
                    // get the value between the space and the stars>
                    string fullAdvFilterStr = filter.Substring(idx + 6, filter.IndexOf(" ", idx) - (idx + 6));
                    int.TryParse(fullAdvFilterStr, out kanjiMoreThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6 + 1);
                }
                else
                {
                    // we assume the string ends after our advanced filter
                    string fullAdvFilterStr = filter.Substring(idx + 6);
                    int.TryParse(fullAdvFilterStr, out kanjiMoreThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6);
                }
            }
            if (filter.Contains("kanji<"))
            {
                int idx = filter.IndexOf("kanji<");
                kanjiLessThan = -1;
                if (filter.Substring(idx).Contains(" "))
                {
                    // get the value between the space and the stars>
                    string fullAdvFilterStr = filter.Substring(idx + 6, filter.IndexOf(" ", idx) - (idx + 6));
                    int.TryParse(fullAdvFilterStr, out kanjiLessThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6 + 1);
                }
                else
                {
                    // we assume the string ends after our advanced filter
                    string fullAdvFilterStr = filter.Substring(idx + 6);
                    int.TryParse(fullAdvFilterStr, out kanjiLessThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6);
                }
            }
            if (filter.Contains("chars>"))
            {
                int idx = filter.IndexOf("chars>");
                charsMoreThan = -1;
                if (filter.Substring(idx).Contains(" "))
                {
                    // get the value between the space and the stars>
                    string fullAdvFilterStr = filter.Substring(idx + 6, filter.IndexOf(" ", idx) - (idx + 6));
                    int.TryParse(fullAdvFilterStr, out charsMoreThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6 + 1);
                }
                else
                {
                    // we assume the string ends after our advanced filter
                    string fullAdvFilterStr = filter.Substring(idx + 6);
                    int.TryParse(fullAdvFilterStr, out charsMoreThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6);
                }
            }
            if (filter.Contains("chars<"))
            {
                int idx = filter.IndexOf("chars<");
                charsLessThan = -1;
                if (filter.Substring(idx).Contains(" "))
                {
                    // get the value between the space and the stars>
                    string fullAdvFilterStr = filter.Substring(idx + 6, filter.IndexOf(" ", idx) - (idx + 6));
                    int.TryParse(fullAdvFilterStr, out charsLessThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6 + 1);
                }
                else
                {
                    // we assume the string ends after our advanced filter
                    string fullAdvFilterStr = filter.Substring(idx + 6);
                    int.TryParse(fullAdvFilterStr, out charsLessThan);
                    // remove the advanced filter from the filter
                    filter = filter.Remove(idx, fullAdvFilterStr.Length + 6);
                }
            }
        }

        int skippedObjects = 0;
        // use the level meta to create the UILevel objects
        for (int i = 0; i < LevelLoader.GetLevelMetaCount(); i++)
        {
            LevelMeta meta = LevelLoader.GetLevelMetaByIdx(i);
            // filters || collapse if not needed
            {
                // only filter if there's a filter text
                bool filterThis = true;
                // normal filters
                if (filter.Length > 0)
                {
                    // filter whatever doesn't contain this
                    if (meta.GetLevelName().Contains(filter))
                    {
                        filterThis = false;
                    }
                    // level doesn't contain author in the filter
                    if (meta.GetAuthor().Contains(filter))
                    {
                        filterThis = false;
                    }
                }
                else
                {
                    filterThis = false;
                }

                // advanced filters
                {
                    // if it hasn't been filtered out yet
                    if (!filterThis)
                    {
                        // filter out anything with less difficulty, filter by more difficulty
                        if (starsBiggerThan >= 0 && meta.GetDifficulty() < starsBiggerThan)
                        {
                            filterThis = true;
                        }
                        // filter out anything with more difficulty, filter by less difficulty
                        if (starsSmallerThan >= 0 && meta.GetDifficulty() > starsSmallerThan)
                        {
                            filterThis = true;
                        }
                        // filter out anything with more characters, filter for less characters
                        if(charsLessThan >= 0 && meta.GetTotalLength() > charsLessThan)
                        {
                            filterThis = true;
                        }
                        // filter out anything with less characters, filter for more characters
                        if (charsMoreThan >= 0 && meta.GetTotalLength() < charsMoreThan)
                        {
                            filterThis = true;
                        }
                        // filter out anything with more kanji, filter for less kanji
                        if(kanjiLessThan >= 0 && meta.GetKanjiCount() > kanjiLessThan)
                        {
                            filterThis = true;
                        }
                        // filter out anything with less kanji, filter for more kanji
                        if(kanjiMoreThan >= 0 && meta.GetKanjiCount() < kanjiMoreThan)
                        {
                            filterThis = true;
                        }
                    }
                }

                // okay filter this out
                if (filterThis)
                {
                    skippedObjects++;
                    continue;
                }
            }
            
            GameObject UILevelObject = Instantiate(UILevelPrefab, UILevelParent);
            // copy over attributes from meta
            UILevel uiLevel = UILevelObject.GetComponent<UILevel>();
            uiLevel.ScriptLevelSelectManager = ScriptLevelSelectManager;
            uiLevel.SetAuthorName(meta.GetAuthor());
            uiLevel.SetLevelName(meta.GetLevelName());
            uiLevel.SetDifficulty(meta.GetDifficulty());
            uiLevel.SetLength((int)meta.GetLevelLength());
            uiLevel.LevelFinder = this;
            // flag if level sentence too long
            uiLevel.SetParserResult(meta.GetParserResult());
            // place the ui object at the right spot in the content view
            UILevelObject.transform.localPosition += new Vector3(0, -100 * (i - skippedObjects), 0);
            // store reference
            uiLevelObjects.Add(UILevelObject);
        }
        // update the scrolling size of the parent to fix a unity issue with updating content in a scroll view
        UILevelParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (LevelLoader.GetLevelMetaCount() - skippedObjects) * 100);
    }

    // used to soft or hard reload levels changes the menu while loading to disable input
    IEnumerator ReloadLevels(bool soft)
    {
        // unselect selected level
        if(idCurrentlySelected >= 0)
        {
            uiLevelObjects[idCurrentlySelected].GetComponent<UILevel>().Unselect();
            idCurrentlySelected = -1;
        }
        // clean up old list
        for (int i = 0; i < uiLevelObjects.Count; i++)
        {
            Destroy(uiLevelObjects[i]);
        }
        // remove all elements
        uiLevelObjects.Clear();
        // update menu and lock other things
        reloadingLevels = true;
        NormalMenu.SetActive(false);
        LoadingScreen.SetActive(true);
        // for passing on the result to us and evaluating it for feedback
        object result;
        // select the enumerator given how we want to reload levels
        if (!soft)
        {
            loadingEnumerator = LevelLoader.HardReloadLevels();
        }
        else
        {
            loadingEnumerator = LevelLoader.SoftReloadLevels();
        }
        // actually reload the levels
        while (loadingEnumerator.MoveNext())
        {
            // get the result
            result = loadingEnumerator.Current;
            // TODO: evaluate result do something with it
            yield return 0; // give control back to the program
        }
        // we're done change back to previous
        // update menu and lock other things
        reloadingLevels = false;
        NormalMenu.SetActive(true);
        LoadingScreen.SetActive(false);
        // recreate ui
        createUILevelObjects();
    }
}
