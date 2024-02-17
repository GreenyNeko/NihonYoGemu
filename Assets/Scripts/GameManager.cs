using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    // UI menues to hide/show
    public GameObject OverlayRunning;
    public GameObject OverlayDone;
    public GameObject PauseMenu;

    // UI elemnts to update in running menu
    public TMPro.TMP_Text TextScore;
    public TMPro.TMP_Text TextAccuracy;
    public TMPro.TMP_Text TextRank;
    public TMPro.TMP_Text TextCombo;

    // UI elements to update in end screen menu
    public TMPro.TMP_Text TextEndRank;
    public TMPro.TMP_Text TextEndScore;
    public TMPro.TMP_Text TextEndCorrect;
    public TMPro.TMP_Text TextEndSloppy;
    public TMPro.TMP_Text TextEndMiss;
    public TMPro.TMP_Text TextEndCombo;
    public TMPro.TMP_Text TextEndAccuracy;

    /**
     * instance to always access the game manager from wherever we need
     */
    /*public static GameManager Instance
    {
        get;
        private set;
    }*/

    // history handler to update the history visualizer
    public HistoryHandler ScriptHistoryHandler;

    [HideInInspector]
    public string LevelName;            // stores the level name for which the highscore should be saved
    [HideInInspector]
    public GameMods Mods;               // stores the mods of the game manager

    private float scoreMultiplier = 1f; // multiplies the score given the games mods
    private int score;                  // keep track of players score
    private int correctKanji;           // keep track of how many kanji are read correctly
    private int sloppyKanji;            // keep track of misread kanji but they have the reading
    private int missedKanji;            // keep track of kanji that were misread, not known or skipped
    private int combo;                  // the combo the player got
    private float accuracy;             // % given the correct, sloppy and missed kanji
    private float hitRate;              // sloppy + correct kanji
    private int maxCombo;               // the highest the combo has been
    private bool running = true;        // whether or not the game is running
    private bool paused;                // whether or not the game is paused

    // Start is called before the first frame update
    void Awake()
    {
        // init
        //Instance = this;
        score = 0;
        correctKanji = 0;
        sloppyKanji = 0;
        missedKanji = 0;
        combo = 0;
        accuracy = 0;
        hitRate = 0;
        maxCombo = 0;
        // don't show end screen, show game screen
        OverlayDone.SetActive(false);
        OverlayRunning.SetActive(true);
        PauseMenu.SetActive(false);
        // pre setup the UI
        updateUI();
    }

    /**
     * Suspends the game
     */
    public void PauseGame(bool pause)
    {
        if(running)
        {
            paused = pause;
            PauseMenu.SetActive(pause);
        }
    }

    public void TogglePause()
    {
        PauseGame(!paused);
    }

    /**
     * Resets the game progress (score, accuracy, etc.)
     */
    public void ResetGame()
    {
        score = 0;
        correctKanji = 0;
        sloppyKanji = 0;
        missedKanji = 0;
        combo = 0;
        accuracy = 0;
        hitRate = 0;
        maxCombo = 0;
        updateUI();
    }

    /**
     * Tells the game manager a kanji has been read correct
     */
    public void OnCorrect()
    {
        if(running)
        {
            correctKanji++;
            combo++;
            score += (int)((300 * combo) * scoreMultiplier);
            updateAccAndRate();
            updateUI();
            ScriptHistoryHandler.RegisterHit(0);
        }
    }

    /**
     * Tells the game manager a kanji has been read sloppy
     */
    public void OnSloppy()
    {
        if(running)
        {
            sloppyKanji++;
            combo++;
            score += (int)((100 * combo) * scoreMultiplier);
            updateAccAndRate();
            updateUI();
            ScriptHistoryHandler.RegisterHit(1);
        }
    }

    /**
     * Tells the game manager a kanji has been missed/misread/skipped
     */
    public void OnMiss()
    {
        if(running)
        {
            missedKanji++;
            if (combo > maxCombo) maxCombo = combo;
            combo = 0;
            updateAccAndRate();
            updateUI();
            ScriptHistoryHandler.RegisterHit(2);
        }
    }

    /**
     * Called when escape has been pressed and shows the end screen, if it already shows end screen it switches to game screen and resets the game
     */
    public void OnEnd()
    {
        if(running)
        {
            // update UI
            updateEndScreenUI();
            // change menu
            OverlayDone.SetActive(true);
            OverlayRunning.SetActive(false);
            // game has ended
            running = false;
        }
        else
        {
            // save highscore
            SaveHighscore(LevelName);
            EndGame();  
        }
    }

    /**
     * Ends the game, cleans up and returns to level select
     */
    public void EndGame()
    {
        // reset the game
        score = 0;
        correctKanji = 0;
        sloppyKanji = 0;
        missedKanji = 0;
        combo = 0;
        accuracy = 0;
        hitRate = 0;
        maxCombo = 0;
        // update UI
        updateUI();

        // change menu
        OverlayDone.SetActive(false);
        OverlayRunning.SetActive(true);
        // set the game as running
        running = true;
        // leave to main menu
        MemoryStream memoryStream = new MemoryStream();
        using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
        {
            binaryWriter.Write(SceneManager.GetActiveScene().buildIndex);
            binaryWriter.Write(false); // if we played a level we can't be in the editor
            binaryWriter.Write(LevelName);
            binaryWriter.Write((int)Mods);
        }
        SceneManagerPlus.LoadScene("LevelSelect", memoryStream.ToArray());
    }

    /**
     * Calculates the score multiplier given which mods are active
     */
    public void SetScoreMultiplier(float multiplier)
    {
        scoreMultiplier = multiplier;
    }

    /**
     * Returns whether or not the game is running and able to change
     */
    public bool IsRunning()
    {
        return running;
    }

    /**
     * Returns whether or not the game is paused
     */
    public bool IsPaused()
    {
        return paused;
    }

    // saves this highscore that was just performed
    private void SaveHighscore(string levelName)
    {
        // get the leaderboard from file
        Leaderboard leaderboard = Leaderboard.LoadLeaderboardByName(levelName);
        // couldn't load because file doesn't exist
        if(leaderboard == null)
        {
            // create a new leaderboard
            leaderboard = new Leaderboard(); 
        }
        // store the highscore
        var highScore = new Leaderboard.HighScore();
        highScore.score = (uint)score;
        highScore.accuracy = Mathf.Round(accuracy*10000) / 100.0f;
        highScore.combo = (ushort)combo;
        highScore.sloppy = (ushort)sloppyKanji;
        highScore.miss = (ushort)missedKanji;
        highScore.correct = (ushort)correctKanji;
        highScore.mods = Mods;
        highScore.username = "Fluffles";
        highScore.rank = (sbyte)getRank();
        highScore.timestamp = (ulong)System.DateTimeOffset.Now.UtcTicks;
        leaderboard.Add(highScore);
        // save the leaderboard to drive
        leaderboard.Save(levelName);
    }

    // returns the string representation of the ranks instead of the internal integers
    private string getRankString()
    {
        switch(getRank())
        {
            case 6:
                return "SS";
            case 5:
                return "S";
            case 4:
                return "A";
            case 3:
                return "B";
            case 2:
                return "C";
            case 1:
                return "D";
            default:
                return "";
        }

    }

    // returns the rank given the players performance
    private int getRank()
    {
        if (accuracy == 1) return 6;
        else if (accuracy > 0.9 && missedKanji == 0) return 5;
        else if (accuracy > 0.9 && hitRate > 0.8) return 4;
        else if (accuracy > 0.8 && hitRate > 0.6) return 3;
        else if (accuracy > 0.6 && hitRate > 0.4) return 2;
        else if (accuracy > 0.4) return 1;
        return 0;
    }

    // updates the accuracy and hitrate
    private void updateAccAndRate()
    {
        accuracy = (correctKanji + sloppyKanji * 0.5f) / (correctKanji + sloppyKanji + missedKanji);
        hitRate = (float)(correctKanji + sloppyKanji) / (correctKanji + sloppyKanji + missedKanji);
    }

    // updates the UI elements of the game
    private void updateUI()
    {
        string strScore = score.ToString();
        // adds 0's at the beginning
        while (strScore.Length < 9)
        {
            strScore = "0" + strScore;
        }
        // updates UI elements
        TextScore.SetText(strScore);
        TextRank.SetText(getRankString());
        TextAccuracy.SetText((accuracy * 100).ToString("F2") + "%");
        TextCombo.SetText(combo.ToString() + "x");
    }

    // update the UI elements of the end screen
    private void updateEndScreenUI()
    {
        string strScore = score.ToString();
        // adds 0's at the beginning
        while (strScore.Length < 9)
        {
            strScore = "0" + strScore;
        }
        if (missedKanji == 0) maxCombo = combo;
        // updates UI elements
        TextEndRank.SetText(getRankString());
        TextEndScore.SetText(strScore);
        TextEndCorrect.SetText(correctKanji.ToString() + "x");
        TextEndSloppy.SetText(sloppyKanji.ToString() + "x");
        TextEndMiss.SetText(missedKanji.ToString() + "x");
        TextEndCombo.SetText(maxCombo.ToString() + "x");
        TextEndAccuracy.SetText((accuracy * 100).ToString("F2") + "%");
    }
}
