using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIEditorSentence : MonoBehaviour
{
    // references to the move up and down buttons
    public GameObject ButtonMoveUp, ButtonMoveDown;

    // reference to the ui lister / innitiator
    public SentenceLister ScriptSentenceLister;

    // reference to the text element
    public TMP_Text TextSentence;



    // index given to it by uilister
    public int index;

    // reference to the script that handles furigana for editor sentences
    SentenceFurigana furiganaScript;
    // the attributes of this ui editor sentence
    string sentence;
    List<(int, string)> readings = new List<(int, string)>();

    void Awake()
    {
        furiganaScript = GetComponent<SentenceFurigana>();
    }

    public void RegisterButtonMoveUpCallback(UnityAction callback)
    {
        ButtonMoveUp.GetComponent<Button>().onClick.AddListener(callback);
    }
    
    public void RegisterButtonMoveDownCallback(UnityAction callback)
    {
        ButtonMoveDown.GetComponent<Button>().onClick.AddListener(callback);
    }

    public void UnregisterButtonMoveUpCallback(UnityAction callback)
    {
        ButtonMoveUp.GetComponent<Button>().onClick.RemoveListener(callback);
    }

    public void UnregisterButtonMoveDownCallback(UnityAction callback)
    {
        ButtonMoveDown.GetComponent<Button>().onClick.RemoveListener(callback);
    }

    /**
     * <summary>Get the account of readings</summary>
     */
    public int GetReadingCount()
    {
        return readings.Count;
    }

    /**
     * Sets the sentence for this editor sentence
     */
    public void SetSentence(string sentence)
    {
        this.sentence = sentence;
        TextSentence.SetText(sentence);
    }

    /**
     * Sets the readings for this sentence
     */
    public void SetKanjiReadings((int, string)[] readings)
    {
        this.readings = new List<(int, string)>(readings);
    }

    /**
     * Sets the readings for this sentence
     */
    public void SetKanjiReading(int index, string reading)
    {
        this.readings[index] = (this.readings[index].Item1, reading);
    }

    /**
     * Updates a reading given the index
     */
    public void UpdateReading(int index, string newReading)
    {
        this.readings[index] = (this.readings[index].Item1, newReading);
    }

    /**
     * Gets a reading from the readings 
     */
    public (int,string) GetReading(int index)
    {
        return this.readings[index];
    }

    /**
     * <summary>Returns all readings</summary>
     */
    public (int,string)[] GetReadings()
    {
        return this.readings.ToArray();
    }

    public bool HasEmptyReading()
    {
        return this.readings.Exists(x => { return x.Item2 == ""; });
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
    }

    /**
     * Update the furigana of this editor sentence
     */
    public void UpdateFuriganas()
    {
        furiganaScript.UpdateFurigana(TextSentence, readings);
    }
}
