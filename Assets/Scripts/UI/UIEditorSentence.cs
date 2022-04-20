using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIEditorSentence : MonoBehaviour
{
    // reference to the ui lister / innitiator
    public SentenceLister ScriptSentenceLister;

    // reference to the text element
    public GameObject TextSentence;

    // index given to it by uilister
    public int index;

    // the attributes of this ui editor sentence
    string sentence;
    List<string> readings;

    /**
     * Sets the sentence for this editor sentence
     */
    public void SetSentence(string sentence)
    {
        this.sentence = sentence;
        TextSentence.GetComponent<TMP_Text>().SetText(sentence);
    }

    /**
     * Sets the readings for this sentence
     */
    public void SetReadings(string[] readings)
    {
        this.readings = new List<string>(readings);
        // TODO: integrate the readings into the preview...
    }

    /**
     * Delete this sentence
     */
    public void Delete()
    {
        ScriptSentenceLister.DeleteSentenceAt(index);
    }

    /**
     * Edit this sentence
     */
    public void Edit()
    {
        ScriptSentenceLister.EditSentence(this);
        // TODO: how to integrate this?
    }
}
