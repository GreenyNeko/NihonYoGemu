using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

/**
 * This scripts manages the progress through the level.
 * If you want the script that handles scores see the GameManager instead.
 */
public class LevelManager : MonoBehaviour
{
    public GameManager ScriptGameManager;   // Reference to the game manager script
    public Image ProgressImage;             // Reference to the image showing level progress
    public TMPro.TMP_Text InputText;

    public Transform SentenceParent;
    public GameObject SentenceObjectPrefab;
    public Image BackgroundImage;

    Level loadedLevel;
    int currPage;
    int currSentence;
    int currKanji;
    int progress;                           // tracks how many kanjis been answered
    List<(string,int)> currKanjis;
    List<GameObject> sentenceObjects;
    List<Sprite> BackgroundSprites;
    List<Texture2D> BackgroundTextures;

    void Awake()
    {
        sentenceObjects = new List<GameObject>();
        currKanjis = new List<(string,int)>();
        // read from data
        int sceneIdx = -1;
        string selectedLevelName;

        MemoryStream memoryStream = new MemoryStream(SceneManagerPlus.GetCurrentData());
        using (BinaryReader reader = new BinaryReader(memoryStream))
        {
            sceneIdx = reader.ReadInt32();              // the scene we came from
            reader.ReadBoolean();                       // editor flag is unneeded here
            selectedLevelName = reader.ReadString();    // the level to load
            // get the mods 
            ScriptGameManager.Mods = (GameMods)reader.ReadInt32();
        }
        loadedLevel = LevelLoader.LoadLevelByName(selectedLevelName);
        if (loadedLevel == null)
        {
            // failed to load level return to level select
            Debug.LogWarning("Unable to load level. Aborting play mode");
            memoryStream = new MemoryStream();
            using(BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write(SceneManager.GetActiveScene().buildIndex);
                binaryWriter.Write(false); // if we played a level we can't be in the editor
                binaryWriter.Write(selectedLevelName);
                binaryWriter.Write((int)ScriptGameManager.Mods);
            }
            SceneManagerPlus.LoadScene("LevelSelect", memoryStream.ToArray());
        }
        BackgroundTextures = new List<Texture2D>();
        BackgroundSprites = new List<Sprite>();
        // load backgrounds
        for(int i = 0; i < loadedLevel.pages.Count; i++)
        {
            BackgroundTextures.Add(new Texture2D(1, 1));
            BackgroundTextures[i].LoadImage(loadedLevel.pages[i].backgrounImageData);
            BackgroundSprites.Add(Sprite.Create(BackgroundTextures[i], new Rect(0, 0, BackgroundTextures[i].width, BackgroundTextures[i].height), new Vector2(0.5f, 0.5f)));
        }
        // scale level
        ScaleLevel();
        // add offset to input field position
        InputText.transform.parent.localPosition = new Vector3(loadedLevel.inputOffsetX, -loadedLevel.inputOffsetY, 0);
    }

    void OnDisable()
    {
        for (int i = 0; i < loadedLevel.pages.Count; i++)
        {
            Destroy(BackgroundTextures[i]);
            Destroy(BackgroundSprites[i]);
        }
        BackgroundTextures.Clear();
        BackgroundSprites.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        currPage = 0;
        currSentence = 0;
        currKanji = 0;
        // calculate score multiplier
        // store because we'll need it a lot
        ScriptGameManager.SetScoreMultiplier(ScriptGameManager.Mods.GetModMultiplier());
        ScriptGameManager.LevelName = loadedLevel.FileName;
        // level is completed by default
        if (loadedLevel.GetSentenceCount() <= 0)
        {
            ScriptGameManager.OnEnd();
        }
        UpdateCurrentKanjis();
        CreateSentenceObjects();
        UpdateBackground();
        UpdateSentenceObjects();
        UpdateProgressImage();
    }

    /**
     * Resets the level and game manager
     */
    public void ResetLevel()
    {
        currPage = 0;
        currSentence = 0;
        currKanji = 0;
        progress = 0;
        ScriptGameManager.ResetGame();
        // level is completed by default
        if (loadedLevel.GetSentenceCount() <= 0)
        {
            ScriptGameManager.OnEnd();
        }
        UpdateCurrentKanjis();
        ClearPage();
        CreateSentenceObjects();
        UpdateBackground();
        UpdateSentenceObjects();
        UpdateProgressImage();
    }

    /**
     * Progresses the level, passes performance to game manager and starts the end screen also leaves level given what game manager does (See. GameManager.OnEnd)
     */
    public void Progress()
    {
        if(!ScriptGameManager.IsRunning())
        {
            ScriptGameManager.OnEnd();
        }
        else
        {
            // determine score
            // are there kanjis in the current sentence
            Level.SentenceData sentenceData = loadedLevel.pages[currPage].sentenceObjects[currSentence];
            // if this sentence has furigana
            if (sentenceData.furigana.Length > 0)
            {
                // clean up input (input should be what we see
                string playerFurigana = InputText.text;
                // compare current input vs kanji reading
                if (playerFurigana == sentenceData.furigana[currKanji])
                {
                    ScriptGameManager.OnCorrect();
                }
                else
                {
                    if (JapaneseDictionary.StringInReadings(playerFurigana, currKanjis[currKanji].Item1[0]))
                    {
                        ScriptGameManager.OnSloppy();
                    }
                    else
                    {
                        ScriptGameManager.OnMiss();
                    }
                }
            }

            // undo marking
            sentenceObjects[currSentence].GetComponent<LevelSentenceObject>().UnmarkCharacter();
            Transform furiganaObject = sentenceObjects[currSentence].transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().furiganaParent;
            if(furiganaObject.childCount > 0)
            {
                furiganaObject.GetChild(0).GetComponent<TMPro.TMP_Text>().color = sentenceObjects[currSentence].GetComponent<LevelSentenceObject>().DisplayedText.color;
            }
            

            // progress
            progress++;
            currKanji++;
            if(currKanji >= sentenceData.furigana.Length)
            {
                // all kanji read, next sentence
                currKanji = 0;
                // skip to the next kanji or end of page
                do
                {
                    currSentence++;
                } while (currSentence < loadedLevel.pages[currPage].sentenceObjects.Count && loadedLevel.pages[currPage].sentenceObjects[currSentence].furigana.Length == 0);
                // end of page
                if(currSentence >= loadedLevel.pages[currPage].sentenceObjects.Count)
                {
                    // all sentences read, next page
                    currSentence = 0;
                    currPage++;
                    if(currPage >= loadedLevel.pages.Count)
                    {
                        currPage = 0;
                        ClearPage();
                        UpdateBackground();
                        ScriptGameManager.OnEnd();
                        return;
                    }
                    UpdateBackground();
                    ClearPage();
                    CreateSentenceObjects();
                }
                UpdateCurrentKanjis();
            }

            UpdateSentenceObjects();
            UpdateProgressImage();
        }
    }

    void UpdateBackground()
    {
        if (BackgroundSprites[currPage] != null)
        {
            BackgroundImage.GetComponent<Image>().color = Color.white;
            BackgroundImage.GetComponent<Image>().sprite = BackgroundSprites[currPage];
            float width = 0, height = 0;
            Vector2 diff;
            float scaleFactor = 1;
            Debug.Log("image scale mode" + loadedLevel.pages[currPage].scaleMode);
            switch (loadedLevel.pages[currPage].scaleMode)
            {
                case 4:
                    // scale
                    width = Screen.width;
                    height = Screen.height;
                    break;
                case 3:
                    // scale up
                    // determine if scaling up or keep
                    width = loadedLevel.nativeX > Screen.width ? loadedLevel.nativeX : Screen.width;
                    height = loadedLevel.nativeY > Screen.height ? loadedLevel.nativeY : Screen.height;
                    break;
                case 2:
                    // keep aspect ratio
                    // determine smaller difference between width and height
                    diff = new Vector2(Screen.width, Screen.height) - BackgroundSprites[currPage].rect.size;
                    if (diff.x < diff.y)
                    {
                        // scale up to screen width
                        scaleFactor = (float)Screen.width / BackgroundSprites[currPage].rect.width;
                    }
                    else
                    {
                        // scale up to screen height
                        scaleFactor = (float)Screen.height / BackgroundSprites[currPage].rect.height;
                    }
                    width = BackgroundSprites[currPage].rect.width * scaleFactor;
                    height = BackgroundSprites[currPage].rect.height * scaleFactor;
                    break;
                case 1:
                    // keep aspect ratio, only scale up
                    // determine if scaling up or keep
                    width = loadedLevel.nativeX > Screen.width ? loadedLevel.nativeX : Screen.width;
                    height = loadedLevel.nativeY > Screen.height ? loadedLevel.nativeY : Screen.height;
                    // determine scaling factor for the fitting axis
                    diff = new Vector2(width, height) - BackgroundSprites[currPage].rect.size;
                    if (diff.x < diff.y)
                    {
                        // scale up to screen or native width
                        scaleFactor = (float)width / BackgroundSprites[currPage].rect.width;
                    }
                    else
                    {
                        // scale up to screen or native height
                        scaleFactor = (float)height / BackgroundSprites[currPage].rect.height;
                    }
                    // width and height set to sprite's
                    width = BackgroundSprites[currPage].rect.width * scaleFactor;
                    height = BackgroundSprites[currPage].rect.height * scaleFactor;
                    break;
                default:
                    // keep scale of image
                    // width and height set to sprite's
                    width = loadedLevel.nativeX;
                    height = loadedLevel.nativeY;
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

    void ClearPage()
    {
        for(int i = 0; i < sentenceObjects.Count; i++)
        {
            Destroy(sentenceObjects[i]);
        }
        sentenceObjects.Clear();
    }

    void CreateSentenceObjects()
    {
        for (int i = 0; i < loadedLevel.pages[currPage].sentenceObjects.Count; i++)
        {
            Level.SentenceData sentenceData = loadedLevel.pages[currPage].sentenceObjects[i];
            GameObject sentenceObject = Instantiate(SentenceObjectPrefab, SentenceParent);
            sentenceObject.GetComponent<LevelSentenceObject>().template = false;
            // convert level coords to ui coords
            sentenceObject.GetComponent<RectTransform>().anchoredPosition = sentenceData.rect.position - new Vector2(50, -50);
            sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sentenceData.rect.width);
            sentenceObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sentenceData.rect.height);
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.text = sentenceData.text;
            if(ScriptGameManager.Mods.HasFlag(GameMods.Furigana))
            {
                // update to create furigana
                sentenceObject.transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().UpdatedSentence(sentenceData.text);
                // set furigana
                for (int j = 0; j < sentenceData.furigana.Length; j++)
                {
                    TMPro.TMP_Text furigana = sentenceObject.transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().transform.GetChild(j).GetComponent<TMPro.TMP_Text>();
                    furigana.text = sentenceData.furigana[j];
                    if (sentenceData.bold)
                    {
                        furigana.fontStyle = TMPro.FontStyles.Bold;
                    }
                    else
                    {
                        furigana.fontStyle = TMPro.FontStyles.Normal;
                    }
                    if (sentenceData.color)
                    {
                        furigana.color = Color.black;
                    }
                    else
                    {
                        furigana.color = Color.white;
                    }
                }
            }
            sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.fontSize = sentenceData.textSize;
            if (sentenceData.bold)
            {
                sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.fontStyle = TMPro.FontStyles.Bold;
            }
            else
            {
                sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.fontStyle = TMPro.FontStyles.Normal;
            }
            if (sentenceData.color)
            {
                sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.color = Color.black;
                if(sentenceObject.GetComponent<LevelSentenceObject>().inputField != null)
                {
                    sentenceObject.GetComponent<LevelSentenceObject>().inputField.textComponent.color = Color.black;
                }
            }
            else
            {
                sentenceObject.GetComponent<LevelSentenceObject>().DisplayedText.color = Color.white;
                if (sentenceObject.GetComponent<LevelSentenceObject>().inputField != null)
                {
                    sentenceObject.GetComponent<LevelSentenceObject>().inputField.textComponent.color = Color.white;
                }
            }
            sentenceObject.GetComponentInChildren<OrientedText>().vertical = sentenceData.vertical;
            sentenceObject.GetComponent<LevelSentenceObject>().sentenceDataIndex = i;

            sentenceObjects.Add(sentenceObject);
        }
    }

    // updates the text, furigana and which kanji is marked
    void UpdateSentenceObjects()
    {
        if(currKanjis.Count > 0)
        {
            sentenceObjects[currSentence].GetComponent<LevelSentenceObject>().MarkCharacter(currKanjis[currKanji].Item2);
            Transform furiganaObject = sentenceObjects[currSentence].transform.GetChild(0).GetComponent<OrientedText>().FuriganaHandler.GetComponent<SentenceFurigana>().furiganaParent;
            if(furiganaObject.childCount > 0)
            {
                furiganaObject.GetChild(currKanji).GetComponent<TMPro.TMP_Text>().color = new Color(1, 0, 0);
            }
        }
    }

    // updates the image showing the level progress
    void UpdateProgressImage()
    {
        ProgressImage.fillAmount = ((float)(progress) / loadedLevel.GetKanjiCount());
    }

    void UpdateCurrentKanjis()
    {
        currKanjis.Clear();
        if(currPage >= loadedLevel.pages.Count || currSentence >= loadedLevel.pages[currPage].sentenceObjects.Count)
        {
            return;
        }
        for (int i = 0; i < loadedLevel.pages[currPage].sentenceObjects[currSentence].text.Length; i++)
        {
            char c = loadedLevel.pages[currPage].sentenceObjects[currSentence].text[i];
            if (JapaneseDictionary.IsKanji(c.ToString()))
            {
                currKanjis.Add((c.ToString(), i));
            }
        }
    }

    void ScaleLevel()
    {
        // by default scale 1, scales to full screen
        // => 1 = Screen.width by Screen.height
        Vector2 native = new Vector2(loadedLevel.nativeX, loadedLevel.nativeY);
        Vector2 screen = new Vector2(Screen.width, Screen.height);
        Vector3 diff = screen - native;
        float scaleFactorX, scaleFactorY;
        float resultWidth, resultHeight;
        Debug.Log(loadedLevel.scaleMode);
        switch (loadedLevel.scaleMode)
        {
            case 4:
                // scale
                scaleFactorX = (float)Screen.width / loadedLevel.nativeX;
                scaleFactorY = (float)Screen.height / loadedLevel.nativeY;
                SentenceParent.localScale = new Vector3(scaleFactorX, scaleFactorY, 1);
                break;
            case 3:
                // scale up
                scaleFactorX = Mathf.Max((float)Screen.width / loadedLevel.nativeX, 1f);
                scaleFactorY = Mathf.Max((float)Screen.height / loadedLevel.nativeY, 1f);
                SentenceParent.localScale = new Vector3(scaleFactorX, scaleFactorY, 1);
                break;
            case 2:
                // aspect ratio
                // scale up the smaller axis
                if (diff.x < diff.y)
                {
                    // scale x axis to Screen.width
                    scaleFactorX = (float)Screen.width / loadedLevel.nativeX;
                }
                else
                {
                    // scale x axis to Screen.width
                    scaleFactorX = (float)Screen.height / loadedLevel.nativeY;
                }
                SentenceParent.localScale *= scaleFactorX;
                resultWidth = native.x * scaleFactorX;
                resultHeight = native.y * scaleFactorX;
                if(resultWidth < Screen.width)
                {
                    SentenceParent.GetComponent<RectTransform>().position += new Vector3((Screen.width - resultWidth) / 2, 0, 0);
                }
                else if(resultHeight < Screen.height)
                {
                    SentenceParent.GetComponent<RectTransform>().position += new Vector3(0, (Screen.height - resultWidth) / 2, 0);
                }
                break;
            case 1:
                // aspect ratio, scale up
                // scale up the smaller axis
                Debug.Log(diff.x + " " + diff.y);
                if(diff.x < diff.y)
                {
                    // scale x axis to Screen.width
                    scaleFactorX = (float)Screen.width / loadedLevel.nativeX;
                }
                else
                {
                    // scale x axis to Screen.width
                    scaleFactorX = (float)Screen.height / loadedLevel.nativeY;
                }
                Debug.Log(scaleFactorX);
                if(scaleFactorX > 1)
                {
                    SentenceParent.localScale *= scaleFactorX;
                }
                resultWidth = native.x * scaleFactorX;
                resultHeight = native.y * scaleFactorX;
                if (resultWidth < Screen.width)
                {
                    SentenceParent.GetComponent<RectTransform>().position += new Vector3((Screen.width - resultWidth) / 2, 0, 0);
                }
                else if (resultHeight < Screen.height)
                {
                    SentenceParent.GetComponent<RectTransform>().position += new Vector3(0, (Screen.height - resultWidth) / 2, 0);
                }
                break;
            default:
                // keep
                // handle offset
                //SentenceParent.GetComponent<RectTransform>().position -= diff / 2;
                break;
        }
    }
}
