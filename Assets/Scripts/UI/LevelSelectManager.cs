using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class LevelSelectManager : MonoBehaviour
{
    public GameObject ButtonCreateLevel;
    public GameObject UILeaderboard;
    public GameObject UIPlayButton;
    public GameObject UIModButton;

    // used to update UI elemnets on start
    public TextScoreMultiplier TextScoreMultiplierScript;
    public ModToggle ModToggleScript;

    GameMods mods;                      // which mods are active
    Level loadedLevel;                  // the level information needed for the game
    string selectedLevelName;           // the level selected to be played
    bool editorFlag;                    // are we selecting a level for the editor?

    // Start is called before the first frame update
    void Start()
    {
        int sceneIdx = -1;
        MemoryStream memoryStream = new MemoryStream(SceneManagerPlus.GetCurrentData());
        using(BinaryReader reader = new BinaryReader(memoryStream))
        {
            // sceneIdx, editorMode, otherData
            sceneIdx = reader.ReadInt32();
            // base data
            editorFlag = reader.ReadBoolean();
            ButtonCreateLevel.SetActive(editorFlag);
            if (editorFlag)
            {
                // disable all play features, change play button
                // add create level button
                UILeaderboard.SetActive(false);
                UIPlayButton.SetActive(false);
                UIModButton.SetActive(false);
                ButtonCreateLevel.SetActive(true);
            }
            if (sceneIdx == SceneUtility.GetBuildIndexByScenePath("Scenes/GameScene"))
            {
                // returning => we got more data
                selectedLevelName = reader.ReadString();
                mods = (GameMods)reader.ReadInt32();
                // update
                TextScoreMultiplierScript.UpdateScoreMultiplier(mods.GetModMultiplier());
                ModToggleScript.UpdateButtonState(mods);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // goes to previous scene when escape is pressed
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OpenPreviousScene();
        }
    }

    /**
    * Set the current selected level
    */
    public void SetSelectedLevelByName(string name)
    {
        selectedLevelName = name;
    }

    /**
     * Updates the text multiplier given the current mods
     */
    public void UpdateScoreMultiplier()
    {
        TextScoreMultiplierScript.UpdateScoreMultiplier(mods.GetModMultiplier());
    }

    /**
     * Starts the respective scene given the data and mode
     */
    public void StartLevel()
    {
        if(selectedLevelName == null || selectedLevelName == "")
        {
            // no level selected
            return;
        }
        // now load the selected input method
        JapaneseDictionary.CreateKanaFromInputFileId(0);
        // make the object persist into the next scene
        MemoryStream memoryStream = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(memoryStream))
        {
            writer.Write(SceneManager.GetActiveScene().buildIndex);
            writer.Write(editorFlag);
            writer.Write(selectedLevelName);
            writer.Write((int)mods);
        }
        if(editorFlag)
        {
            SceneManagerPlus.LoadScene("EditorScene", memoryStream.ToArray());
        }
        else
        {
            SceneManagerPlus.LoadScene("GameScene", memoryStream.ToArray());
        }
        
    }

    /**
     * Used to go to the main menu
     */
    public void OpenPreviousScene()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /**
     * Start editor to create a new level
     */
    public void CreateLevel()
    {
        string empty = "";
        MemoryStream memoryStream = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(memoryStream))
        {
            writer.Write(SceneManager.GetActiveScene().buildIndex);
            writer.Write(editorFlag);
            writer.Write(empty);
            writer.Write(0);
        }
        SceneManagerPlus.LoadScene("EditorScene", memoryStream.ToArray());
    }

    /**
     * Adds a new mod to the mods
     */
    public void AddMod(GameMods flag)
    {
        mods |= flag;
    }

    /**
     * Removes a mod from the active mods
     */
    public void RemoveMod(GameMods flag)
    {
        mods &= ~flag;
    }
}
