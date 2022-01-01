using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class/Data structure for levels
 */
public class Level
{
    public string Name;             // file name
    public string Author;           // who is entered as levle creator
    List<string> sentences;         // all the sentence content
    List<string> furigana;          // all the furigana content
    List<string> kanjiPerSentence;  // the kanjis that are in each sentence
    float kanjiDifficulty;          // the difficulty of the kanjis
    float levelDifficulty;          // the determines level difficulty
    int kanjiCount;                 // how many kanjis there are
    int totalCharacters;            // how many characters there are in the sentences
    int mostKanjiPerSentence;       // the most kanjis to be expected in any sentence
    int longestSentence;            // length of the longest sentence

    /**
     * Constructor creating a level object given the name of the level
     */
    public Level(string name)
    {
        this.Name = name;
        sentences = new List<string>();
        furigana = new List<string>();
        kanjiPerSentence = new List<string>();
    }

    /**
     * Adds a new sentence to the level
     */
    public void AddLine(string line)
    {
        // adds an sentence and the solutions for kanji reading given the line
        bool hasSolutions = line.Contains(",");
        if(hasSolutions)
        {
            sentences.Add(line.Substring(0, line.IndexOf(',')));
            furigana.AddRange(line.Substring(line.IndexOf(',')+1).Split(','));
        }
        else
        {
            sentences.Add(line);
        }
    }

    /**
     * Gets the xth sentence from the level
     */
    public string GetLine(int line)
    {
        return sentences[line];
    }

    /**
     * Gets a string of kanjis in the xth sentence
     */
    public string GetKanjiFromSentence(int sentence)
    {
        return kanjiPerSentence[sentence];
    }

    /**
     * Gets the furigana for the xth kanji in the level
     */
    public string GetFuriganaFromKanji(int kanji)
    {
        return furigana[kanji];
    }

    /**
     * Returns the amount of sentences the level has
     */
    public int GetSentenceCount()
    {
        return sentences.Count;
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
        // go through all sentences to analyze them
        foreach(string sentence in sentences)
        {
            // store sentence length if it has been the longest so far
            if(sentence.Length > longestSentence)
            {
                longestSentence = sentence.Length;
            }
            // analyze kanjis
            string kanjis = "";
            // count characters in this sentence
            for(int i = 0; i < sentence.Length; i++)
            {
                // is this a kanji?
                if(JapaneseDictionary.IsKanji(sentence[i].ToString()))
                {
                    // add to the difficulty
                    totalKanjiDifficulty += JapaneseDictionary.GetKanjiDifficulty(sentence[i].ToString());
                    // store the kanji character
                    kanjis += sentence[i];
                    // increment counts
                    totalKanjiCount++;
                }
                else
                {
                    otherCharacters++;
                }
            }
            // update the most kanjis per sentence if we found a new record
            if(kanjis.Length > mostKanjiPerSentence)
            {
                mostKanjiPerSentence = kanjis.Length;
            }
            // store the kanjjis
            kanjiPerSentence.Add(kanjis);
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
        // the sentences are too long to be displayed on all supported screens
        if(longestSentence > 105)
        {
            return 1;
        }
        int furiganas = furigana.Count;
        for(int i = 0; i < sentences.Count; i++)
        {
            //furiganas -= kanjiPerSentence[i].Length;
            // we have more kanjis appear than there are furigana solutions
        }
        if (furiganas < kanjiCount)
        {
            return 2;
        }
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
