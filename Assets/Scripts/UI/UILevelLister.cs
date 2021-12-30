using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Script that handles the list of all UI levels
 */
public class UILevelLister : MonoBehaviour
{
    public Transform UILevelParent;     // where to instantiate them to
    public GameObject UILevelPrefab;    // the prefab used to create new elements
    List<GameObject> uiLevelObjects;    // a reference to all objects created

    // Start is called before the first frame update
    void Start()
    {
        // init list
        uiLevelObjects = new List<GameObject>();
        createUILevelObjects();
    }

    /**
     * Clears all elements and loads them again
     */
    public void ReloadLevelList()
    {
        // remove all elements
        uiLevelObjects.Clear();
        // load all elements again
        createUILevelObjects();
    }

    // Creates all UIlevel elements given the meta files that have been loaded
    void createUILevelObjects()
    {
        // use the level meta to create the UILevel objects
        for (int i = 0; i < LevelLoader.GetLevelMetaCount(); i++)
        {
            LevelMeta meta = LevelLoader.GetLevelMetaByIdx(i);
            GameObject UILevelObject = Instantiate(UILevelPrefab, UILevelParent);
            // copy over attributes from meta
            UILevel uiLevel = UILevelObject.GetComponent<UILevel>();
            uiLevel.SetAuthorName(meta.GetAuthor());
            uiLevel.SetLevelName(meta.GetLevelName());
            uiLevel.SetDifficulty(meta.GetDifficulty());
            uiLevel.LevelFinder = this;
            // flag if level sentence too long
            uiLevel.SetParserResult(meta.GetParserResult());
            // place the ui object at the right spot in the content view
            UILevelObject.transform.localPosition += new Vector3(0, -100 * i, 0);
            // store reference
            uiLevelObjects.Add(UILevelObject);
        }
        // update the scrolling size of the parent to fix a unity issue with updating content in a scroll view
        UILevelParent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LevelLoader.GetLevelMetaCount() * 100);
    }
}
