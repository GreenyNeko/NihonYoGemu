using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class/Data structure for levels
 */
public class Level
{
    public enum ScaleType
    {
        /// <summary>Don't scale</summary>
        KEEP,
        /// <summary>Keep aspect ratio, only scale upwards</summary>
        ASPECT_RATIO_UP,
        /// <summary>Keep the aspect ratio while scaling it</summary>
        ASPECT_RATIO,
        /// <summary>Only scale upwards</summary>
        SCALE_UP,
        /// <summary>Scale completely freely</summary>
        SCALE,
    }

    public class SentenceData
    {
        public Rect rect;
        public string text;
        public string[] furigana;
        public float textSize;
        public float outlineSize;
        // store as aaaavcbx
        // lefttop: 000, centertop: 001, righttop: 010, leftcenter: 011, centercenter: 100, rightcenter: 101, leftbot: 110, centerbot: 111, rightbot: 1000, ...
        public byte alignment;
        public bool vertical;
        public bool color;
        public bool bold;
    }

    public class PageData
    {
        public byte[] backgrounImageData;
        public byte scaleMode;
        public List<SentenceData> sentenceObjects = new List<SentenceData>();
    }

    public List<PageData> pages = new List<PageData>();

    public string FileName;         // file name
    public string LevelName;        // level name
    public string Author;           // who is entered as levle creator
    public int nativeX, nativeY;    // used for scaling purposes
    public int inputOffsetX;        // offset for the input field
    public int inputOffsetY;        // offset for the input field
    public byte scaleMode;          // used for scaling purposes
    float kanjiDifficulty;          // the difficulty of the kanjis
    float levelDifficulty;          // the determines level difficulty
    int kanjiCount;                 // how many kanjis there are
    int totalCharacters;            // how many characters there are in the sentences
    int mostKanjiPerSentence;       // the most kanjis to be expected in any sentence
    int longestSentence;            // length of the longest sentence
    int sentenceCount;              // number of sentences in the level

    /**
     * Constructor creating a level object given the name of the level
     */
    public Level(string fileName, string levelName, string author)
    {
        FileName = fileName;
        LevelName = levelName;
        Author = author;
    }

    /**
     * Returns the amount of sentences the level has
     */
    public int GetSentenceCount()
    {
        return sentenceCount;
    }

    /**
     * Derives different stats about the level which is needed to work with level information.
     * This includes level difficulty, kanji count, total characters, most kanji per sentence, etc.
     */
    public void DeriveStats()
    {
        // initialize local variables
        float totalKanjiDifficulty = 0;
        int totalKanjiCount = 0;
        int otherCharacters = 0;
        sentenceCount = 0;
        // go through all sentences to analyze them
        for(int i = 0; i < pages.Count; i++)
        {
            PageData pageData = pages[i];
            sentenceCount += pageData.sentenceObjects.Count;
            for(int j = 0; j < pageData.sentenceObjects.Count; j++)
            {
                SentenceData sentenceData = pageData.sentenceObjects[j];
                // store sentence length if it has been the longest so far
                if (sentenceData.text.Length > longestSentence)
                {
                    longestSentence = sentenceData.text.Length;
                }
                // analyze kanjis
                string kanjis = "";
                // count characters in this sentence
                for (int k = 0; k < sentenceData.text.Length; k++)
                {
                    // is this a kanji?
                    if (JapaneseDictionary.IsKanji(sentenceData.text[k].ToString()))
                    {
                        // add to the difficulty
                        totalKanjiDifficulty += JapaneseDictionary.GetKanjiDifficulty(sentenceData.text[k].ToString());
                        // store the kanji character
                        kanjis += sentenceData.text[k];
                        // increment counts
                        totalKanjiCount++;
                    }
                    else
                    {
                        otherCharacters++;
                    }
                }
                // update the most kanjis per sentence if we found a new record
                if (kanjis.Length > mostKanjiPerSentence)
                {
                    mostKanjiPerSentence = kanjis.Length;
                }
            }
            
        }
        kanjiCount = totalKanjiCount;
        // derive final stats from analysis
        kanjiDifficulty = (kanjiCount == 0) ? 0 : totalKanjiDifficulty / kanjiCount;        // average kanji difficulty
        kanjiCount = totalKanjiCount;                                                       // number of kanjis in file
        totalCharacters = otherCharacters + kanjiCount;                                     // total amount of characters
        float kanjiRate = (totalCharacters == 0) ? 0 : (float)kanjiCount / totalCharacters; // how often do kanjis occur?
        levelDifficulty = kanjiDifficulty * (1 + (kanjiRate-0.5f)*2);                       // calculate the levels difficulty
    }

    /**
     * Parses the given level and returns a result code
     * 0 - no errors
     * 1 - sentence too long
     * 2 - Furigana and kanji mismatch
     */
    public int ParseLevel()
    {
        /*int furiganas = furigana.Count;
        for(int i = 0; i < sentenceCount; i++)
        {
            //furiganas -= kanjiPerSentence[i].Length;
            // we have more kanjis appear than there are furigana solutions
        }
        if (furiganas < kanjiCount)
        {
            return 2;
        }*/
        // no errors occured
        return 0;
    }

    /**
     * Returns how many kanjis are in the level
     */
    public int GetKanjiCount()
    {
        return kanjiCount;
    }

    /**
     * Returns most kanjis per sentence over all sentences
     */
    public int GetMostKanjisPerSentence()
    {
        return mostKanjiPerSentence;
    }

    /**
     * Returns the levels difficulty
     */
    public float GetLevelDifficulty()
    {
        return levelDifficulty;
    }

    /**
     * Gets the amount of characters in the level
     */
    public int GetLevelLength()
    {
        return totalCharacters;
    }

    /**
     * returns the length of the longest sentece
     */
    public int GetLongestSentenceLength()
    {
        return longestSentence;
    }
}
