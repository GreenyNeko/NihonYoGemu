using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class EditorManager : MonoBehaviour
{
    // new system stuff
    public GameObject sentenceObjectInstance;
    public Transform LevelCanvas;
    public GameObject BackgroundImage;
    public GameObject ResolutionMarker;
    public GameObject InputField;

    public bool editingFurigana;
    [Header("Menues")]
    // references to different menues
    public GameObject WindowLevelCreator;
    public GameObject UnsavedChangesWindow;
    public GameObject ToolBar;
    public GameObject SentenceContextMenu;
    public GameObject PopupSaveOverwriteConfirmation;
    public GameObject SentenceSettingsMenu;
    [Header("Level Info Window")]
    // window level info
    public GameObject InputFileName;
    public GameObject InputLevelName;
    public GameObject InputAuthorName;
    public GameObject InputResolutionX;
    public GameObject InputResolutionY;
    public GameObject InputInputOffsetX;
    public GameObject InputInputOffsetY;
    public GameObject DropdownScaleMode;
    public GameObject ButtonSaveInfo;
    public GameObject TextFeedbackInfo;
    [Header("Button Menu")]
    // reference to general buttons
    public Button ButtonSave;
    public Button ButtonLeave;
    public Button ButtonEditInfo;
    public Button ButtonToolBarToggle;

    bool unsavedChanges;
    bool showingToolBar;
    int selectedSentenceObject;
    Sprite currBackgroundSprite;
    Texture2D currBackgroundTexture;
    bool furiganaMode = false;

    int currPage;
    Level currLevel;

    string levelName;
    List<GameObject> sentenceObjects;

    List<EditorAction> editorHistory;
    int historyMarker;
    int saveMarker;

    //Required to keep track of the outline slider change for undo and redo
    float lastOutlineSize;

    void Awake()
    {
        lastOutlineSize = 0;
        historyMarker = -1;
        selectedSentenceObject = -1;
        SentenceContextMenu.SetActive(false);
        editorHistory = new List<EditorAction>();
        sentenceObjects = new List<GameObject>();
        showingToolBar = false;
        // grab data, if the name is empty it's a new level
        MemoryStream memoryStream = new MemoryStream(SceneManagerPlus.GetCurrentData());
        using (BinaryReader reader = new BinaryReader(memoryStream))
        {
            reader.ReadInt32(); // previous scene, unused here
            reader.ReadBoolean(); // editor flag, unused here
            levelName = reader.ReadString();
            reader.ReadInt32(); // mods, unused here
        }
        // if name is empty we create a new level
        if (levelName == "")
        {
            WindowLevelCreator.SetActive(true);
        }
        else
        {
            WindowLevelCreator.SetActive(false);
            // load the level and populate scene
        }
        //LevelEditorMenu.SetActive(true);
        //SentenceEditorMenu.SetActive(false);
        UnsavedChangesWindow.SetActive(false);
        ButtonSave.interactable = false;
        SetBackgroundImage(null);
    }

    void Start()
    {
        // load level data
        if (levelName != "")
        {
            currLevel = LevelLoader.LoadLevelByName(levelName);
            currPage = 0;
            // file name
            InputFileName.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(currLevel.FileName);
            InputLevelName.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(currLevel.LevelName);
            InputAuthorName.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(currLevel.Author);
            Debug.Log(currLevel.nativeX);
            InputResolutionX.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(currLevel.nativeX.ToString());
            InputResolutionY.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(currLevel.nativeY.ToString());
            InputInputOffsetX.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(currLevel.inputOffsetX.ToString());
            InputInputOffsetY.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(currLevel.inputOffsetY.ToString());
            DropdownScaleMode.GetComponent<TMPro.TMP_Dropdown>().SetValueWithoutNotify(currLevel.scaleMode);
            for (int i = 0; i < currLevel.pages[currPage].sentenceObjects.Count; i++)
            {
                Debug.Log("create game object #" + i.ToString());
                RecreateSentenceObject(i);
            }
            DestroyImmediate(currBackgroundSprite);
            currBackgroundTexture.LoadImage(currLevel.pages[0].backgrounImageData);
            currBackgroundSprite = Sprite.Create(currBackgroundTexture, new Rect(0, 0, currBackgroundTexture.width, currBackgroundTexture.height), new Vector2(0.5f, 0.5f));
            SetBackgroundImage(currBackgroundSprite);
            // level offset
            Vector2 offset = (new Vector2(Screen.width, -Screen.height) - new Vector2(currLevel.nativeX, -currLevel.nativeY)) / 2; // - new Vector2(50, -50)
            // update input position
            InputField.transform.localPosition = new Vector3(currLevel.inputOffsetX + offset.x, -currLevel.inputOffsetY + offset.y, 0);
        }
        else
        {
            InputResolutionX.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(Screen.width.ToString());
            InputResolutionY.GetComponent<TMPro.TMP_InputField>().SetTextWithoutNotify(Screen.height.ToString());
        }
        // update resolution marker
        ResolutionMarker.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, int.Parse(InputResolutionX.GetComponent<TMPro.TMP_InputField>().text));
        ResolutionMarker.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, int.Parse(InputResolutionY.GetComponent<TMPro.TMP_InputField>().text));
        currPage = 0;
    }

    void OnEnable()
    {
        currBackgroundTexture = new Texture2D(1, 1);
        if (currLevel != null)
        {
            currBackgroundTexture.LoadImage(currLevel.pages[currPage].backgrounImageData);
            currBackgroundSprite = Sprite.Create(currBackgroundTexture, new Rect(0, 0, currBackgroundTexture.width, currBackgroundTexture.height), new Vector2(0.5f, 0.5f));
            SetBackgroundImage(currBackgroundSprite);
        }
    }

    void OnDisable()
    {
        if (currLevel != null)
        {
            DestroyImmediate(currBackgroundTexture);
            DestroyImmediate(currBackgroundSprite);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (selectedSentenceObject > -1)
            {
                if (!sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().IsEditing())
                {
                    DeleteSelectedSentenceObject();
                }
            }
        }
        if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (selectedSentenceObject <= -1 || !sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().IsEditing())
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    Debug.Log("redo");
                    RedoEditorAction();
                }
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    Debug.Log("undo");
                    UndoEditorAction();
                }
                if(Input.GetKeyDown(KeyCode.C))
                {
                    // copy
                    if(selectedSentenceObject > -1)
                    {
                        // copy sentencedata to clipboard
                        GUIUtility.systemCopyBuffer = "NHYLSO" + JsonUtility.ToJson(currLevel.pages[currPage].sentenceObjects[selectedSentenceObject]);
                    }
                }
                if (Input.GetKeyDown(KeyCode.X))
                {
                    if (selectedSentenceObject > -1)
                    {
                        // cut
                        Vector2 offset = (new Vector2(Screen.width, Screen.height) - new Vector2(currLevel.nativeX, currLevel.nativeY)) / 2;
                        GUIUtility.systemCopyBuffer = "NHYLSO" + JsonUtility.ToJson(currLevel.pages[currPage].sentenceObjects[selectedSentenceObject]);
                        AddEditorAction(new EditorActionDeleteSentence(selectedSentenceObject, currPage, currLevel.pages[currPage].sentenceObjects[selectedSentenceObject], sentenceObjectInstance, LevelCanvas, offset));
                    }
                }
                if(Input.GetKeyDown(KeyCode.V))
                {
                    // paste
                    if(GUIUtility.systemCopyBuffer.Substring(0, 6) == "NHYLSO")
                    {
                        Vector2 offset = (new Vector2(Screen.width, Screen.height) - new Vector2(currLevel.nativeX, currLevel.nativeY)) / 2;
                        var clipboardObject = JsonUtility.FromJson<Level.SentenceData>(GUIUtility.systemCopyBuffer.Substring(6));
                        AddEditorAction(new EditorActionPasteSentenceObject(sentenceObjects.Count, currPage, clipboardObject, sentenceObjectInstance, LevelCanvas, offset));
                    }
                }
            }
        }
    }

    /**
     * <summary>Updates the level info.</summary>
     */
    public void UpdateLevelInfo()
    {
        // history update: UpdateLevelinfo(
        /*
         * if(currLevel != null)
         * {
         *  UpdateLevelinfo(currLevel.FileName, currLevel.Levelname, currLevel.Author, currLevel.nativeX, currLevel.nativeY, currLevel.scaleMode,
         * }
         */
        if (currLevel == null)
        {
            currLevel = new Level(InputFileName.GetComponent<TMPro.TMP_InputField>().text, InputLevelName.GetComponent<TMPro.TMP_InputField>().text, InputAuthorName.GetComponent<TMPro.TMP_InputField>().text);
            currLevel.pages = new List<Level.PageData>();
            Level.PageData currPage = new Level.PageData();
            currPage.sentenceObjects = new List<Level.SentenceData>();
            currLevel.pages.Add(currPage);
        }
        else
        {
            currLevel.FileName = InputFileName.GetComponent<TMPro.TMP_InputField>().text;
            currLevel.LevelName = InputLevelName.GetComponent<TMPro.TMP_InputField>().text;
            currLevel.Author = InputAuthorName.GetComponent<TMPro.TMP_InputField>().text;
        }
        currLevel.nativeX = int.Parse(InputResolutionX.GetComponent<TMPro.TMP_InputField>().text);
        currLevel.nativeY = int.Parse(InputResolutionY.GetComponent<TMPro.TMP_InputField>().text);
        currLevel.inputOffsetX = int.Parse(InputInputOffsetX.GetComponent<TMPro.TMP_InputField>().text);
        currLevel.inputOffsetY = int.Parse(InputInputOffsetY.GetComponent<TMPro.TMP_InputField>().text);
        currLevel.scaleMode = (byte)DropdownScaleMode.GetComponent<TMPro.TMP_Dropdown>().value;
        // level offset
        Vector2 offset = (new Vector2(Screen.width, -Screen.height) - new Vector2(currLevel.nativeX, -currLevel.nativeY)) / 2; // - new Vector2(50, -50)
        // update input position
        InputField.transform.localPosition = new Vector3(currLevel.inputOffsetX + offset.x, -currLevel.inputOffsetY + offset.y, 0);
        // update resolution marker
        ResolutionMarker.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, int.Parse(InputResolutionX.GetComponent<TMPro.TMP_InputField>().text));
        ResolutionMarker.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, int.Parse(InputResolutionY.GetComponent<TMPro.TMP_InputField>().text));
        NotifyOfChanges();
    }

    /**
     * <summary>Returns whether or not editor is in furigana or sentence object mode.</summary>
     */
    public bool IsFuriganaMode()
    {
        return furiganaMode;
    }

    /**
     * <summary>Returns the native size of the level</summary>
     */
    public Vector2 GetLevelNativeSize()
    {
        return new Vector2(currLevel.nativeX, currLevel.nativeY);
    }

    /**
     * <summary>Attempt to leave the editor but ask user if there are unsaved changes</summary>
     */
    public void TryLeavingEditor()
    {
        if (!unsavedChanges)
        {
            LeaveEditor();
        }
        else
        {
            UnsavedChangesWindow.SetActive(true);
        }
    }

    /**
     * <summary>Leave the editor</summary>
     */
    public void LeaveEditor()
    {
        string fileName = "";
        if (currLevel != null)
        {
            fileName = currLevel.FileName;
        }
        MemoryStream memoryStream = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(memoryStream))
        {
            writer.Write(SceneUtility.GetBuildIndexByScenePath("Scenes/EditorScene"));
            writer.Write(true); // we come from editor so we're in editor mode
            writer.Write(fileName);
            writer.Write(0);
        }
        SceneManagerPlus.LoadScene("LevelSelect", memoryStream.ToArray());
    }

    /**
     * <summary>Saves the level</summary>
     */
    public void Save(bool overwrite)
    {
        if (!overwrite)
        {
            if (File.Exists("Levels/" + currLevel.FileName + ".nyl"))
            {
                PopupSaveOverwriteConfirmation.SetActive(true);
                return;
            }
        }
        unsavedChanges = false;
        saveMarker = historyMarker;
        ButtonSave.interactable = false;
        // save in file
        /*
         *  string version
         *  string levelname
         *  string author
         *  int nativeX
         *  int nativeY
         *  int scaleMode
         *  int pages
         *
         *  Page:
         *  int backgroundImageDataLength;
         *  Byte[] backgroundImageData
         *  int scaleMode
         *  int sentenceObjects;
         *
         *  SentenceData:
         *  float x
         *  float y
         *  float width
         *  float height
         *  string text;
         *  float textSize;
         *  float outlineSize;
         *  bool color;
         *  bool vertical;
         *  string[] furigana;
         */
        string strData = "";
        using (BinaryWriter streamWriter = new BinaryWriter(new FileStream("Levels/" + currLevel.FileName + ".nyl", FileMode.Create)))
        {
            // write user meta data
            streamWriter.Write("NYLv2");
            strData += "NYLv2";
            streamWriter.Write(currLevel.LevelName);
            strData += currLevel.LevelName;
            streamWriter.Write(currLevel.Author);
            strData += currLevel.Author;
            streamWriter.Write(currLevel.nativeX);
            strData += currLevel.nativeX.ToString();
            streamWriter.Write(currLevel.nativeY);
            strData += currLevel.nativeY.ToString();
            streamWriter.Write(currLevel.scaleMode);
            strData += currLevel.scaleMode.ToString();
            streamWriter.Write(currLevel.inputOffsetX);
            strData += currLevel.inputOffsetX.ToString();
            streamWriter.Write(currLevel.inputOffsetY);
            strData += currLevel.inputOffsetY.ToString();
            streamWriter.Write(currLevel.pages.Count);
            strData += currLevel.pages.Count.ToString();
            // write page
            for (int i = 0; i < currLevel.pages.Count; i++)
            {
                Level.PageData pageData = currLevel.pages[i];
                if (pageData.backgrounImageData == null)
                {
                    Debug.Log("null?");
                    streamWriter.Write(0);
                    strData += "0";
                    streamWriter.Write('\0');
                    strData += "0";
                }
                else
                {
                    streamWriter.Write(pageData.backgrounImageData.Length);
                    strData += pageData.backgrounImageData.Length.ToString();
                    streamWriter.Write(pageData.backgrounImageData);
                    strData += pageData.backgrounImageData.ToString();
                }
                streamWriter.Write(pageData.scaleMode);
                strData += pageData.scaleMode.ToString();
                streamWriter.Write(pageData.sentenceObjects.Count);
                strData += pageData.sentenceObjects.Count.ToString();
                // write sentence objects
                for (int j = 0; j < pageData.sentenceObjects.Count; j++)
                {
                    Level.SentenceData sentenceData = pageData.sentenceObjects[j];
                    streamWriter.Write(sentenceData.rect.x);
                    strData += sentenceData.rect.x.ToString();
                    streamWriter.Write(sentenceData.rect.y);
                    strData += sentenceData.rect.y.ToString();
                    streamWriter.Write(sentenceData.rect.width);
                    strData += sentenceData.rect.width.ToString();
                    streamWriter.Write(sentenceData.rect.height);
                    strData += sentenceData.rect.height.ToString();
                    streamWriter.Write(sentenceData.text);
                    strData += sentenceData.text;
                    streamWriter.Write(sentenceData.textSize);
                    strData += sentenceData.textSize.ToString();
                    streamWriter.Write(sentenceData.outlineSize);
                    strData += sentenceData.outlineSize.ToString();
                    // cover alignement(4), boldness(1), vertical(1) and color(1) in one byte
                    byte abvc = (byte)(sentenceData.alignment << 1);
                    abvc |= sentenceData.bold ? (byte)1 : (byte)0;
                    abvc <<= 1;
                    abvc |= sentenceData.vertical ? (byte)1 : (byte)0;
                    abvc <<= 1;
                    abvc |= sentenceData.color ? (byte)1 : (byte)0;
                    abvc <<= 1;
                    streamWriter.Write(abvc);
                    strData += abvc.ToString();
                    streamWriter.Write(sentenceData.furigana.Length);
                    strData += sentenceData.furigana.Length.ToString();
                    for (int k = 0; k < sentenceData.furigana.Length; k++)
                    {
                        streamWriter.Write(sentenceData.furigana[k]);
                        strData += sentenceData.furigana[k];
                    }
                }
            }
        }
        Debug.Log(strData);
    }

    /**
     * <summary>Toggles the tool bar up or down</summary>
     */
    public void ToggleToolBar()
    {
        if (showingToolBar)
        {
            ToolBar.transform.position += new Vector3(0, -48);
            ButtonToolBarToggle.transform.position += new Vector3(0, -48);
            ButtonToolBarToggle.GetComponentInChildren<TMPro.TMP_Text>().SetText("^");
        }
        else
        {
            ToolBar.transform.position += new Vector3(0, 48);
            ButtonToolBarToggle.transform.position += new Vector3(0, 48);
            ButtonToolBarToggle.GetComponentInChildren<TMPro.TMP_Text>().SetText("v");
        }
        showingToolBar = !showingToolBar;
    }

    /**
     * <summary>Toggles between sentence object and furigana mode.</summary>
     */
    public void ToggleFuriganaMode()
    {
        DeselectSentenceObject();
        furiganaMode = !furiganaMode;
    }

    /**
     * <summary>Shows the sentence settings window></summary>
     */
    public void ToggleSentenceSettings()
    {
        if (selectedSentenceObject == -1)
        {
            return;
        }
        if (SentenceSettingsMenu.activeInHierarchy)
        {
            SentenceSettingsMenu.SetActive(false);
            return;
        }
        RectTransform rectTransform = SentenceSettingsMenu.GetComponent<RectTransform>();
        if (sentenceObjects[selectedSentenceObject].GetComponent<RectTransform>().anchoredPosition.x < Screen.width / 2 - 50)
        {
            // show on right side
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 0.5f);
        }
        else
        {
            // show on left side
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
        }
        SentenceSettingsMenu.SetActive(true);
    }

    /**
     * <summary>Creates a new sentence object given the rect</summary>
     */
    public void CreateSentenceObject(Rect rect)
    {
        Vector2 offset = (new Vector2(Screen.width, -Screen.height) - new Vector2(currLevel.nativeX, -currLevel.nativeY)) / 2 - new Vector2(50, -50);
        AddEditorAction(new EditorActionCreateSentence(sentenceObjects.Count, currPage, rect, sentenceObjectInstance, LevelCanvas, offset));
    }

    /**
     * <summary>Recreate all sentence objects given the backend data.</summary>
     */
    public void RecreateSentenceObject(int index)
    {
        // convert from level coords to gameobj
        Vector2 offset = (new Vector2(Screen.width, -Screen.height) - new Vector2(currLevel.nativeX, -currLevel.nativeY)) / 2 - new Vector2(50, -50);
        // create object for each in data
        GameObject sentenceObject = Instantiate(sentenceObjectInstance, LevelCanvas);
        sentenceObject.GetComponent<LevelSentenceObject>().template = false;
        
        Debug.Log(currLevel.pages[currPage].sentenceObjects[index].rect.position);
        sentenceObject.GetComponent<RectTransform>().anchoredPosition = currLevel.pages[currPage].sentenceObjects[index].rect.position + offset;
        sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currLevel.pages[currPage].sentenceObjects[index].rect.width);
        sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currLevel.pages[currPage].sentenceObjects[index].rect.height);
        sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.text = currLevel.pages[currPage].sentenceObjects[index].text;
        // update to create furigana
        sentenceObject.transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().UpdatedSentence(currLevel.pages[currPage].sentenceObjects[index].text);
        // set furigana
        for (int i = 0; i < currLevel.pages[currPage].sentenceObjects[index].furigana.Length; i++)
        {
            TMPro.TMP_Text furigana = sentenceObject.transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().transform.GetChild(i).GetComponent<TMPro.TMP_Text>();
            furigana.text = currLevel.pages[currPage].sentenceObjects[index].furigana[i];
            if (currLevel.pages[currPage].sentenceObjects[index].bold)
            {
                furigana.fontStyle = TMPro.FontStyles.Bold;
            }
            else
            {
                furigana.fontStyle = TMPro.FontStyles.Normal;
            }
            if (currLevel.pages[currPage].sentenceObjects[index].color)
            {
                furigana.color = Color.black;
            }
            else
            {
                furigana.color = Color.white;
            }
        }
        sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.fontSize = currLevel.pages[currPage].sentenceObjects[index].textSize;
        if (currLevel.pages[currPage].sentenceObjects[index].bold)
        {
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.fontStyle = TMPro.FontStyles.Bold;
        }
        else
        {
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.fontStyle = TMPro.FontStyles.Normal;
        }
        if (currLevel.pages[currPage].sentenceObjects[index].color)
        {
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.color = Color.black;
            sentenceObject.GetComponent<LevelSentenceObject>().inputField.textComponent.color = Color.black;
        }
        else
        {
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.color = Color.white;
            sentenceObject.GetComponent<LevelSentenceObject>().inputField.textComponent.color = Color.white;
        }
        sentenceObject.GetComponent<LevelSentenceObject>().sentenceDataIndex = index;
        sentenceObjects.Add(sentenceObject);
        sentenceObject.GetComponentInChildren<OrientedText>().SetOrientation(currLevel.pages[currPage].sentenceObjects[index].vertical);
        Debug.Log("Sentence Objects increased to " + sentenceObjects.Count.ToString());
    }

    /**
     * <summary>Delete the selected sentence object.</summary>
     */
    public void DeleteSelectedSentenceObject()
    {
        if (selectedSentenceObject > -1)
        {
            Vector2 offset = (new Vector2(Screen.width, -Screen.height) - new Vector2(currLevel.nativeX, -currLevel.nativeY)) / 2 - new Vector2(50, -50);
            int tempSelectedSentenceObj = selectedSentenceObject;
            DeselectSentenceObject();
            AddEditorAction(new EditorActionDeleteSentence(tempSelectedSentenceObj, currPage, currLevel.pages[currPage].sentenceObjects[tempSelectedSentenceObj], sentenceObjectInstance, LevelCanvas, offset));
        }
    }

    /**
     * <summary>Used for input events, updates the x position of the sentence object.</summary>
     */
    public void UpdateSentenceObjectPositionX(string x)
    {
        if(selectedSentenceObject > -1)
        {
            Rect rect = currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].rect;
            float tempX = float.Parse(x);
            // no need to convert
            rect.x = tempX;
            UpdateSentenceObjectRect(rect, true);
        }
    }

    /**
     * <summary>Used for input events, updates the y position of the sentence object.</summary>
     */
    public void UpdateSentenceObjectPositionY(string y)
    {
        if (selectedSentenceObject > -1)
        {
            Rect rect = currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].rect;
            float tempY = float.Parse(y);
            // no need to convert
            rect.y = tempY;
            UpdateSentenceObjectRect(rect, true);
        }
    }

    /**
     * <summary>Used for input events, updates the y position of the sentence object.</summary>
     */
    public void UpdateSentenceObjectSizeWidth(string width)
    {
        if (selectedSentenceObject > -1)
        {
            Rect rect = currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].rect;
            rect.width = float.Parse(width);
            UpdateSentenceObjectRect(rect, true);
        }
    }

    /**
     * <summary>Used for input events, updates the y position of the sentence object.</summary>
     */
    public void UpdateSentenceObjectSizeHeight(string height)
    {
        if (selectedSentenceObject > -1)
        {
            Rect rect = currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].rect;
            rect.height = float.Parse(height);
            UpdateSentenceObjectRect(rect, true);
        }
    }

    /**
     * <summary>Updates the rect of the sentence object.
     * Use undoable flag if this is part of another action (e.g. creation)</summary>
     */
    public void UpdateSentenceObjectRect(Rect rect, bool undoable)
    {
        if (selectedSentenceObject > -1)
        {
            Vector2 offset = (new Vector2(Screen.width, -Screen.height) - new Vector2(currLevel.nativeX, -currLevel.nativeY)) / 2 - new Vector2(50, -50);
            Rect prev = currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].rect;
            if(undoable)
            {
                AddEditorAction(new EditorActionTextRect(selectedSentenceObject, currPage, rect, prev, offset));
            }
            else
            {
                unsavedChanges = true;
                currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].rect = rect;
            }
        }
    }

    /**
     * <summary>Updates the sentence object text in the backend and update the furigana.</summary>
     */
    public void UpdateSentenceText(string text)
    {
        if (selectedSentenceObject > -1)
        {
            string undo, redo;
            undo = currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].text;
            redo = text;
            int kanjiCount = 0;
            for (int i = 0; i < text.Length; i++)
            {
                kanjiCount += JapaneseDictionary.IsKanji(text[i].ToString()) ? 1 : 0;
            }
            string[] newFurigana = new string[kanjiCount];
            string[] oldFurigana = currLevel.pages[currPage].sentenceObjects[sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().sentenceDataIndex].furigana;
            for (int i = 0; i < Mathf.Min(oldFurigana.Length, newFurigana.Length); i++)
            {
                newFurigana[i] = oldFurigana[i];
            }
            AddEditorAction(new EditorActionEditSentence(selectedSentenceObject, currPage, redo, undo, newFurigana, oldFurigana));
        }
    }

    /**
     * <summary>Swaps the position of the current object's order.</summary>
     */
    public void UpdateOrder()
    {
        if (selectedSentenceObject > -1)
        {
            AddEditorAction(new EditorActionChangeOrder(selectedSentenceObject, currPage, SentenceContextMenu));
        }
    }

    /**
     * <summary>Changes the outline size. If the provided size is smaller than 0 it will use the data from the backend data</summary>
     */
    public void ChangeSentenceOutlineSize(float size)
    {
        Debug.Log("changing outline size");
        if (selectedSentenceObject > -1)
        {
            float currSize = currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].outlineSize;
            if(size > -1)
            {
                currSize = size;
            }
            AddEditorAction(new EditorActionEditTextOutline(selectedSentenceObject, currPage, currSize, lastOutlineSize));
            lastOutlineSize = currSize;
        }
    }

    /**
     * <summary>Changes the outline size. These changes are not considered for undo and redo.</summary>
     */
    public void ChangeSentenceOutlineSizeTemp(float size)
    {
        if (selectedSentenceObject > -1)
        {
            currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].outlineSize = size; // necesary?
            sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().DisplayedText.outlineWidth = size;
            sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().inputField.textComponent.outlineWidth = size;
            // update furigana
            Transform furiganaParent = sentenceObjects[selectedSentenceObject].transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.transform;
            for (int i = 0; i < furiganaParent.childCount; i++)
            {
                furiganaParent.GetChild(i).GetComponent<TMPro.TMP_Text>().outlineWidth = size;
            }
            unsavedChanges = true;
        }
    }

    /**
     * <summary>Changes the text size of a sentence object in the backend.</summary>
     */
    public void ChangeSentenceTextSize(string size)
    {
        TMPro.TMP_Text displayedText = sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().DisplayedText;
        if (selectedSentenceObject > -1)
        {
            float prevSize = displayedText.fontSize;
            AddEditorAction(new EditorActionEditTextSize(selectedSentenceObject, currPage, float.Parse(size), prevSize));
        }
    }

    /**
     * <summary>Toggles the text color of a sentene object in the backend.</summary>
     */
    public void ToggleSentenceTextColor()
    {
        if (selectedSentenceObject > -1)
        {
            bool prev = currLevel.pages[currPage].sentenceObjects[sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().sentenceDataIndex].color;
            AddEditorAction(new EditorActionTextColor(selectedSentenceObject, currPage, !prev, prev));
        }
    }

    /**
     * <summary>Toggles the boldness of the sentence object's text.</summary>
     */
    public void ToggleSentenceTextBold()
    {
        if(selectedSentenceObject > -1)
        {
            bool prev = currLevel.pages[currPage].sentenceObjects[sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().sentenceDataIndex].bold;
            AddEditorAction(new EditorActionTextBold(selectedSentenceObject, currPage, !prev, prev));
        }
    }

    /**
     * <summary>Sets the orientation of a sentence object in the backend.</summary>
     */
    public void SetSentenceOrientation(int option)
    {
        if (selectedSentenceObject > -1)
        {
            bool newState = false;
            if (option != 0)
            {
                newState = true;
            }
            AddEditorAction(new EditorActionTextOrientation(selectedSentenceObject, currPage, newState, currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].vertical));
        }
    }

    /**
     * <summary>Sets the vertical alignment of a sentence object in the backend.</summary>
     */
    public void SetSentenceVerticalAlignment(int option)
    {
        if(selectedSentenceObject > -1)
        {
            // this works because alignment >9 is currently not used and for smaller it iterates through horizontal incrementing vertical second
            byte currVAlign = (byte)(currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].alignment / 3);
            AddEditorAction(new EditorActionTextAlignmentVertical(selectedSentenceObject, currPage, option, currVAlign));
            StartCoroutine(MainThreadUpdateAlignment((selectedSentenceObject, option, currVAlign)));
        }
    }

    /**
     * <summary>Sets the vertical alignment of a sentence object in the backend.</summary>
     */
    public void SetSentenceHorizontalAlignment(int option)
    {
        if (selectedSentenceObject > -1)
        {
            // this works because alignment >9 is currently not used and for smaller it iterates through horizontal incrementing vertical second
            byte currHAlign = (byte)(currLevel.pages[currPage].sentenceObjects[selectedSentenceObject].alignment % 3);
            AddEditorAction(new EditorActionTextAlignmentHorizontal(selectedSentenceObject, currPage, option, currHAlign));
            StartCoroutine(MainThreadUpdateAlignment((selectedSentenceObject, currHAlign, option)));
        }
    }

    IEnumerator MainThreadUpdateAlignment((int,int,int) packedParams)
    {
        TMPro.TextAlignmentOptions[,] alignmentMapping = { { TMPro.TextAlignmentOptions.TopLeft, TMPro.TextAlignmentOptions.Top, TMPro.TextAlignmentOptions.TopRight },
            { TMPro.TextAlignmentOptions.Left, TMPro.TextAlignmentOptions.Center, TMPro.TextAlignmentOptions.Right },
            { TMPro.TextAlignmentOptions.BottomLeft, TMPro.TextAlignmentOptions.Bottom, TMPro.TextAlignmentOptions.BottomRight },
        };
        sentenceObjects[packedParams.Item1].GetComponent<TMPro.TMP_Text>().alignment = alignmentMapping[packedParams.Item2, packedParams.Item3];
        yield return null;
    }

    /**
     * <summary>Stores the provided image data in the backend and loads it as background.</summary>
     */
    public void StoreBackgroundImage(byte[] bytes)
    {
        currLevel.pages[currPage].backgrounImageData = bytes;
        // update background image

        // clear to save memory
        DestroyImmediate(currBackgroundSprite);
        currBackgroundTexture.LoadImage(bytes);
        currBackgroundSprite = Sprite.Create(currBackgroundTexture, new Rect(0, 0, currBackgroundTexture.width, currBackgroundTexture.height), new Vector2(0.5f, 0.5f));
        SetBackgroundImage(currBackgroundSprite);
    }

    /**
     * <summary>Sets the background image scaling mode</summary>
     */
    public void SetScalingModeBackground(int mode)
    {
        if(mode >= (1 << 8))
        {
            Debug.LogError("Mode overflow");
        }
        currLevel.pages[currPage].scaleMode = (byte)mode;
    }

    /**
     * <summary>Sets the background image to the provided sprite, else to black</summary>
     */
    public void SetBackgroundImage(Sprite sprite)
    {
        if (sprite != null)
        {
            unsavedChanges = true;
            BackgroundImage.GetComponent<Image>().color = Color.white;
            BackgroundImage.GetComponent<Image>().sprite = sprite;
            float width = 0, height = 0;
            Vector2 diff;
            float scaleFactor = 1;
            switch (currLevel.pages[currPage].scaleMode)
            {
                case 4:
                    // scale
                    width = Screen.width;
                    height = Screen.height;
                    break;
                case 3:
                    // scale up
                    // determine if scaling up or keep
                    width = currLevel.nativeX > Screen.width ? currLevel.nativeX : Screen.width;
                    height = currLevel.nativeY > Screen.height ? currLevel.nativeY : Screen.height;
                    break;
                case 2:
                    // keep aspect ratio
                    // determine smaller difference between width and height
                    diff = new Vector2(Screen.width, Screen.height) - sprite.rect.size;
                    if(diff.x < diff.y)
                    {
                        // scale up to screen width
                        scaleFactor = (float)Screen.width / sprite.rect.width;
                    }
                    else
                    {
                        // scale up to screen height
                        scaleFactor = (float)Screen.height / sprite.rect.height;
                    }
                    width = sprite.rect.width * scaleFactor;
                    height = sprite.rect.height * scaleFactor;
                    break;
                case 1:
                    // keep aspect ratio, only scale up
                    // determine if scaling up or keep
                    width = currLevel.nativeX > Screen.width ? currLevel.nativeX : Screen.width;
                    height = currLevel.nativeY > Screen.height ? currLevel.nativeY : Screen.height;
                    // determine scaling factor for the fitting axis
                    diff = new Vector2(width, height) - sprite.rect.size;
                    if (diff.x < diff.y)
                    {
                        // scale up to screen or native width
                        scaleFactor = (float)width / sprite.rect.width;
                    }
                    else
                    {
                        // scale up to screen or native height
                        scaleFactor = (float)height / sprite.rect.height;
                    }
                    // width and height set to sprite's
                    width = sprite.rect.width * scaleFactor;
                    height = sprite.rect.height * scaleFactor;
                    break;
                default:
                    // keep scale of image
                    // width and height set to sprite's
                    width = currLevel.nativeX;
                    height = currLevel.nativeY;
                    break;
            }
            BackgroundImage.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            BackgroundImage.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            // aligned to top left
            BackgroundImage.GetComponent<RectTransform>().localPosition = Vector2.zero;
            // no change to width/height
            BackgroundImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            BackgroundImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
        else
        {
            BackgroundImage.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            BackgroundImage.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            BackgroundImage.GetComponent<Image>().color = Color.black;
            BackgroundImage.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            BackgroundImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
            BackgroundImage.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
            //BackgroundImage.GetComponent<RectTransform>().SetSize
        }
    }

    /**
     * <summary>Sets the furigana at the given index</summary>
     */
    public void SetSentenceFurigana(int furiganaIndex, string furigana)
    {
        Level.SentenceData sentenceData = currLevel.pages[currPage].sentenceObjects[selectedSentenceObject];
        AddEditorAction(new EditorActionSetFurigana(selectedSentenceObject, currPage, furiganaIndex, furigana, sentenceData.furigana[furiganaIndex]));
    }

    /**
     * <summary>Selects the given gameobject (sentence object) and adds the context to it</summary>
     */
    public void SetSelectedSentenceObject(GameObject gameObject)
    {
        // deselect the previous
        if (selectedSentenceObject > -1)
        {
            sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().Deselect();
        }
        selectedSentenceObject = sentenceObjects.IndexOf(gameObject);
        SentenceContextMenu.GetComponent<RectTransform>().position = GetSentenceContextPosition();
        // prevent context menu being off-screen
        if (-SentenceContextMenu.GetComponent<RectTransform>().anchoredPosition.y < 0)
        {
            SentenceContextMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(SentenceContextMenu.GetComponent<RectTransform>().anchoredPosition.x, 0);
        }
        if (SentenceContextMenu.GetComponent<RectTransform>().anchoredPosition.x > Screen.width - 120)
        {
            SentenceContextMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width - 120, SentenceContextMenu.GetComponent<RectTransform>().anchoredPosition.y);
        }
        if (!IsFuriganaMode())
        {
            SentenceContextMenu.gameObject.SetActive(true);
        }
        // update sentence settings
        SentenceSettingsMenu.GetComponent<MenuSentenceSettings>().buttonColor.GetComponent<Image>().color = gameObject.GetComponent<LevelSentenceObject>().DisplayedText.color;
        SentenceSettingsMenu.GetComponent<MenuSentenceSettings>().verticalOption.GetComponent<TMPro.TMP_Dropdown>().SetValueWithoutNotify(gameObject.transform.GetChild(0).GetComponent<OrientedText>().vertical ? 1 : 0);
        SentenceSettingsMenu.GetComponent<MenuSentenceSettings>().inputTextSize.GetComponent<TMPro.TMP_InputField>().text = gameObject.GetComponent<LevelSentenceObject>().DisplayedText.fontSize.ToString();
        SentenceSettingsMenu.GetComponent<MenuSentenceSettings>().sliderOutlineSize.GetComponent<Slider>().value = gameObject.GetComponent<LevelSentenceObject>().DisplayedText.outlineWidth;
        Vector2 offset = (new Vector2(Screen.width, -Screen.height) - new Vector2(currLevel.nativeX, -currLevel.nativeY)) / 2 - new Vector2(50, -50);
        // convert from gameobject position to level coords
        Vector2 convertedPos = (Vector2)gameObject.GetComponent<RectTransform>().anchoredPosition - offset;
        SentenceSettingsMenu.GetComponent<MenuSentenceSettings>().inputPositionX.GetComponent<TMPro.TMP_InputField>().text = convertedPos.x.ToString();
        SentenceSettingsMenu.GetComponent<MenuSentenceSettings>().inputPositionY.GetComponent<TMPro.TMP_InputField>().text = convertedPos.y.ToString();
        SentenceSettingsMenu.GetComponent<MenuSentenceSettings>().inputSizeWidth.GetComponent<TMPro.TMP_InputField>().text = gameObject.GetComponent<RectTransform>().rect.width.ToString();
        SentenceSettingsMenu.GetComponent<MenuSentenceSettings>().inputSizeHeight.GetComponent<TMPro.TMP_InputField>().text = gameObject.GetComponent<RectTransform>().rect.height.ToString();
        // update sentence context menu order button
        SentenceContextMenu.transform.GetChild(0).GetComponentInChildren<TMPro.TMP_Text>().SetText((selectedSentenceObject + 1).ToString());
    }

    /**
     * <summary>Switch to the next page, if it doesn't exist create it.</summary>
     */
    public void NextPage()
    {
        if (currPage + 1 >= currLevel.pages.Count)
        {
            currLevel.pages.Add(new Level.PageData());
        }
        GotoPage(currPage + 1);
    }

    /**
     * <summary>Switch to a previous page if it exists</summary>
     */
    public void PrevPage()
    {
        GotoPage(currPage - 1);
    }

    public void GotoPage(int page)
    {
        if(page < 0 || page >= currLevel.pages.Count)
        {
            return;
        }
        DeselectSentenceObject();
        currPage = page;
        //clear current objets
        ClearPage();
        // recreate this page's objects
        for (int i = 0; i < currLevel.pages[currPage].sentenceObjects.Count; i++)
        {
            RecreateSentenceObject(i);
        }
        // update background
        DestroyImmediate(currBackgroundSprite);
        currBackgroundTexture.LoadImage(currLevel.pages[currPage].backgrounImageData);
        currBackgroundSprite = Sprite.Create(currBackgroundTexture, new Rect(0, 0, currBackgroundTexture.width, currBackgroundTexture.height), new Vector2(0.5f, 0.5f));

        SetBackgroundImage(currBackgroundSprite);
    }

    /**
     * <summary>Clear the page of all sentence objects</summary>
     */
    public void ClearPage()
    {
        for (int i = 0; i < sentenceObjects.Count; i++)
        {
            Destroy(sentenceObjects[i]);
        }
        sentenceObjects.Clear();
    }

    /**
     * <summary>Delete current page</summary>
     */
    public void DeleteCurrentPage()
    {
        if(currLevel.pages.Count <= 1)
        {
            return;
        }
        AddEditorAction(new EditorActionDeletePage(selectedSentenceObject, currPage, currLevel.pages[currPage]));
        if(currPage > 0)
        {
            GotoPage(currPage - 1);
        }
        else if(currPage == 0)
        {
            GotoPage(currPage);
        }
    }

    /**
     * <summary>Sets the level name</summary>
     */
    public void SetName(TMPro.TMP_Text inputFieldName)
    {
        currLevel.LevelName = inputFieldName.text;
        NotifyOfChanges();
    }

    /**
     * <summary>Sets the level name</summary>
     */
    public void SetAuthor(TMPro.TMP_Text inputFieldAuthor)
    {
        currLevel.Author = inputFieldAuthor.text;
        NotifyOfChanges();
    }

    public Vector3 GetSentenceContextPosition()
    {
        if (selectedSentenceObject < 0)
        {
            return new Vector3(-1, -1, -1);
        }
        return sentenceObjects[selectedSentenceObject].GetComponent<RectTransform>().position + new Vector3(0, 40); // new Vector3(-selectedSentenceObject.GetComponent<RectTransform>().rect.width / 2, selectedSentenceObject.GetComponent<RectTransform>().rect.height / 2);
    }

    /**
     * Deselects the current selected sentence object
     */
    public void DeselectSentenceObject()
    {
        if (selectedSentenceObject > -1)
        {
            sentenceObjects[selectedSentenceObject].GetComponent<LevelSentenceObject>().Deselect();
        }
        selectedSentenceObject = -1;
        SentenceContextMenu.SetActive(false);
        SentenceSettingsMenu.SetActive(false);
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
     * <summary>Updates the button used to save level info.</summary>
     */
    public void UpdateButtonSaveInfo()
    {
        bool interactable = true;
        string fileInput = InputFileName.GetComponent<TMPro.TMP_InputField>().text;
        string authorInput = InputAuthorName.GetComponent<TMPro.TMP_InputField>().text;
        string levelInput = InputLevelName.GetComponent<TMPro.TMP_InputField>().text;
        string warning = "";
        if (fileInput.Length <= 0)
        {
            interactable = false;
            warning = "File name is empty\nファイル名無し。";
        }
        // author field empty
        if (authorInput.Length <= 0)
        {
            interactable = false;
            warning = "Author is empty\n作者不明。";
        }
        // level name field empty
        if (levelInput.Length <= 0)
        {
            interactable = false;
            warning = "Level name is empty\nレベル名無し。";
        }
        // level name contains characters incompatible with windows names
        Match match = Regex.Match(levelInput, "^([a-zA-Z0-9_ ]|-)+$");
        if (!match.Success)
        {
            interactable = false;
            warning = "Level name only supports alphanumeric, space, \"-\", and \"_\"\nレベル名に英数文字とスペースと「-」と「_」は使えます。";
        }
        ButtonSaveInfo.GetComponent<Button>().interactable = interactable;

        // set message if there's a warning
        TextFeedbackInfo.GetComponent<TMPro.TMP_Text>().SetText(warning);
    }

    /**
     * <summary>Update the UI to prevent and misunderstand general UI elements during sentence editing</summary>
     */
    public void UpdateGeneralUI(bool interactable)
    {
        ButtonSave.interactable = interactable;
        ButtonLeave.interactable = interactable;
        ButtonEditInfo.interactable = interactable;
    }

    /**
     * <summary>Updates the info fields for the level</summary>
     */
    public void UpdateLevelInfoInputFields()
    {

    }

    /**
     * <summary>Undo an action that has been performed</summary>
     */
    public void UndoEditorAction()
    {
        // are there actions to undo?
        if(editorHistory.Count <= 0
            || historyMarker < 0)
        {
            return;
        }
        DeselectSentenceObject();
        // get action
        EditorAction action = editorHistory[historyMarker];
        // move to the page with the change
        if (action.page != currPage)
        {
            GotoPage(action.page);
        }
        // undo
        action.Undo(sentenceObjects, currLevel, SentenceSettingsMenu);
        if(action is not EditorActionCreateSentence && action is not EditorActionPasteSentenceObject && action is not EditorActionDeletePage)
        {
            SetSelectedSentenceObject(sentenceObjects[action.index]);
        }
        if(action is EditorActionDeletePage)
        {
            // force reload
            GotoPage(action.page);
        }

        // update marker position
        historyMarker--;
        // update save button
        if (historyMarker != saveMarker)
        {
            NotifyOfChanges();
        }
        else
        {
            unsavedChanges = false;
            ButtonSave.interactable = false;
        }
    }

    /**
     * <summary>Redo an action that has been performed</summary>
     */
    public void RedoEditorAction()
    {
        // are there actions to redo?
        if(historyMarker + 1 >= editorHistory.Count)
        {
            return;
        }
        DeselectSentenceObject();
        EditorAction action = editorHistory[historyMarker + 1];
        // move to the page with the change
        if (action.page != currPage)
        {
            GotoPage(action.page);
        }
        // redo
        action.Perform(sentenceObjects, currLevel, SentenceSettingsMenu);
        if (action is not EditorActionDeleteSentence && action is not EditorActionDeletePage)
        {
            SetSelectedSentenceObject(sentenceObjects[action.index]);
        }
        if (action is EditorActionDeletePage)
        {
            if (currPage > 0)
            {
                GotoPage(currPage - 1);
            }
            else if (currPage == 0)
            {
                GotoPage(currPage);
            }
        }
        historyMarker++;
        // update save button
        if(historyMarker != saveMarker)
        {
            NotifyOfChanges();
        }
        else
        {
            unsavedChanges = false;
            ButtonSave.interactable = false;
        }
    }

    void AddEditorAction(EditorAction editorAction)
    {
        NotifyOfChanges();
        // marker not at end, remove everything after marker
        if (historyMarker < editorHistory.Count - 1)
        {
            editorHistory.RemoveRange(historyMarker + 1, editorHistory.Count - (historyMarker + 1));
        }
        // TODO: limit how many actions there can be to prevent out of memory

        // perform action
        editorAction.Perform(sentenceObjects, currLevel, SentenceSettingsMenu);
        // add action
        editorHistory.Add(editorAction);
        historyMarker++;
    }
}
