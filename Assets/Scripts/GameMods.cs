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

