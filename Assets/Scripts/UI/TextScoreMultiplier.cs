using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Specific script to update the score multiplier that is displayed given the active mods
 */
public class TextScoreMultiplier : MonoBehaviour
{
    public TMPro.TMP_Text TMP_TextScoreMultiplier;

    /**
     * When called updates the text to display the correct multiplier
     */
    public void UpdateScoreMultiplier(float mult)
    {
        TMP_TextScoreMultiplier.SetText("Score Multiplier: x" + (Mathf.Round(mult * 100) / 100).ToString());
    }
}
