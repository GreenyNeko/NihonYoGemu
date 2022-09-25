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
    public FuriganaData furiganaData;       // Reference to the data about furigana
    public TMPro.TMP_Text SentencesOutput;  // Text element the sentence will be put into
    public GameObject Furiganas;            // Parent of all furigana text elements
    public GameManager ScriptGameManager;   // Reference to the game manager script
    public Image ProgressImage;             // Reference to the image showing level progress

    public GameObject FuriganaPrefab;       // prefab to create new furigana elements

    Level loadedLevel;
    int currLine;                           // tracks the current line
    int progress;                           // tracks how many kanjis been answered
    int prevLinesKanjiCount;                // sum of kanjis in previous lines
    int kanjiCount;                         // how many kanjis have been answered

    void Awake()
    {
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
    }

    // Start is called before the first frame update
    void Start()
    {
        currLine = 0;
        kanjiCount = 0;
        prevLinesKanjiCount = 0;
        // calculate score multiplier

        // store because we'll need it a lot
        ScriptGameManager.SetScoreMultiplier(ScriptGameManager.Mods.GetModMultiplier());
        // load furigana elements if furigana mod is active
        if(ScriptGameManager.Mods.HasFlag(GameMods.Furigana))
        {
            // load furigana elements given the maximum kanjis per sentence
            for (int i = 0; i < loadedLevel.GetMostKanjisPerSentence(); i++)
            {
                // we don't need to store a reference, we'll access them through transform.GetChild(...)
                Instantiate(FuriganaPrefab, Furiganas.transform);
            }
        }
        ScriptGameManager.LevelName = loadedLevel.Name;
        // level is completed by default
        if (loadedLevel.GetSentenceCount() <= 0)
        {
            ScriptGameManager.OnEnd();
        }
        updateTextMesh();
        updateProgressImage();
    }

    /**
     * Resets the level and game manager
     */
    public void ResetLevel()
    {
        currLine = 0;
        kanjiCount = 0;
        progress = 0;
        ScriptGameManager.ResetGame();
        // level is completed by default
        if (loadedLevel.GetSentenceCount() <= 0)
        {
            ScriptGameManager.OnEnd();
        }
        updateTextMesh();
        updateProgressImage();
    }


    /**
     * Progresses the level, passes performance to game manager and starts the end screen also leaves level given what game manager does (See. GameManager.OnEnd)
     */
    public void Progress(string input)
    {
        if(ScriptGameManager.IsRunning())
        {
            // determine score
            // are ther kanjis in the current sentence
            if (loadedLevel.GetKanjiFromSentence(currLine).Length > 0)
            {
                // clean up input (input should be what we see
                string playerFurigana = JapaneseDictionary.ConvertRomajiToKana(input, false);
                // compare current input vs kanji reading
                if (playerFurigana == loadedLevel.GetFuriganaFromKanji(kanjiCount))
                {
                    ScriptGameManager.OnCorrect();
                }
                else
                {
                    if (JapaneseDictionary.StringInReadings(playerFurigana, loadedLevel.GetKanjiFromSentence(currLine)[progress]))
                    {
                        ScriptGameManager.OnSloppy();
                    }
                    else
                    {
                        ScriptGameManager.OnMiss();
                    }
                }
                kanjiCount++;
            }

            // progress
            progress++;

            if (progress >= loadedLevel.GetKanjiFromSentence(currLine).Length)
            {
                // update the previous kanji counting the current kanjiCount after the last line been completed
                prevLinesKanjiCount = kanjiCount;
                currLine++;
                // end of level
                if (currLine >= loadedLevel.GetSentenceCount())
                {
                    ScriptGameManager.OnEnd();
                    return;
                }
                progress = 0;
            }
            updateTextMesh();
            updateProgressImage();
        }
    }

    // updates the text, furigana and which kanji is marked
    void updateTextMesh()
    {
        // are there more lines?
        if(currLine < loadedLevel.GetSentenceCount())
        {
            // get the currline after the increment
            string line = loadedLevel.GetLine(currLine);
            SentencesOutput.SetText(line);
            SentencesOutput.ForceMeshUpdate();
            int kanjiCount = loadedLevel.GetKanjiFromSentence(currLine).Length;

            if(ScriptGameManager.Mods.HasFlag(GameMods.Furigana))
            {
                // clean up furiganas a.k.a. hide old ones, and show again if updated
                foreach(Transform child in Furiganas.transform)
                {
                    child.gameObject.SetActive(false);
                }
                int mostKana = 0;
                // determine length of longest furigana
                for(int i = 0; i < kanjiCount; i++)
                {
                    int tmp = loadedLevel.GetFuriganaFromKanji(prevLinesKanjiCount + i).Length;
                    if(tmp > mostKana)
                    {
                        mostKana = tmp;
                    }
                }
                // fill all the furigana texts with contents
                for (int i = 0; i < kanjiCount; i++)
                {
                    Furiganas.transform.GetChild(i).gameObject.SetActive(true);
                    TMPro.TMP_Text furigana = Furiganas.transform.GetChild(i).GetComponent<TMPro.TMP_Text>();
                    // get the furigana given the offset of the kanjis of previous sentences
                    furigana.SetText(loadedLevel.GetFuriganaFromKanji(prevLinesKanjiCount + i));
                    // get the kanji position
                    int charIdx = line.IndexOf(loadedLevel.GetKanjiFromSentence(currLine)[i]);
                    TMPro.TMP_CharacterInfo charInfo = SentencesOutput.textInfo.characterInfo[charIdx];
                    int vertexIndex = charInfo.vertexIndex;
                    Vector3[] vertexPositions = SentencesOutput.mesh.vertices;
                    // place the furigana above the kanji
                    furigana.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vertexPositions[vertexIndex + 2].x - vertexPositions[vertexIndex + 1].x);
                    furigana.transform.localPosition = new Vector3((vertexPositions[vertexIndex + 1].x + vertexPositions[vertexIndex + 2].x) / 2, vertexPositions[vertexIndex + 1].y - 32, furigana.transform.position.z);
                    // determine furigana size given amount of characters
                    furigana.fontSize = furiganaData.GetSizeByCount(mostKana);
                    // if this kanji is marked, color the text red as well, else make it white
                    if (i == progress)
                    {
                        furigana.color = Color.red;
                    }
                    else
                    {
                        furigana.color = Color.white;
                    }
                }
            }

            // line not completed yet
            if (kanjiCount > 0 && progress < kanjiCount)
            {
                // get current selected kanji and color it
                int charIdx = line.IndexOf(loadedLevel.GetKanjiFromSentence(currLine)[progress]);
                TMPro.TMP_CharacterInfo charInfo = SentencesOutput.textInfo.characterInfo[charIdx];
                int vertexIndex = charInfo.vertexIndex;
                int meshIndex = charInfo.materialReferenceIndex;
                Color32[] vertexColors = SentencesOutput.textInfo.meshInfo[meshIndex].colors32;
                vertexColors[vertexIndex + 0] = new Color(1, 0, 0);
                vertexColors[vertexIndex + 1] = new Color(1, 0, 0);
                vertexColors[vertexIndex + 2] = new Color(1, 0, 0);
                vertexColors[vertexIndex + 3] = new Color(1, 0, 0);

                SentencesOutput.UpdateVertexData(TMPro.TMP_VertexDataUpdateFlags.Colors32);
            }
        }
    }

    // updates the image showing the level progress
    void updateProgressImage()
    {
        ProgressImage.fillAmount = (float)(kanjiCount + currLine) / (loadedLevel.GetKanjiCount() + loadedLevel.GetSentenceCount());
    }
}
