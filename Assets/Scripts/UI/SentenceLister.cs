using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentenceLister : MonoBehaviour
{
    public GameObject PrefabButtonAddSentence;
    public GameObject PrefabEditorSentence;

    public GameObject Content;
    public EditorManager ScriptEditorManager;

    List<GameObject> uiSentences = new List<GameObject>();

    /**
     * Populate with the existing sentences
     */
    public void Populate(Level level)
    {
        // create sentence objects
        int kanjiCount = 0;
        for(int i = 0; i < level.GetSentenceCount(); i++)
        {
            GameObject uiSentence = Instantiate(PrefabEditorSentence, Content.transform);
            uiSentence.GetComponent<UIEditorSentence>().ScriptSentenceLister = this;
            var scriptRef = uiSentence.GetComponent<UIEditorSentence>();
            scriptRef.SetSentence(level.GetLine(i));
            List<string> readings = new List<string>();
            for(int j = 0; j < level.GetKanjiFromSentence(i).Length; j++)
            {
                readings.Add(level.GetFuriganaFromKanji(j + kanjiCount));
            }
            kanjiCount += level.GetKanjiFromSentence(i).Length;
            scriptRef.SetReadings(readings.ToArray());
            uiSentences.Add(uiSentence);
        }
        PopulateEmpty();
        UpdateElementPositioning();
    }

    /**
     * Populate the lister without a level or add the add sentence button at its respective place
     */
    public void PopulateEmpty()
    {
        // create final button
        GameObject buttonAddSentence = Instantiate(PrefabButtonAddSentence, Content.transform);
        buttonAddSentence.GetComponent<ButtonAddSentence>().ScriptSentenceLister = this;
        uiSentences.Add(buttonAddSentence);
    }

    /**
     * Adds a new ui sentence object to the list of sentence objects
     */
    public void AddNewSentence()
    {
        GameObject uiSentence = Instantiate(PrefabEditorSentence, Content.transform);
        uiSentence.GetComponent<UIEditorSentence>().ScriptSentenceLister = this;
        uiSentences.Insert(uiSentences.Count - 1, uiSentence);
        UpdateElementPositioning();
        ScriptEditorManager.NotifyOfChanges();
    }

    public void DeleteSentenceAt(int index)
    {
        // remove from list and destroy the gameobject
        GameObject uiGameObject = uiSentences[index];
        uiSentences.RemoveAt(index);
        Destroy(uiGameObject);
        UpdateElementPositioning();
        ScriptEditorManager.NotifyOfChanges();
    }

    void UpdateElementPositioning()
    {
        for(int i = 0; i < uiSentences.Count; i++)
        {
            // if it is an UIEditorSentence object
            if(uiSentences[i].GetComponent<UIEditorSentence>() != null)
            {
                // set its index
                uiSentences[i].GetComponent<UIEditorSentence>().index = i;
            }
            uiSentences[i].transform.localPosition = new Vector3(uiSentences[i].transform.localPosition.x, -48 * i, uiSentences[i].transform.localPosition.z);
        }
        // update the scrolling size of the parent to fix a unity issue with updating content in a scroll view
        Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (uiSentences.Count) * 48);
    }

    public void EditSentence(UIEditorSentence uiEditorSentence)
    {
        // pass information to editor to edit
        ScriptEditorManager.EditSentence(uiEditorSentence);
    }
}
