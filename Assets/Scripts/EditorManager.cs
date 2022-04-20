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

    // reference to sentence lister
    public SentenceLister ScriptSentenceLister;

    string levelName; // the level name
    bool unsavedChanges;
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
    public void EditSentence(UIEditorSentence uIEditorSentence)
    {
        // TODO: implement
    }
}
