using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    // references to different menues
    public GameObject WindowLevelCreator;
    public GameObject LevelEditorMenu;
    public GameObject TextLevelName;
    public GameObject SentenceEditorMenu;
    public GameObject UnsavedChangesWindow;

    // reference to save button
    public Button ButtonSave;

    // references to sentence editor elements
    public TMPro.TMP_Text TextSnippet;
    public TMPro.TMP_InputField InputSentence;
    public TMPro.TMP_InputField InputReading;

    // reference to sentence lister
    public SentenceLister ScriptSentenceLister;

    string prevSentence;
    (int, string)[] prevReadings;
    string levelName; // the level name
    bool unsavedChanges;
    int currentReading;
    UIEditorSentence sentenceToEdit;

    void Awake()
    {
        // grab data, if the name is empty it's a new level
        MemoryStream memoryStream = new MemoryStream(SceneManagerPlus.GetCurrentData());
        using(BinaryReader reader = new BinaryReader(memoryStream))
        {
            reader.ReadInt32(); // previous scene, unused here
            reader.ReadBoolean(); // editor flag, unused here
            levelName = reader.ReadString();
            reader.ReadInt32(); // mods, unused here
        }
        // if name is empty we create a new level
        if(levelName == "")
        {
            WindowLevelCreator.SetActive(true);
        }
        else
        {
            WindowLevelCreator.SetActive(false);
            // load the level and populate scene
        }
        LevelEditorMenu.SetActive(true);
        SentenceEditorMenu.SetActive(false);
        UnsavedChangesWindow.SetActive(false);
        ButtonSave.interactable = false;
    }

    void Start()
    {
        if(levelName != "")
        {
            Level level = LevelLoader.LoadLevelByName(levelName);
            ScriptSentenceLister.Populate(level);
        }
        else
        {
            ScriptSentenceLister.PopulateEmpty();
        }
        
    }

    /**
     * <summary>Attempt to leave the editor but ask user if there are unsaved changes</summary>
     */
    public void TryLeavingEditor()
    {
        if(!unsavedChanges)
        {
            LeaveEditor();
        }
        else
        {
            UnsavedChangesWindow.SetActive(true);
        }
    }

    public void LeaveEditor()
    {
        // warn if unsaved changes
        MemoryStream memoryStream = new MemoryStream();
        using(BinaryWriter writer = new BinaryWriter(memoryStream))
        {
            writer.Write(SceneUtility.GetBuildIndexByScenePath("Scenes/EditorScene"));
            writer.Write(true); // we come from editor so we're in editor mode
            writer.Write(levelName);
            writer.Write(0);
        }
        SceneManagerPlus.LoadScene("LevelSelect", memoryStream.ToArray());
    }

    /**
     * <summary>Saves the level</summary>
     */
    public void Save()
    {
        unsavedChanges = false;
        ButtonSave.interactable = false;
        // save in file
    }

    /**
     * <summary>Sets the level name</summary>
     */
    public void SetName(TMPro.TMP_Text inputFieldName)
    {
        levelName = inputFieldName.text;
        TextLevelName.GetComponent<TMPro.TMP_Text>().SetText(levelName);
    }

    /**
     * <summary>Flags the file to contain changes</summary>
     */
    public void NotifyOfChanges()
    {
        unsavedChanges = true;
        ButtonSave.interactable = true;
    }

    /**
     * <summary>Starts the screen to edit the sentence and then applies the changes to the provided object.</summary>
     */
    public void EditSentence(UIEditorSentence uiEditorSentence)
    {
        currentReading = 0;
        sentenceToEdit = uiEditorSentence;
        prevSentence = sentenceToEdit.TextSentence.GetParsedText();
        prevReadings = sentenceToEdit.GetReadings();
        LevelEditorMenu.SetActive(false);
        SentenceEditorMenu.SetActive(true);
        string sentence = sentenceToEdit.TextSentence.GetParsedText();
        InputSentence.SetTextWithoutNotify(sentence);
        UpdateReading();
    }

    /**
     * <summary>Updates the readings given the new sentence</summary>
     */
    public void UpdateSentence(string NewSentence)
    {
        string prevSentence = sentenceToEdit.TextSentence.text;
        int kanjiCountPrev = 0;
        // needed for context
        for(int i = 0; i < this.prevSentence.Length; i++)
        {
            if(JapaneseDictionary.IsKanji(this.prevSentence[i].ToString()))
            {
                kanjiCountPrev++;
            }
        }
        var newReadings = new List<(int, string)>();
        sentenceToEdit.TextSentence.SetText(NewSentence);
        // create new readings for each kanji
        for(int i = 0; i < NewSentence.Length; i++)
        {
            if(JapaneseDictionary.IsKanji(NewSentence[i].ToString()))
            {
                newReadings.Add((i, ""));
            }
        }
        // if kanji acount is same
        if(newReadings.Count == kanjiCountPrev)
        {
            // assuming only non kanji were changed => copy readings over
            for(int i = 0; i < newReadings.Count; i++)
            {
                newReadings[i] = (newReadings[i].Item1, sentenceToEdit.GetReading(i).Item2);
            }
        }
        
        sentenceToEdit.SetKanjiReadings(newReadings.ToArray());
        currentReading = 0;
        UpdateReading();
    }

    /**
     * <summary>Save the current sentence changes</summary>
     */
    public void SaveSentence()
    {
        NotifyOfChanges();
    }

    /**
     * <summary>Saves the current reading given the current kanji reading being edited</summary>
     */
    public void SaveReading(string reading)
    {
        sentenceToEdit.SetKanjiReading(currentReading, reading);
    }

    /**
     * <summary>Cancel editing the sentence and don't change</summary>
     */
    public void UndoSentenceChanges()
    {
        sentenceToEdit.TextSentence.SetText(prevSentence);
        sentenceToEdit.SetKanjiReadings(prevReadings);
    }

    /**
     * Moves the reading over
     */
    public void NextReading()
    {
        currentReading = Mathf.Clamp(currentReading + 1, 0, sentenceToEdit.GetReadingCount() - 1);
        UpdateReading();
    }

    /**
     * Moves the reading over
     */
    public void PrevReading()
    {
        currentReading = Mathf.Clamp(currentReading - 1, 0, sentenceToEdit.GetReadingCount() - 1);
        UpdateReading();
    }

    /**
     * <summary>Update the reading UI in the sentence editor</summary>
     */
    void UpdateReading()
    {
        // update all reading related fields
        string sentence = sentenceToEdit.TextSentence.text;
        var kanjiReading = sentenceToEdit.GetReading(currentReading);
        InputReading.SetTextWithoutNotify(kanjiReading.Item2);
        // substring starting at -2 going 5 chars, keeping invalid access in mind
        TextSnippet.SetText(sentence.Substring(Mathf.Clamp(kanjiReading.Item1 - 2, 0, sentence.Length - 1), Mathf.Clamp(sentence.Length - kanjiReading.Item1, 0, 5)));
    }
}
