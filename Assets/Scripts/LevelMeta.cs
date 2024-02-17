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
    public const ushort Version = 4;
    
    string fileName;            // file name
    string levelName;           // level name
    string author;              // level author
    float difficulty;           // level difficulty
    uint totalCharacters;       // total characters in the level
    uint totalKanjis;           // total kanjis in the level
    uint longestSentence;       // the longest sentence
    uint sentences;             // how many sentences the level has
    sbyte parserResult;         // the result of the level parsing
    Level.PageData pageData;    // first page of the level

    // private constructor, to create levelmetas without information first internally
    private LevelMeta()
    {
        fileName = "";
        levelName = "";
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
    public LevelMeta(string fileName, string levelName, float difficulty, uint totalCharacters, uint totalKanjis, string author, uint longestSentence, uint sentences, sbyte parserResult, Level.PageData pageData)
    {
        this.fileName = fileName;
        this.levelName = levelName;
        this.difficulty = difficulty;
        this.totalCharacters = totalCharacters;
        this.totalKanjis = totalKanjis;
        this.author = author;
        this.longestSentence = longestSentence;
        this.sentences = sentences;
        this.parserResult = parserResult;
        this.pageData = pageData;
    }

    /**
 * Returns the level name
 */
    public string GetFileName()
    {
        return fileName;
    }

    /**
     * Returns the level name
     */
    public string GetLevelName()
    {
        return levelName;
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
     * Returns the page data of the first page of the level
     */
    public Level.PageData GetPageData()
    {
        return pageData;
    }

    /**
     * Creates a levelmeta given a level
     */
    public static LevelMeta CreateFromLevel(Level level)
    {
        // create meta
        LevelMeta levelMeta = new LevelMeta();
        // pass information from level
        levelMeta.fileName = level.FileName;
        levelMeta.levelName = level.LevelName;
        levelMeta.totalCharacters = (uint)level.GetLevelLength();
        levelMeta.author = level.Author;
        levelMeta.difficulty = level.GetLevelDifficulty();
        levelMeta.totalKanjis = (uint)level.GetKanjiCount();
        levelMeta.longestSentence = (uint)level.GetLongestSentenceLength();
        levelMeta.sentences = (uint)level.GetSentenceCount();
        levelMeta.parserResult = (sbyte)level.ParseLevel();
        levelMeta.pageData = level.pages[0];
        // create the file in the system
        using(BinaryWriter binaryWriter = new BinaryWriter(File.Open("Levels/" + levelMeta.fileName + ".nyl.meta", FileMode.Create)))
        {
            binaryWriter.Write("NYLMv");
            binaryWriter.Write(Version);
            binaryWriter.Write(levelMeta.levelName);
            binaryWriter.Write(levelMeta.author);
            binaryWriter.Write(levelMeta.difficulty);
            binaryWriter.Write(levelMeta.totalCharacters);
            binaryWriter.Write(levelMeta.totalKanjis);
            binaryWriter.Write(levelMeta.longestSentence);
            binaryWriter.Write(levelMeta.sentences);
            binaryWriter.Write(levelMeta.parserResult);
            // write first page
            if(levelMeta.pageData.backgrounImageData == null)
            {
                binaryWriter.Write(0);
                binaryWriter.Write('\0');
            }
            else
            {
                binaryWriter.Write(levelMeta.pageData.backgrounImageData.Length);
                binaryWriter.Write(levelMeta.pageData.backgrounImageData);
            }
            // write sentences
            binaryWriter.Write(levelMeta.pageData.sentenceObjects.Count);
            for (int j = 0; j < levelMeta.pageData.sentenceObjects.Count; j++)
            {
                Level.SentenceData sentenceData = levelMeta.pageData.sentenceObjects[j];
                binaryWriter.Write(sentenceData.rect.x);
                binaryWriter.Write(sentenceData.rect.y);
                binaryWriter.Write(sentenceData.rect.width);
                binaryWriter.Write(sentenceData.rect.height);
                binaryWriter.Write(sentenceData.text);
                binaryWriter.Write(sentenceData.textSize);
                binaryWriter.Write(sentenceData.color);
                binaryWriter.Write(sentenceData.vertical);
                binaryWriter.Write(sentenceData.furigana.Length);
                for (int k = 0; k < sentenceData.furigana.Length; k++)
                {
                    binaryWriter.Write(sentenceData.furigana[k]);
                }
            }
        }
        return levelMeta;
    }
}
