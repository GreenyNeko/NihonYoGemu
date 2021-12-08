using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * This class is used to start a level, it transports information from level select to the game
 */
public class GameStarter : MonoBehaviour
{
    GameMods mods;                      // which mods are active
    Level loadedLevel;                  // the level information needed for the game

    // the instance to itself, allowing other scripts to easily access it
    public static GameStarter Instance
    {
        get;
        private set;
    }

    private void Start()
    {
        Instance = this; // store instance to this script
    }

    /**
     * Start the level loading the input file and level and load the game
     */
    public void StartLevel(string levelName)
    {
        // now load the selected input method
        JapaneseDictionary.CreateKanaFromInputFileId(0);
        // make the object persist into the next scene
        DontDestroyOnLoad(gameObject);
        // get the level that should be started
        loadedLevel = LevelLoader.LoadLevelByName(levelName);
        if(loadedLevel == null)
        {
            Debug.LogWarning("Unable to load level");
        }
        else
        {
            SceneManager.LoadScene("GameScene");
        }
        
    }

    /**
     * Passes on the level (we don't wanna keep this object)
     */
    public Level GetLevel()
    {
        return loadedLevel;
    }

    /**
     * Returns the resulting score multiplier given the active mods
     */
    public float GetScoreMultiplier()
    {
        float scoreMultiplier = 1f;
        if (mods.HasFlag(GameMods.Furigana))
        {
            scoreMultiplier -= 1f;
        }
        // prevent underflow
        if (scoreMultiplier < 0f)
        {
            scoreMultiplier = 0f;
        }
        return scoreMultiplier;
    }

    /**
     * Returns whether or not the given mod is active
     */
    public bool HasMod(GameMods mod)
    {
        return mods.HasFlag(mod);
    }

    /**
     * Adds a new mod to the mods
     */
    public void AddMod(GameMods mod)
    {
        mods |= mod;
    }

    /**
     * Removes a mod from the active mods
     */
    public void RemoveMod(GameMods mod)
    {
        mods &= ~mod;
    }
}
