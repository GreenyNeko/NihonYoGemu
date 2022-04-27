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
            (int, string)[] readingsWithPos = new (int, string)[level.GetKanjiFromSentence(i).Length];
            for (int j = 0; j < level.GetKanjiFromSentence(i).Length; j++)
            {
                int position = level.GetLine(i).IndexOf(level.GetKanjiFromSentence(i)[j]);
                string reading = level.GetFuriganaFromKanji(j + kanjiCount);
                readingsWithPos[j] = (position, reading);
            }
            kanjiCount += level.GetKanjiFromSentence(i).Length;
            scriptRef.SetKanjiReadings(readingsWithPos);
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

    /**
     * <summary>Returns an array of the sentences</summary>
     */
    public List<string> GetSentences()
    {
        // converts the gameobjects to the strings contained in their UIEditorSentences
        return uiSentences.ConvertAll<string>(x =>
        {
            if (x.GetComponent<UIEditorSentence>() != null)
            {
                return x.GetComponent<UIEditorSentence>().TextSentence.text;
            }
            else
            {
                return "";
            }
        });
    }

    /**
     * <summary>Returns an array of readings of the xths sentence</summary>
     */
    public (int, string)[] GetReadings(int index)
    {
        // -1 because the last element is not a UIEditorSentence but the add sentence button
        if(index > 0 && index < uiSentences.Count - 1)
        {
            return uiSentences[index].GetComponent<UIEditorSentence>().GetReadings();
        }
        return null;
    }
}
