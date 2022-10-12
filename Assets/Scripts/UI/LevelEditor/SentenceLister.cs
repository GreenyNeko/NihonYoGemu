using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SentenceLister : MonoBehaviour
{
    public GameObject PrefabButtonAddSentence;
    public GameObject PrefabEditorSentence;

    public GameObject Content;
    public EditorManager ScriptEditorManager;

    List<GameObject> uiSentences = new List<GameObject>();

    List<UnityAction> actionsMoveUp = new List<UnityAction>(), actionsMoveDown = new List<UnityAction>();

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
        if(index >= 0 && index < uiSentences.Count - 1)
        {
            return uiSentences[index].GetComponent<UIEditorSentence>().GetReadings();
        }
        return null;
    }

    /**
     * Populate with the existing sentences
     */
    public void Populate(Level level)
    {
        // create sentence objects
        int kanjiCount = 0;
        for (int i = 0; i < level.GetSentenceCount(); i++)
        {
            GameObject uiSentence = Instantiate(PrefabEditorSentence, Content.transform);
            uiSentence.GetComponent<UIEditorSentence>().ScriptSentenceLister = this;

            if (i > 0)
            {
                int a = i;
                actionsMoveUp.Add(new UnityAction(delegate { SwapElements(a, a - 1); }));
            }
            else
            {
                actionsMoveUp.Add(new UnityAction(delegate { }));
            }
            if (i < level.GetSentenceCount() - 1)
            {
                int a = i;
                actionsMoveDown.Add(new UnityAction(delegate { SwapElements(a, a + 1); }));
            }
            else
            {
                actionsMoveDown.Add(new UnityAction(delegate { }));
            }
            uiSentence.GetComponent<UIEditorSentence>().RegisterButtonMoveUpCallback(actionsMoveUp[i]);
            uiSentence.GetComponent<UIEditorSentence>().RegisterButtonMoveDownCallback(actionsMoveDown[i]);
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
        UpdateFurigana();
    }

    /**
     * Adds a new ui sentence object to the list of sentence objects
     */
    public void AddNewSentence()
    {
        // add new sentences
        GameObject uiSentence = Instantiate(PrefabEditorSentence, Content.transform);
        uiSentence.GetComponent<UIEditorSentence>().ScriptSentenceLister = this;
        if (uiSentences.Count - 3 > 0)
        {
            int fixedInt = uiSentences.Count;
            actionsMoveUp.Add(new UnityAction(delegate { SwapElements(fixedInt + 1 - 2, fixedInt + 1 - 3); }));
        }
        else
        {
            actionsMoveUp.Add(new UnityAction(delegate { }));
        }
        actionsMoveDown.Add(new UnityAction(delegate { }));
        uiSentence.GetComponent<UIEditorSentence>().RegisterButtonMoveUpCallback(actionsMoveUp[uiSentences.Count + 1 - 2]);
        uiSentence.GetComponent<UIEditorSentence>().RegisterButtonMoveDownCallback(actionsMoveDown[uiSentences.Count + 1 - 2]);
        uiSentences.Insert(uiSentences.Count - 1, uiSentence);
        UpdateElementPositioning();
        ScriptEditorManager.NotifyOfChanges();
        // update previous move down action
        if (uiSentences.Count > 2)
        {
            var prevUISentence = uiSentences[uiSentences.Count - 3].GetComponent<UIEditorSentence>();
            prevUISentence.UnregisterButtonMoveDownCallback(actionsMoveDown[uiSentences.Count - 3]);
            int fixedInt = uiSentences.Count;
            actionsMoveDown[uiSentences.Count - 3] = (new UnityAction(delegate { SwapElements(fixedInt - 3, fixedInt - 2); }));
            prevUISentence.RegisterButtonMoveDownCallback(actionsMoveDown[uiSentences.Count - 3]);
        }
    }

    public void DeleteSentenceAt(int index)
    {
        // remove from list and destroy the gameobject
        GameObject uiGameObject = uiSentences[index];
        uiSentences.RemoveAt(index);
        bool lastElement = index == uiSentences.Count - 2;
        // if last element was deleted
        if (lastElement && uiSentences.Count > 1)
        {
            // unregister down event of the new last element
            uiSentences[index - 1].GetComponent<UIEditorSentence>().UnregisterButtonMoveDownCallback(actionsMoveDown[index - 1]);
            // register empty
            actionsMoveDown[index - 1] = (new UnityAction(delegate { }));
            uiSentences[index - 1].GetComponent<UIEditorSentence>().RegisterButtonMoveDownCallback(actionsMoveDown[index - 1]);
        }
        // update all further events
        for (int i = 0; i + index < uiSentences.Count - 1; i++)
        {
            // unregister current events
            uiSentences[i + index].GetComponent<UIEditorSentence>().UnregisterButtonMoveUpCallback(actionsMoveUp[i + index + 1]);
            uiSentences[i + index].GetComponent<UIEditorSentence>().UnregisterButtonMoveDownCallback(actionsMoveDown[i + index + 1]);

            // register the correct ones respectively
            uiSentences[i + index].GetComponent<UIEditorSentence>().RegisterButtonMoveUpCallback(actionsMoveUp[i + index]);
            // update last element event
            if (i + index == uiSentences.Count - 2)
            {
                actionsMoveDown[i + index] = (new UnityAction(delegate { }));
            }
            uiSentences[i + index].GetComponent<UIEditorSentence>().RegisterButtonMoveDownCallback(actionsMoveDown[i + index]);
        }
        // remove last actions (left over) | the order is important here for unregistering events!
        actionsMoveDown.RemoveAt(actionsMoveDown.Count - 1);
        actionsMoveUp.RemoveAt(actionsMoveUp.Count - 1);
        Destroy(uiGameObject);
        UpdateElementPositioning();
        ScriptEditorManager.NotifyOfChanges();
    }

    void UpdateElementPositioning()
    {
        float elementHeight = uiSentences[0].GetComponent<RectTransform>().rect.height;
        for (int i = 0; i < uiSentences.Count; i++)
        {
            // if it is an UIEditorSentence object
            if (uiSentences[i].GetComponent<UIEditorSentence>() != null)
            {
                // set its index
                uiSentences[i].GetComponent<UIEditorSentence>().index = i;
            }
            uiSentences[i].transform.localPosition = new Vector3(uiSentences[i].transform.localPosition.x, -elementHeight * i, uiSentences[i].transform.localPosition.z);
        }
        // update the scrolling size of the parent to fix a unity issue with updating content in a scroll view
        Content.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (uiSentences.Count) * elementHeight);
    }

    /**
     * Tell the editor manager that the given sentence will be edited
     */
    public void EditSentence(UIEditorSentence uiEditorSentence)
    {
        // pass information to editor to edit
        ScriptEditorManager.EditSentence(uiEditorSentence);
    }

    /**
     * Update the furigana of each sentence
     */
    public void UpdateFurigana()
    {
        for(int i = 0; i < uiSentences.Count - 1; i++)
        {
            uiSentences[i].GetComponent<UIEditorSentence>().UpdateFuriganas();
        }
    }

    private void SwapElements(int idx1, int idx2)
    {
        // unregister current events
        uiSentences[idx1].GetComponent<UIEditorSentence>().UnregisterButtonMoveUpCallback(actionsMoveUp[idx1]);
        uiSentences[idx1].GetComponent<UIEditorSentence>().UnregisterButtonMoveDownCallback(actionsMoveDown[idx1]);
        uiSentences[idx2].GetComponent<UIEditorSentence>().UnregisterButtonMoveUpCallback(actionsMoveUp[idx2]);
        uiSentences[idx2].GetComponent<UIEditorSentence>().UnregisterButtonMoveDownCallback(actionsMoveDown[idx2]);
        // swap elements
        GameObject temp = uiSentences[idx1];
        uiSentences[idx1] = uiSentences[idx2];
        uiSentences[idx2] = temp;
        // reregister events
        uiSentences[idx1].GetComponent<UIEditorSentence>().RegisterButtonMoveUpCallback(actionsMoveUp[idx1]);
        uiSentences[idx1].GetComponent<UIEditorSentence>().RegisterButtonMoveDownCallback(actionsMoveDown[idx1]);
        uiSentences[idx2].GetComponent<UIEditorSentence>().RegisterButtonMoveUpCallback(actionsMoveUp[idx2]);
        uiSentences[idx2].GetComponent<UIEditorSentence>().RegisterButtonMoveDownCallback(actionsMoveDown[idx2]);
        // update index and visuals
        UpdateElementPositioning();
        // notify of level change
        ScriptEditorManager.NotifyOfChanges();
    }
       
}
