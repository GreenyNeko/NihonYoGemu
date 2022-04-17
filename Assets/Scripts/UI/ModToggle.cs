using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Handles the mod buttons in the mod menu
 */
public class ModToggle : MonoBehaviour
{
    public LevelSelectManager scriptLevelSelectManager;
    public GameMods Mod;                   // Which mod is toggled by this button?

    /**
     * Called by UI events, toggles the given mod in the gamestarter
     */
    public void OnToggle(bool value)
    {
        if(value)
        {
            scriptLevelSelectManager.AddMod(Mod);
        }
        else
        {
            scriptLevelSelectManager.RemoveMod(Mod);
        }
    }

    /**
     * Changes the button as toggled or not depending on the given state
     */
    public void UpdateButtonState(GameMods state)
    {
        GetComponent<Toggle>().isOn = state.HasFlag(Mod);
    }
}
