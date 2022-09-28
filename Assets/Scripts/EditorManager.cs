using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class EditorManager : MonoBehaviour
{
    // references to different menues
    public GameObject WindowLevelCreator;
    public GameObject LevelEditorMenu;
    public GameObject SentenceEditorMenu;
    public GameObject UnsavedChangesWindow;

    // UI elements
    public GameObject ButtonSaveSentence;
    public GameObject InputLevelName;
    public GameObject InputAuthorName;
    public GameObject ButtonSaveInfo;
    public GameObject TextLevelName;
    public GameObject TextFeedbackInfo;
    public GameObject TextFeedbackSentence;

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
    string levelAuthor;
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
        // load level data
        if(levelName != "")
        {
            Level level = LevelLoader.LoadLevelByName(levelName);
            levelAuthor = level.Author;
            ScriptSentenceLister.Populate(level);
            InputLevelName.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(levelName);
            InputAuthorName.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(levelAuthor);
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
        using (StreamWriter streamWriter = new StreamWriter("Levels/" + levelName + ".nyl"))
        {
            // write user meta data
            streamWriter.Write("[author=" + levelAuthor + "]\n");
            // write sentences
            var sentences = ScriptSentenceLister.GetSentences();
            for (int i = 0; i < sentences.Count; i++)
            {
                // replace ASCII comma with japense to prevent file from breaking
                sentences[i] = sentences[i].Replace(",", "、");
                // write sentence
                streamWriter.Write(sentences[i]);
                // save readings
                (int, string)[] readings = ScriptSentenceLister.GetReadings(i);
                if(readings != null)
                {
                    foreach (var reading in readings)
                    {
                        streamWriter.Write("," + reading.Item2);
                    }
                }
                if(i < sentences.Count - 1)
                {
                    streamWriter.Write("\n");
                }
            }
        }
    }

    /**
     * <summary>Sets the level name</summary>
     */
    public void SetName(TMPro.TMP_Text inputFieldName)
    {
        levelName = inputFieldName.text;
        NotifyOfChanges();
    }

    /**
 * <summary>Sets the level name</summary>
 */
    public void SetAuthor(TMPro.TMP_Text inputFieldAuthor)
    {
        levelAuthor = inputFieldAuthor.text;
        NotifyOfChanges();
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
        UpdateButtonSaveSentence();
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
        UpdateButtonSaveSentence();
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
        UpdateButtonSaveSentence();
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
     * <summary>Updates the button used to save level info.</summary>
     */
    public void UpdateButtonSaveInfo()
    {
        bool interactable = true;
        string authorInput = InputAuthorName.GetComponent<TMPro.TMP_InputField>().text;
        string levelInput = InputLevelName.GetComponent<TMPro.TMP_InputField>().text;
        string warning = "";
        // author field empty
        if (authorInput.Length <= 0)
        {
            interactable = false;
            warning = "Author is empty\n作者名がありません。";
        }
        // author contains breaking characters
        if(authorInput.Contains("[") || authorInput.Contains("]"))
        {
            interactable = false;
            warning = "Author cannot contain \"[\" and \"]\".\n作者名に「[」と「]」は使用できません。";
        }
        // level name field empty
        if (levelInput.Length <= 0)
        {
            interactable = false;
            warning = "Level name is empty\nレベル名がありません。";
        }
        // level name contains characters incompatible with windows names
        Match match = Regex.Match(levelInput, "^([a-zA-Z0-9_ ]|-)+$");
        if (!match.Success)
        {
            interactable = false;
            warning = "Level name only supports alphanumeric, space, \"-\", and \"_\"\n作者名に英数文字とスペースと「-」と「_」は使えます。";
        }
        ButtonSaveInfo.GetComponent<Button>().interactable = interactable;
        
        // set message if there's a warning
        TextFeedbackInfo.GetComponent<TMPro.TMP_Text>().SetText(warning);
    }

    /**
     * <summary>Updates the info fields for the level</summary>
     */
    public void UpdateLevelInfoInputFields()
    {

    }

    /**
     * <summary>Update the reading UI in the sentence editor</summary>
     */
    void UpdateReading()
    {
        // update all reading related fields
        string sentence = sentenceToEdit.TextSentence.text;
        if(sentenceToEdit.GetReadingCount() > 0)
        {
            var kanjiReading = sentenceToEdit.GetReading(currentReading);
            InputReading.SetTextWithoutNotify(kanjiReading.Item2);
            // show a snippet of the sentence with the kanji centralized
            // ex. abcdefg
            // 1: "  abc" 0=>pos-2,5+(pos-2)<5, 2 spaces front
            // 2: " abcd" 1=>pos-2,4, 1 space front
            // 3: "abcde" 2=>pos-2,else
            // 4: "bcdef" 3=>pos-2,else
            // 5: "cdefg" 4=>pos-2,else
            // 6: "defg " 5=>pos-2,4 + 1 sapce
            // 7: "efg  " 6=>pos-2,3 + 2 spaces
            int length = 5;
            int pos = kanjiReading.Item1;
            string frontSpaces = "", backSpaces = "";
            if(pos - 2 < 0) // fill front with spaces
            {
                length = 5 + pos - 2;
                for(int i = 0; i < Mathf.Abs(pos - 2); i++)
                {
                    frontSpaces += " ";
                }
            }
            if(sentence.Length - pos - 2 + 5 < 0) // fill back with spaces
            {
                length = 2 * sentence.Length - pos - 2 + 5;
                for (int i = 0; i < Mathf.Abs(length); i++)
                {
                    backSpaces += " ";
                }
            }
            TextSnippet.SetText(frontSpaces + sentence.Substring(Mathf.Clamp(kanjiReading.Item1 - 2, 0, sentence.Length - 1), length) + backSpaces);
        }
    }

    /**
     * <summary>Determines interactivity of the save sentence button given the readings</summary>
     */
    void UpdateButtonSaveSentence()
    {
        string warning = "";
        ButtonSaveSentence.GetComponent<Button>().interactable = true;
        if (InputSentence.text.Contains(","))
        {
            warning = "Sentence contains ASCII commas that will be converted to Japanese commas.\n分の読点はASCIIです。日本の読点までを変える。";
        }
        if (sentenceToEdit.GetReadingCount() > 0)
        {
            if(sentenceToEdit.HasEmptyReading())
            {
                ButtonSaveSentence.GetComponent<Button>().interactable = false;
                warning = "Missing Furigana!\nふりがながありません。";
            }
            else
            {
                // check each reading
                for (int i = 0; i < sentenceToEdit.GetReadingCount(); i++)
                {
                    int kanaType = JapaneseDictionary.GetKanaType(sentenceToEdit.GetReading(i).Item2);
                    // check if the reading only consists of hiragana
                    if (kanaType != 1)
                    {
                        // if it's only katakana we can convert!
                        if (kanaType == 2)
                        {
                            sentenceToEdit.SetKanjiReading(i, JapaneseDictionary.ConvertKanaToKana(sentenceToEdit.GetReading(i).Item2));
                        }
                        else
                        {
                            ButtonSaveSentence.GetComponent<Button>().interactable = false;
                            warning = "Furigana contains non-hiragana characters!\nふりがながひらがなではありません！";
                        }
                    }
                }
            }
        }
        // set message if there's a warning
        TextFeedbackSentence.GetComponent<TMPro.TMP_Text>().SetText(warning);
    }
}
