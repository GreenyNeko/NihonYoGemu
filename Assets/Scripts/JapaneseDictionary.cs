using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Xml;
using System.IO;

/**
 * Contains helpful functions and ddefinitions needed
 */
public static class JapaneseDictionary
{
    /*
     * Contains defintions of kanjis, could use a dictionary file instead but this provides faster lookup and cannot be edited by people as easily
     * Competitiveness might play a role in the future. Additionally these defintions will never change
     */
    static List<Kanji> kanjis = new List<Kanji>();

    /*
     * Unlike the kanjis these will be geneerated from a file that determines the romaji
     * Unlike to other kanas romaji does not have a fixed definition and varies.
     */
    static List<Kana> kana = new List<Kana>();

    /**
     * Reads the kanjidic2.xml and creates the kanji entries from it and returns
     * the progress
     */
    public static IEnumerator CreateKanjiFromXML(TextAsset textAsset)
    {
        // TODO: is there a way to speed this up / make it more performant?
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(textAsset.text);
        XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("character");
        float runTime = 0;
        foreach(XmlNode xmlNode in xmlNodeList)
        {
            string character = xmlNode.SelectSingleNode("literal").InnerText;
            XmlNode miscNode = xmlNode.SelectSingleNode("misc");
            XmlNode freqNode = miscNode.SelectSingleNode("freq");
            int ranking = 2502;
            if (freqNode != null)
            {
                ranking = int.Parse(freqNode.InnerText);
            }
            
            XmlNode jlptNode = miscNode.SelectSingleNode("jlpt");
            int jlpt = 6;
            if(jlptNode != null)
            {
                jlpt = int.Parse(jlptNode.InnerText);
            }

            List<string> readings = new List<string>();
            // get onyomi readings
            XmlNodeList readingsNodes = xmlNode.SelectNodes("./reading_meaning/rmgroup/reading[@r_type='ja_on']");
            foreach(XmlNode readingNode in readingsNodes)
            {
                string reading = readingNode.InnerText;
                readings.Add(ConvertKanaToKana(reading));
            }

            // get kunyomi readings
            readingsNodes = xmlNode.SelectNodes("./reading_meaning/rmgroup/reading[@r_type='ja_kun']");
            foreach (XmlNode readingNode in readingsNodes)
            {
                
                string rawReading = readingNode.InnerText;
                if (rawReading.Contains("."))
                {
                    rawReading = rawReading.Substring(0, rawReading.IndexOf("."));
                }
                if (rawReading.Contains("-"))
                {
                    rawReading = rawReading.Replace("-", "");
                }
                readings.Add(rawReading);
            }
            kanjis.Add(new Kanji(character, ranking, jlpt).SetReadings(readings.Distinct<string>().ToArray()));
            runTime += Time.deltaTime;
            // run for at least 0.5 seconds before
            if (runTime > 0.5f)
            {
                runTime %= 0.5f;
                yield return (character, kanjis.Count);
            }
        }
    }

    /**
     * Reads the kanjidef.bin and creates the kanji entries from it and returns
     * the progress
     */
    public static IEnumerator CreateKanjiFromBinary(TextAsset textAsset)
    {
        float runTime = 0;
        using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(textAsset.bytes)))
        {
            while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
            {
                string character = binaryReader.ReadString();
                int jlpt = binaryReader.ReadInt32();
                int ranking = binaryReader.ReadInt32();
                int readingsCount = binaryReader.ReadInt32();
                string[] readings = new string[readingsCount];  
                for(int i = 0; i < readingsCount; i++)
                {
                    readings[i] = binaryReader.ReadString();
                }
                kanjis.Add(new Kanji(character, ranking, jlpt).SetReadings(readings));
                runTime += Time.deltaTime;
                // run for at least 0.5 seconds before
                if (runTime > 0.5f)
                {
                    runTime %= 0.5f;
                    yield return (character, kanjis.Count);
                }
            }
        }
    }

    /**
     * Fills the definition for kanas using an input methods definition file
     */
    public static void CreateKanaFromInputFileId(int fileId)
    {
        kana.Clear();
        Dictionary<string,string> dict = InputFileHandler.GetDefinition(fileId);
        foreach(string key in dict.Keys)
        {
            kana.Add(new Kana(key.ToString(), dict[key], true));
        }
    }

    /**
     * Return the difficulty of the kanji
     */
    public static int GetKanjiDifficulty(string character)
    {
        Kanji soughtKanji = kanjis.Find((kanji) => { return kanji.GetCharacter() == character; });
        return soughtKanji.GetRanking() / 2500 * 4 + (6 - soughtKanji.GetJLPTLevel());
    }

    /**
     * Returns whether or not the given character is a kanji 
     * (used for determining the meta information of a level such as amount of kanjis)
     */
    public static bool IsKanji(string character)
    {
        return kanjis.Exists((kanji) => { return kanji.GetCharacter() == character; });
    }

    /**
     * Checks whether or not the given string is in the readings of the given kanji character.
     */
    public static bool StringInReadings(string str, char kanji)
    {
        // iterate until we get the kanji we want
        for(int i = 0; i < kanjis.Count; i++)
        {
            // match the given kanji with the kanji in the kanji list
            if(kanjis[i].GetCharacter() == kanji.ToString())
            {
                for(int j = 0; j < kanjis[i].GetReadings().Length; j++)
                { 
                    // does the string match the kana?
                    if(str == kanjis[i].GetReadings()[j])
                    {
                        return true;
                    }
                    // does the string converted to hiragana match the hiragana of the kanji?
                    if (ConvertRomajiToKana(str, false) == kanjis[i].GetReadings()[j])
                    {
                        return true;
                    }
                }
                // string not in readings => we're done here
                break;
            }
        }
        return false;
    }

    /**
     * Converts kana to romaji
     */
    public static string ConvertKanaToRomaji(string givenKana)
    {
        // TODO: update this, it used fixed reading but now the readings depend on the input method file
        string newString = "";
        bool doubleConsonant = false;
        for (int i = 0; i < givenKana.Length; i++)
        {
            Kana character = kana.Find((kana) => { return kana.GetCharacter() == givenKana[i].ToString(); });
            
            if (doubleConsonant)
            {
                string reading = character.GetReading();
                newString += reading[0] + reading;
            }
            else if (character.IsHiragana() && (character.GetCharacter() == "ゃ" || character.GetCharacter() == "ょ" || character.GetCharacter() == "ゅ")
                || !character.IsHiragana() && (character.GetCharacter() == "ゃ" || character.GetCharacter() == "ょ" || character.GetCharacter() == "ゅ"))
            {
                newString.Remove(newString.Length - 1);
                newString += character.GetReading();
            }
            else
            {
                string reading = character.GetReading();
                newString += reading[0] + reading;
            }
            if (character.GetReading() == "")
            {
                doubleConsonant = true;
            }
        }
        return newString;
    }

    /**
     * Converts romaji to kana
     */
    public static string ConvertRomajiToKana(string givenRomaji, bool keepRest)
    {
        //string rest = "";
        string result = "";
        // sort kana by length of kana2
        kana.Sort((kana1, kana2) => { return kana1.GetReading().Length.CompareTo(kana2.GetReading().Length); });
        Debug.Log(kana.Count); // why is this sometimes 0!?!?! not on main thread?
        int maxLength = kana[kana.Count - 1].GetReading().Length;
        // go through the romaji
        for (int i = 0; i <= givenRomaji.Length; i++)
        {
            // forward look for small tsu
            if(i+1 < givenRomaji.Length && givenRomaji[i] == givenRomaji[i + 1] && givenRomaji[i] != 'a' && givenRomaji[i] != 'e' && givenRomaji[i] != 'i' && givenRomaji[i] != 'o' && givenRomaji[i] != 'u' && givenRomaji[i] != 'y' && givenRomaji[i] != 'n')
            {
                result += "っ";
                continue;
            }
            bool partOfKana = false;
            // check different sizes
            for(int j = 0; j < kana.Count; j++)
            {
                // check if the romaji matches our kana
                if(i + kana[j].GetReading().Length <= givenRomaji.Length && givenRomaji.Substring(i, kana[j].GetReading().Length) == kana[j].GetReading())
                {
                    // store the character replacement and move to the next character after ours
                    result += kana[j].GetCharacter();
                    partOfKana = true;
                    i += kana[j].GetReading().Length - 1;
                    break;
                }
            }
            if (!partOfKana && keepRest && i < givenRomaji.Length)
            {
                result += givenRomaji[i];
            }
        }

        return result;
    }

    /**
     * Converts a string of hiragana to katakana and converts a string of katakana to hiragana
     */
    public static string ConvertKanaToKana(string kana)
    {
        string newStr = "";
        for(int i = 0; i < kana.Length; i++)
        {
            // literally looks up each katakana given hiragana and vice versa, I recommend colapsing this switch
            switch(kana[i])
            {
                // hiragana
                case 'わ':
                    newStr += 'ワ';
                    break;
                case 'を':
                    newStr += 'ヲ';
                    break;
                case 'ゅ':
                    newStr += 'ュ';
                    break;
                case 'ょ':
                    newStr += 'ョ';
                    break;
                case 'ゃ':
                    newStr += 'ャ';
                    break;
                case 'っ':
                    newStr += 'ッ';
                    break;
                case 'ゆ':
                    newStr += 'ユ';
                    break;
                case 'よ':
                    newStr += 'ヨ';
                    break;
                case 'や':
                    newStr += 'ヤ';
                    break;
                case 'に':
                    newStr += '二';
                    break;
                case 'ね':
                    newStr += 'ネ';
                    break;
                case 'ぬ':
                    newStr += 'ヌ';
                    break;
                case 'の':
                    newStr += 'ノ';
                    break;
                case 'な':
                    newStr += 'ナ';
                    break;
                case 'み':
                    newStr += '三';
                    break;
                case 'め':
                    newStr += 'メ';
                    break;
                case 'む':
                    newStr += 'ム';
                    break;
                case 'も':
                    newStr += 'モ';
                    break;
                case 'ま':
                    newStr += 'マ';
                    break;
                case 'り':
                    newStr += 'リ';
                    break;
                case 'れ':
                    newStr += 'レ';
                    break;
                case 'る':
                    newStr += 'ル';
                    break;
                case 'ろ':
                    newStr += 'ロ';
                    break;
                case 'ら':
                    newStr += 'ラ';
                    break;
                case 'ぢ':
                    newStr += 'ヂ';
                    break;
                case 'で':
                    newStr += 'デ';
                    break;
                case 'づ':
                    newStr += 'ヅ';
                    break;
                case 'ど':
                    newStr += 'ド';
                    break;
                case 'だ':
                    newStr += 'ダ';
                    break;
                case 'ち':
                    newStr += 'チ';
                    break;
                case 'て':
                    newStr += 'テ';
                    break;
                case 'つ':
                    newStr += 'ツ';
                    break;
                case 'と':
                    newStr += 'ト';
                    break;
                case 'た':
                    newStr += 'タ';
                    break;
                case 'じ':
                    newStr += 'ジ';
                    break;
                case 'ぜ':
                    newStr += 'ゼ';
                    break;
                case 'ず':
                    newStr += 'ズ';
                    break;
                case 'ぞ':
                    newStr += 'ゾ';
                    break;
                case 'ざ':
                    newStr += 'ザ';
                    break;
                case 'し':
                    newStr += 'シ';
                    break;
                case 'せ':
                    newStr += 'セ';
                    break;
                case 'す':
                    newStr += 'ス';
                    break;
                case 'そ':
                    newStr += 'ソ';
                    break;
                case 'さ':
                    newStr += 'サ';
                    break;
                case 'ぴ':
                    newStr += 'ピ';
                    break;
                case 'ぺ':
                    newStr += 'ペ';
                    break;
                case 'ぷ':
                    newStr += 'プ';
                    break;
                case 'ぽ':
                    newStr += 'ポ';
                    break;
                case 'ぱ':
                    newStr += 'パ';
                    break;
                case 'び':
                    newStr += 'ビ';
                    break;
                case 'べ':
                    newStr += 'ベ';
                    break;
                case 'ぶ':
                    newStr += 'ブ';
                    break;
                case 'ぼ':
                    newStr += 'ボ';
                    break;
                case 'ば':
                    newStr += 'バ';
                    break;
                case 'ひ':
                    newStr += 'ヒ';
                    break;
                case 'へ':
                    newStr += 'ヘ';
                    break;
                case 'ふ':
                    newStr += 'フ';
                    break;
                case 'ほ':
                    newStr += 'ㇹ';
                    break;
                case 'は':
                    newStr += 'ハ';
                    break;
                case 'ぎ':
                    newStr += 'ギ';
                    break;
                case 'げ':
                    newStr += 'ゲ';
                    break;
                case 'ぐ':
                    newStr += 'グ';
                    break;
                case 'ご':
                    newStr += 'ゴ';
                    break;
                case 'が':
                    newStr += 'ガ';
                    break;
                case 'き':
                    newStr += 'キ';
                    break;
                case 'け':
                    newStr += 'ケ';
                    break;
                case 'く':
                    newStr += 'ク';
                    break;
                case 'こ':
                    newStr += 'コ';
                    break;
                case 'か':
                    newStr += 'カ';
                    break;
                case 'い':
                    newStr += 'イ';
                    break;
                case 'え':
                    newStr += 'エ';
                    break;
                case 'う':
                    newStr += 'ウ';
                    break;
                case 'お':
                    newStr += 'オ';
                    break;
                case 'あ':
                    newStr += 'ア';
                    break;
                // katakana
                case 'ワ':
                    newStr += 'わ';
                    break;
                case 'ヲ':
                    newStr += 'を';
                    break;
                case 'ュ':
                    newStr += 'ゅ';
                    break;
                case 'ョ':
                    newStr += 'ょ';
                    break;
                case 'ャ':
                    newStr += 'ゃ';
                    break;
                case 'ッ':
                    newStr += 'っ';
                    break;
                case 'ユ':
                    newStr += 'ゆ';
                    break;
                case 'ヨ':
                    newStr += 'よ';
                    break;
                case 'ヤ':
                    newStr += 'や';
                    break;
                case '二':
                    newStr += 'に';
                    break;
                case 'ネ':
                    newStr += 'ね';
                    break;
                case 'ヌ':
                    newStr += 'ぬ';
                    break;
                case 'ノ':
                    newStr += 'の';
                    break;
                case 'ナ':
                    newStr += 'な';
                    break;
                case '三':
                    newStr += 'み';
                    break;
                case 'メ':
                    newStr += 'め';
                    break;
                case 'ム':
                    newStr += 'む';
                    break;
                case 'モ':
                    newStr += 'も';
                    break;
                case 'マ':
                    newStr += 'ま';
                    break;
                case 'リ':
                    newStr += 'り';
                    break;
                case 'レ':
                    newStr += 'れ';
                    break;
                case 'ル':
                    newStr += 'る';
                    break;
                case 'ロ':
                    newStr += 'ろ';
                    break;
                case 'ラ':
                    newStr += 'ら';
                    break;
                case 'ヂ':
                    newStr += 'ぢ';
                    break;
                case 'デ':
                    newStr += 'で';
                    break;
                case 'ヅ':
                    newStr += 'づ';
                    break;
                case 'ド':
                    newStr += 'ど';
                    break;
                case 'ダ':
                    newStr += 'だ';
                    break;
                case 'チ':
                    newStr += 'ち';
                    break;
                case 'テ':
                    newStr += 'て';
                    break;
                case 'ツ':
                    newStr += 'つ';
                    break;
                case 'ト':
                    newStr += 'と';
                    break;
                case 'タ':
                    newStr += 'た';
                    break;
                case 'ジ':
                    newStr += 'じ';
                    break;
                case 'ゼ':
                    newStr += 'ぜ';
                    break;
                case 'ズ':
                    newStr += 'ず';
                    break;
                case 'ゾ':
                    newStr += 'ぞ';
                    break;
                case 'ザ':
                    newStr += 'ざ';
                    break;
                case 'シ':
                    newStr += 'し';
                    break;
                case 'セ':
                    newStr += 'せ';
                    break;
                case 'ス':
                    newStr += 'す';
                    break;
                case 'ソ':
                    newStr += 'そ';
                    break;
                case 'サ':
                    newStr += 'さ';
                    break;
                case 'ピ':
                    newStr += 'ぴ';
                    break;
                case 'ペ':
                    newStr += 'ぺ';
                    break;
                case 'プ':
                    newStr += 'ぷ';
                    break;
                case 'ポ':
                    newStr += 'ぽ';
                    break;
                case 'パ':
                    newStr += 'ぱ';
                    break;
                case 'ビ':
                    newStr += 'び';
                    break;
                case 'ベ':
                    newStr += 'べ';
                    break;
                case 'ブ':
                    newStr += 'ぶ';
                    break;
                case 'ボ':
                    newStr += 'ぼ';
                    break;
                case 'バ':
                    newStr += 'ば';
                    break;
                case 'ヒ':
                    newStr += 'ひ';
                    break;
                case 'ヘ':
                    newStr += 'へ';
                    break;
                case 'フ':
                    newStr += 'ふ';
                    break;
                case 'ㇹ':
                    newStr += 'ほ';
                    break;
                case 'ハ':
                    newStr += 'は';
                    break;
                case 'ギ':
                    newStr += 'ぎ';
                    break;
                case 'ゲ':
                    newStr += 'げ';
                    break;
                case 'グ':
                    newStr += 'ぐ';
                    break;
                case 'ゴ':
                    newStr += 'ご';
                    break;
                case 'ガ':
                    newStr += 'が';
                    break;
                case 'キ':
                    newStr += 'き';
                    break;
                case 'ケ':
                    newStr += 'け';
                    break;
                case 'ク':
                    newStr += 'く';
                    break;
                case 'コ':
                    newStr += 'こ';
                    break;
                case 'カ':
                    newStr += 'か';
                    break;
                case 'イ':
                    newStr += 'い';
                    break;
                case 'エ':
                    newStr += 'え';
                    break;
                case 'ウ':
                    newStr += 'う';
                    break;
                case 'オ':
                    newStr += 'お';
                    break;
                case 'ア':
                    newStr += 'あ';
                    break;
                default:
                    newStr += kana[i];
                    break;
            }
        }
        return newStr;
    }

    /**
     * <summary>Returns what kind of kana  the given sentence is.</summary>
     * <returns>0 - no japanese characters are contained
     * 1 - hiragana flag
     * 2 - katakana flag
     * 4 - kanji flag</returns>
     */
    public static int GetKanaType(string sentence)
    {
        int flags = 0;
        foreach (char c in sentence)
        {
            if (JapaneseDictionary.IsKanji(c.ToString()))
            {
                flags |= 4;
            }
            if (kana.Exists(k => { return k.GetCharacter() == c.ToString() && k.IsHiragana(); }))
            {
                flags |= 1;
            }
            if (kana.Exists(k => { return k.GetCharacter() == c.ToString() && !k.IsHiragana(); }))
            {
                flags |= 2;
            }
        }
        return flags;
    }
}

/**
 * Represents a hiragana or katakana character
 */
class Kana
{
    string character;   // the character
    string reading;     // the reading
    bool hiragana;      // whether it's hiragana or katakana

    /**
     * Constructor creating a kana
     */
    public Kana(string character, string reading, bool isHiraga)
    {
        this.character = character;
        this.reading = reading;
        this.hiragana = isHiraga;
    }

    /**
     * Returns the kana character
     */
    public string GetCharacter()
    {
        return character;
    }

    /**
     * Returns the readings
     */
    public string GetReading()
    {
        return reading;
    }

    /**
     * Returns true if hiragana else false
     */
    public bool IsHiragana()
    {
        return hiragana;
    }
}

/**
 * Represents a kanji character
 */
class Kanji
{
    string character;       // the character
    List<string> readings;  // all the ways it can be officially read (without ateji)
    int ranking;            // how often does it occur?
    int jlptLevel;          // what level is it in jlpt, else 6

    /**
     * Create a kanji definition
     */
    public Kanji(string character, int ranking, int jlptLevel)
    {
        this.character = character;
        this.ranking = ranking;
        this.jlptLevel = jlptLevel;
    }

    /**
     * Set the readings for the kanji
     */
    public Kanji SetReadings(string[] readings)
    {
        this.readings = new List<string>(readings);
        return this;
    }

    /**
     * 
     */
    public string[] GetReadings()
    {
        return readings.ToArray();
    }

    /**
     * Get the character
     */
    public string GetCharacter()
    {
        return character;
    }

    /**
     * Get the ranking of this kanji given it's occurence
     */

    public int GetRanking()
    {
        return ranking;
    }

    /**
     * Get the jlpt level of the kanji
     */
    public int GetJLPTLevel()
    {
        return jlptLevel;
    }
}
