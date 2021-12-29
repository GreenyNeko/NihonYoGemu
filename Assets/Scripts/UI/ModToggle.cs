using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Handles the mod buttons in the mod menu
 */
public class ModToggle : MonoBehaviour
{
    public GameMods Mod;                   // Which mod is toggled by this button?
    public GameStarter ScriptGameStarter;  // Apply mods to the gamestarter

    /**
     * Called by UI events, toggles the given mod in the gamestarter
     */
    public void OnToggle(bool value)
    {
        if(value)
        {
            ScriptGameStarter.AddMod(Mod);
        }
        else
        {
            ScriptGameStarter.RemoveMod(Mod);
        }
    }

    public void UpdateButtonState()
    {
        GetComponent<Toggle>().isOn = ScriptGameStarter.HasMod(Mod);
    }
}
