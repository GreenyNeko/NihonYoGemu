using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * This scripts manages the progress through the level.
 * If you want the script that handles scores see the GameManager instead.
 */
public class LevelManager : MonoBehaviour
{
    public TMPro.TMP_Text SentencesOutput;  // Text element the sentence will be put into
    public GameObject Furiganas;            // Parent of all furigana text elements
    public GameManager ScriptGameManager;   // Reference to the game manager script
    public Image ProgressImage;             // Reference to the image showing level progress

    public GameObject FuriganaPrefab;       // prefab to create new furigana elements

    GameStarter gameStarter;                // for storing the instance

    int currLine;                           // tracks the current line
    int progress;                           // tracks how many kanjis been answered
    int prevLinesKanjiCount;                // sum of kanjis in previous lines
    int kanjiCount;                         // how many kanjis have been answered

    // Start is called before the first frame update
    void Start()
    {
        currLine = 0;
        kanjiCount = 0;
        prevLinesKanjiCount = 0;
        // store because we'll need it a lot
        gameStarter = GameStarter.Instance;
        ScriptGameManager.SetScoreMultiplier(gameStarter.GetScoreMultiplier());
        // load furigana elements if furigana mod is active
        if(gameStarter.HasMod(GameMods.Furigana))
        {
            // load furigana elements given the maximum kanjis per sentence
            for (int i = 0; i < gameStarter.GetLevel().GetMostKanjisPerSentence(); i++)
            {
                // we don't need to store a reference, we'll access them through transform.GetChild(...)
                Instantiate(FuriganaPrefab, Furiganas.transform);
            }
        }
        ScriptGameManager.LevelName = gameStarter.GetLevel().Name;
        ScriptGameManager.Mods = gameStarter.GetGameMods();
        // level is completed by default
        if (gameStarter.GetLevel().GetSentenceCount() <= 0)
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
        if (gameStarter.GetLevel().GetSentenceCount() <= 0)
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
            if (gameStarter.GetLevel().GetKanjiFromSentence(currLine).Length > 0)
            {
                // clean up input (input should be what we see
                string playerFurigana = JapaneseDictionary.ConvertRomajiToKana(input);
                // compare current input vs kanji reading
                if (playerFurigana == gameStarter.GetLevel().GetFuriganaFromKanji(kanjiCount))
                {
                    ScriptGameManager.OnCorrect();
                }
                else
                {
                    if (JapaneseDictionary.StringInReadings(playerFurigana, gameStarter.GetLevel().GetKanjiFromSentence(currLine)[progress]))
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

            if (progress >= gameStarter.GetLevel().GetKanjiFromSentence(currLine).Length)
            {
                // update the previous kanji counting the current kanjiCount after the last line been completed
                prevLinesKanjiCount = kanjiCount;
                currLine++;
                // end of level
                if (currLine >= gameStarter.GetLevel().GetSentenceCount())
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
        if(currLine < gameStarter.GetLevel().GetSentenceCount())
        {
            // get the currline after the increment
            string line = gameStarter.GetLevel().GetLine(currLine);
            SentencesOutput.SetText(line);
            SentencesOutput.ForceMeshUpdate();
            int kanjiCount = gameStarter.GetLevel().GetKanjiFromSentence(currLine).Length;

            if(gameStarter.HasMod(GameMods.Furigana))
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
                    int tmp = gameStarter.GetLevel().GetFuriganaFromKanji(prevLinesKanjiCount + i).Length;
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
                    furigana.SetText(gameStarter.GetLevel().GetFuriganaFromKanji(prevLinesKanjiCount + i));
                    // get the kanji position
                    int charIdx = line.IndexOf(gameStarter.GetLevel().GetKanjiFromSentence(currLine)[i]);
                    TMPro.TMP_CharacterInfo charInfo = SentencesOutput.textInfo.characterInfo[charIdx];
                    int vertexIndex = charInfo.vertexIndex;
                    Vector3[] vertexPositions = SentencesOutput.mesh.vertices;
                    // place the furigana above the kanji
                    furigana.transform.localPosition = new Vector3((vertexPositions[vertexIndex + 1].x + vertexPositions[vertexIndex + 2].x) / 2, vertexPositions[vertexIndex + 1].y + 2, furigana.transform.position.z);
                    furigana.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vertexPositions[vertexIndex + 2].x - vertexPositions[vertexIndex + 1].x);

                    // biggest = 26
                    // smallest = 8
                    // determine furigana size given amount of characters
                    // currently hardcoded for the best effect
                    switch(mostKana)
                    {
                        case 4:
                            furigana.fontSize = 12;
                            break;
                        case 3:
                            furigana.fontSize = 16;
                            break;
                        case 2:
                            furigana.fontSize = 20;
                            break;
                        case 1:
                            furigana.fontSize = 26;
                            break;
                        default:
                            furigana.fontSize = 9;
                            break;
                    }
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
                int charIdx = line.IndexOf(gameStarter.GetLevel().GetKanjiFromSentence(currLine)[progress]);
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
        ProgressImage.fillAmount = (float)(kanjiCount + currLine) / (gameStarter.GetLevel().GetKanjiCount() + gameStarter.GetLevel().GetSentenceCount());
    }
}
