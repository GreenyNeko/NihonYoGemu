using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/**
 * This class manages the level metas of the program
 */
public class LevelMeta
{
    // used to find file differences
    public const ushort Version = 3;
    
    string name;                // file name
    string author;              // level author
    float difficulty;           // level difficulty
    uint totalCharacters;       // total characters in the level
    uint totalKanjis;           // total kanjis in the level
    uint longestSentence;       // the longest sentence
    uint sentences;         // how many sentences the level has
    sbyte parserResult;         // the result of the level parsing

    // private constructor, to create levelmetas without information first internally
    private LevelMeta()
    {
        name = "";
        author = "";
        difficulty = 0;
        totalCharacters = 0;
        totalKanjis = 0;
        longestSentence = 0;
        parserResult = 0;
        sentences = 0;
    }

    /**
     * Create a new level meta given necessary parameters
     */
    public LevelMeta(string name, float difficulty, uint totalCharacters, uint totalKanjis, string author, uint longestSentence, uint senteces, sbyte parserResult)
    {
        this.name = name;
        this.difficulty = difficulty;
        this.totalCharacters = totalCharacters;
        this.totalKanjis = totalKanjis;
        this.author = author;
        this.longestSentence = longestSentence;
        this.sentences = sentences;
        this.parserResult = parserResult;
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
    public uint GetTotalLength()
    {
        return totalCharacters;
    }

    /**
     * Returns the amount of kanjis in the level
     */
    public uint GetKanjiCount()
    {
        return totalKanjis;
    }

    /**
     * Returns the length of the longest sentence
     */
    public uint GetLongestSentenceLength()
    {
        return longestSentence;
    }

    /**
     * Return number of sentences
     */
    public uint GetSentenceCount()
    {
        return sentences;
    }

    /**
     * Returns how many kanjis occur per character
     */
    public float GetKanjiRate()
    {
        return (float)totalKanjis / totalCharacters;
    }

    /**
     * How many elements the level requires you to progress
     */
    public uint GetLevelLength()
    {
        return sentences + totalKanjis;
    }

    /**
     * Returns the result of the parsing stored in the meta data
     */
    public int GetParserResult()
    {
        return parserResult;
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
        levelMeta.totalCharacters = (uint)level.GetLevelLength();
        levelMeta.difficulty = level.GetLevelDifficulty();
        levelMeta.totalKanjis = (uint)level.GetKanjiCount();
        levelMeta.author = level.Author;
        levelMeta.longestSentence = (uint)level.GetLongestSentenceLength();
        levelMeta.sentences = (uint)level.GetSentenceCount();
        levelMeta.parserResult = (sbyte)level.ParseLevel();
        // create the file in the system
        using(BinaryWriter binaryWriter = new BinaryWriter(File.Open("Levels/" + levelMeta.name + ".nyl.meta", FileMode.Create)))
        {
            binaryWriter.Write("NYLMv");
            binaryWriter.Write(Version);
            binaryWriter.Write(levelMeta.difficulty);
            binaryWriter.Write(levelMeta.totalCharacters);
            binaryWriter.Write(levelMeta.totalKanjis);
            binaryWriter.Write(levelMeta.longestSentence);
            binaryWriter.Write(levelMeta.sentences);
            binaryWriter.Write(levelMeta.parserResult);
            binaryWriter.Write(levelMeta.author);
        }
        return levelMeta;
    }
}
