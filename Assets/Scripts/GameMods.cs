using System;

/**
 * Flag enum to handle the level's mod state
 */
[UnityEngine.SerializeField]
[Flags]
public enum GameMods
{
    None = 0,       // default
    Furigana = 1,   // kanji is given with furigana
}

public static class GameModsUtility
{
    public static float GetModMultiplier(this GameMods mods)
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
}
