using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/**
 * This class manages the level metas of the program
 */
public class LevelMeta
{
    string name;                // file name
    string author;              // level author
    float difficulty;           // level difficulty
    int totalCharacters;        // total characters in the level
    int totalKanjis;            // total kanjis in the level
    int longestSentence;        // the longest sentence

    // private constructor, to create levelmetas without information first internally
    private LevelMeta()
    {
        name = "";
        author = "";
        difficulty = 0;
        totalCharacters = 0;
        totalKanjis = 0;
        longestSentence = 0;
    }

    /**
     * Create a new level meta given necessary parameters
     */
    public LevelMeta(string name, float difficulty, int totalCharacters, int totalKanjis, string author, int longestSentence)
    {
        this.name = name;
        this.difficulty = difficulty;
        this.totalCharacters = totalCharacters;
        this.totalKanjis = totalKanjis;
        this.author = author;
        this.longestSentence = longestSentence;
    }

    /**
     * Returns the level name
     */
    public string GetLevelName()
    {
        return name;
    }

    /**
     * Returns the level author
     */
    public string GetAuthor()
    {
        return author;
    }

    /**
     * Returns the levels difficulty
     */
    public float GetDifficulty()
    {
        return difficulty;
    }

    /**
     * Returns the amount of characters in the level
     */
    public int GetTotalLength()
    {
        return totalCharacters;
    }

    /**
     * Returns the amount of kanjis in the level
     */
    public int GetKanjiCount()
    {
        return totalKanjis;
    }

    /**
     * Returns the length of the longest sentence
     */
    public int GetLongestSentenceLength()
    {
        return longestSentence;
    }

    /**
     * Returns how many kanjis occur per character
     */
    public float GetKanjiRate()
    {
        return (float)totalKanjis / totalCharacters;
    }

    /**
     * Creates a levelmeta given a level
     */
    public static LevelMeta CreateFromLevel(Level level)
    {
        // create meta
        LevelMeta levelMeta = new LevelMeta();
        // pass information from level
        levelMeta.name = level.Name;
        levelMeta.totalCharacters = level.GetLevelLength();
        levelMeta.difficulty = level.GetLevelDifficulty();
        levelMeta.totalKanjis = level.GetKanjiCount();
        levelMeta.author = level.Author;
        levelMeta.longestSentence = level.GetLongestSentenceLength();
        // create the file in the system
        using (StreamWriter streamWriter = new StreamWriter("Levels/" + levelMeta.name + ".nyl.meta"))
        {
            streamWriter.Write("d" + (Mathf.Round(levelMeta.difficulty * 100) / 100).ToString() + "l" + levelMeta.totalCharacters.ToString() + "k" + levelMeta.totalKanjis.ToString());
            streamWriter.Write("s" + levelMeta.longestSentence.ToString() + "a_" + levelMeta.author);
        }
        return levelMeta;
    }
}
