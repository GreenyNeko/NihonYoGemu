using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public static GameManager Instance
    {
        get;
        private set;
    }

    // history handler to update the history visualizer
    public HistoryHandler ScriptHistoryHandler;

    private float scoreMultiplier = 1f; // multiplies the score given the games mods
    private int score;                  // keep track of players score
    private int correctKanji;           // keep track of how many kanji are read correctly
    private int sloppyKanji;            // keep track of misread kanji but they have the reading
    private int missedKanji;            // keep track of kanji that were misread, not known or skipped
    private int combo;                  // the combo the player got
    private float accuracy;             // % given the correct, sloppy and missed kanji
    private float hitRate;              // sloppy + correct kanji
    private int maxCombo;               // the highest the combo has been
    private bool running;               // whether or not the game is running
    private bool paused;                // whether or not the game is paused

    // Start is called before the first frame update
    void Start()
    {
        // init
        Instance = this;
        score = 0;
        correctKanji = 0;
        sloppyKanji = 0;
        missedKanji = 0;
        combo = 0;
        accuracy = 0;
        hitRate = 0;
        maxCombo = 0;
        running = true;
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
        paused = pause;
        PauseMenu.SetActive(pause);
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
        SceneManager.LoadScene("LevelSelect");
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

    // returns the rank given your accuracy, hitrate and missed kanji's
    private string getRank()
    {
        if (accuracy == 1) return "SS";
        else if (accuracy > 0.9 && missedKanji == 0) return "S";
        else if (accuracy > 0.9 && hitRate > 0.8) return "A";
        else if (accuracy > 0.8 && hitRate > 0.6) return "B";
        else if (accuracy > 0.6 && hitRate > 0.4) return "C";
        else if (accuracy > 0.4) return "D";
        return " ";
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
        TextRank.SetText(getRank());
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
        TextEndRank.SetText(getRank());
        TextEndScore.SetText(strScore);
        TextEndCorrect.SetText(correctKanji.ToString() + "x");
        TextEndSloppy.SetText(sloppyKanji.ToString() + "x");
        TextEndMiss.SetText(missedKanji.ToString() + "x");
        TextEndCombo.SetText(maxCombo.ToString() + "x");
        TextEndAccuracy.SetText((accuracy * 100).ToString("F2") + "%");
    }
}
