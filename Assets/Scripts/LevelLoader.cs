using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/**
 * This script stores level metas internally
 */
public class LevelLoader : MonoBehaviour
{
    static List<LevelMeta> levelMetas;  // contains all loaded level metas

    /**
     * Returns how many level metas have been loaded.
     */
    public static int GetLevelMetaCount()
    {
        return levelMetas.Count;
    }

    /**
     * Returns the level meta from the array given the index
     */
    public static LevelMeta GetLevelMetaByIdx(int idx)
    {
        return levelMetas[idx];
    }

    /**
     * Returns the level meta given the filename
     */
    public static LevelMeta GetLevelMetaByFileName(string fileName)
    {
        return levelMetas.Find(meta => meta.GetFileName() == fileName);
    }

    /**
     * Loads a level given the name
     * return null on error
     */
    public static Level LoadLevelByName(string name)
    {
        /*
        sing (BinaryWriter streamWriter = new BinaryWriter(new FileStream("Levels/" + levelName + ".nyl", FileMode.CreateNew)))
        {
            string version
            string levelname
            string author
            int nativeX
            int nativeY
            int scaleMode
            int pages

            Page:
            int backgroundImageDataLength;
            Byte[] backgroundImageData
            int scaleMode;
            int sentenceObjects;

            SentenceData:
            float x
            float y
            float width
            float height
            string text;
            float textSize;
            float outlineSize;
            bool color;
            bool vertical;
            string[] furigana;
        }
         */
        // load level
        Level level = null;
        using (BinaryReader streamReader = new BinaryReader(new FileStream("Levels/" + name + ".nyl", FileMode.Open)))
        {
            string version = streamReader.ReadString();                                     // version
            if (version == "NYLv2")
            {
                level = LoadLevelVersion2(streamReader, name);
            }
            else
            {
                Debug.LogWarning("Version mismatch");
                return level;
            }
        }
        return level;
    }

    /**
     * Reload level given the name
     */
    public static void ReloadLevelByName(string name)
    {
        Level level = LoadLevelByName(name);
        // update meta
        for(int i = 0; i < levelMetas.Count; i++)
        {
            if(levelMetas[i].GetFileName() == name)
            {
                levelMetas[i] = LevelMeta.CreateFromLevel(level);
            }
        }
    }

    /**
     * <summary>This reloads all levels that have a parsing error or are missing metas, creates them  and returns the progress with the following schema:
     * (state, file name, amount of loaded files, total files to load)
     * The state is 0, if it's loading a meta file, 1 if loading a level and 2 if generating a meta file.</summary>
     */
    public static IEnumerator SoftReloadLevels()
    {
        List<Level> levels = new List<Level>();
        // where to search
        DirectoryInfo directoryInfo = new DirectoryInfo("Levels/");
        int files = directoryInfo.GetFiles("*.nyl.meta").Length;
        int count = 0;
        // load meta
        foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.nyl.meta"))
        {
            string name = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
            // check if this meta has been loaded already
            LevelMeta levelMeta = levelMetas.Find((m) => { return m.GetFileName() == name; });
            if (levelMeta != null)
            {
                // check if the loaded meta has an error
                if(levelMeta.GetParserResult() == 0)
                {
                    // don't need to relaod this one
                    continue;
                }
                else
                {
                    // remove this meta we will readd it
                    levelMetas.Remove(levelMeta);
                    // skip reading this meta to cause the program to create a new one
                    continue;
                }
            }
            bool success = true;
            try
            {
                using (BinaryReader binaryReader = new BinaryReader(File.Open("Levels/" + fileInfo.Name, FileMode.Open)))
                {
                    // load data from file
                    string startStr = binaryReader.ReadString();         // file string
                    // check meta tag
                    if (!startStr.StartsWith("NYLM"))
                    {
                        // file corrupt or not a valid nihonyolevel meta
                        // skip this file
                        continue;
                    }
                    ushort version = binaryReader.ReadUInt16();         // version
                    if (version != LevelMeta.Version)
                    {
                        // skip this file if the version mismatches with the current
                        // this forces a creation of a new meta file
                        continue;
                    }
                    string levelName = binaryReader.ReadString();                               // level name
                    string author = binaryReader.ReadString();                                  // author
                    float difficulty = binaryReader.ReadSingle();                               // difficulty
                    uint levelLength = binaryReader.ReadUInt32();                               // totalCharacters
                    uint kanjiCount = binaryReader.ReadUInt32();                                // total kanjis
                    uint sentenceLength = binaryReader.ReadUInt32();                            // longest sentence length
                    uint sentences = binaryReader.ReadUInt32();                                 // # sentences
                    sbyte parserResult = binaryReader.ReadSByte();                              // parser result
                    Level.PageData firstPage = new Level.PageData();
                    int backgroundDataSize = binaryReader.ReadInt32();                          // background image data size
                    if (backgroundDataSize == 0)
                    {
                        binaryReader.ReadBytes(1);
                        firstPage.backgrounImageData = null;
                    }
                    else
                    {
                        firstPage.backgrounImageData = binaryReader.ReadBytes(backgroundDataSize); // backgroundImageData
                    }
                    int sentenceObjects = binaryReader.ReadInt32();                             // sentence object count
                    firstPage.sentenceObjects = new List<Level.SentenceData>();
                    for(int i = 0; i < sentenceObjects; i++)
                    {
                        Level.SentenceData sentenceData = new Level.SentenceData();
                        float x = binaryReader.ReadSingle();
                        float y = binaryReader.ReadSingle();
                        float width = binaryReader.ReadSingle();
                        float height = binaryReader.ReadSingle();
                        sentenceData.rect = new Rect(x, y, width, height);
                        sentenceData.text = binaryReader.ReadString();
                        sentenceData.textSize = binaryReader.ReadInt32();
                        sentenceData.color = binaryReader.ReadBoolean();
                        sentenceData.vertical = binaryReader.ReadBoolean();
                        int furiganas = binaryReader.ReadInt32();
                        sentenceData.furigana = new string[furiganas];
                        for(int j = 0; j < furiganas; j++)
                        {
                            sentenceData.furigana[j] = binaryReader.ReadString();
                        }
                        firstPage.sentenceObjects.Add(sentenceData);
                    }
                    levelMeta = new LevelMeta(fileInfo.Name, levelName, difficulty, levelLength, kanjiCount, author, sentenceLength, sentences, parserResult, firstPage);
                    Debug.Log("addMeta " + levelMeta.GetFileName());
                    levelMetas.Add(levelMeta);
                }
            }
            catch (EndOfStreamException)
            {
                Debug.LogWarning("Unable to read meta file. skipping");
                success = false;
            }
            if (success)
            {
                // return progress
                yield return (0, fileInfo.Name, ++count, files);
            }
            else
            {
                // return progress
                yield return (0, fileInfo.Name + " FAILED!", ++count, files);
            }
        }
        files = directoryInfo.GetFiles("*.nyl").Length;
        count = 0;
        // load levels without meta and create meta
        foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.nyl"))
        {
            string fileName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
            // if level and meta file don't exist already
            if (!levels.Exists((level) => { return level.FileName == fileName; }) && !levelMetas.Exists((levelMeta) => { return levelMeta.GetFileName() == fileName; }))
            {
                // create a new level structure
                Level level = LoadLevelByName(fileName);
                // return progress
                yield return (1, fileInfo.Name, ++count, files);
                // create meta file
                {
                    level.DeriveStats();
                    LevelMeta levelMeta = LevelMeta.CreateFromLevel(level);
                    levelMetas.Add(levelMeta);
                    levels.Add(level);
                }
                // return progress
                yield return (2, fileInfo.Name, ++count, files);
            }
        }
    }

    /**
     * <summary>Loads levels of version 2, this allows the loading functions to be changed later down the line.</summary>
     */
    public static Level LoadLevelVersion2(BinaryReader binaryReader, string name)
    {
        string debugData = "";
        Level level;
        string levelName = binaryReader.ReadString();                                   // levelName
        debugData += levelName;
        string author = binaryReader.ReadString();                                      // author
        debugData += author;
        level = new Level(name, levelName, author);
        level.nativeX = binaryReader.ReadInt32();                                       // native x
        debugData += level.nativeX.ToString();
        level.nativeY = binaryReader.ReadInt32();                                       // native y
        debugData += level.nativeY.ToString();
        level.scaleMode = binaryReader.ReadByte();                                      // scale mode
        debugData += level.scaleMode.ToString();
        level.inputOffsetX = binaryReader.ReadInt32();                                // input field offset x
        debugData += level.nativeX.ToString();
        level.inputOffsetY = binaryReader.ReadInt32();                                // input field offset y
        debugData += level.nativeY.ToString();
        int pages = binaryReader.ReadInt32();                                           // #pages
        debugData += pages.ToString();
        Debug.Log("pages");
        for (int i = 0; i < pages; i++)
        {
            // read page
            Level.PageData pageData = new Level.PageData();
            int backgroundDataLength = binaryReader.ReadInt32();                        // backgroundImageDataLength
            debugData += backgroundDataLength.ToString();
            if (backgroundDataLength == 0)
            {
                debugData += binaryReader.ReadBytes(1)[0].ToString();
                pageData.backgrounImageData = null;
            }
            else
            {
                pageData.backgrounImageData = binaryReader.ReadBytes(backgroundDataLength); // backgroundImageData
                debugData += pageData.backgrounImageData.ToString();
            }
            pageData.scaleMode = binaryReader.ReadByte();                              // background scale mode
            debugData += pageData.scaleMode.ToString();
            pageData.sentenceObjects = new List<Level.SentenceData>();
            int sentenceObjects = binaryReader.ReadInt32();                             // #sentenceobjects
            debugData += sentenceObjects.ToString();
            Debug.Log("sentences");
            for (int j = 0; j < sentenceObjects; j++)
            {
                Level.SentenceData sentenceData = new Level.SentenceData();
                float x = binaryReader.ReadSingle();                                    // read rect x
                debugData += x.ToString();
                float y = binaryReader.ReadSingle();                                    // read rect y
                debugData += y.ToString();
                float width = binaryReader.ReadSingle();                                // read rect width
                debugData += width.ToString();
                float height = binaryReader.ReadSingle();                               // read rect height
                debugData += height.ToString();
                sentenceData.rect = new Rect(x, y, width, height);
                sentenceData.text = binaryReader.ReadString();                          // sentence text
                debugData += sentenceData.text;
                sentenceData.textSize = binaryReader.ReadSingle();                      // text size
                debugData += sentenceData.textSize.ToString();
                sentenceData.outlineSize = binaryReader.ReadSingle();                   // outline size
                debugData += sentenceData.outlineSize.ToString();
                // packed byte, alignment (4), bold (1), v(1), color(1), unused(1)
                byte abvc = binaryReader.ReadByte();
                sentenceData.alignment = (byte)(abvc >> 4);
                sentenceData.bold = (abvc & 8) == 8;
                sentenceData.vertical = (abvc & 4) == 4;
                sentenceData.color = (abvc & 2) == 2;
                debugData += sentenceData.alignment.ToString();
                debugData += sentenceData.bold.ToString();
                debugData += sentenceData.color.ToString();
                debugData += sentenceData.vertical.ToString();
                int furiganas = binaryReader.ReadInt32();                               // furiganas
                debugData += furiganas.ToString();
                sentenceData.furigana = new string[furiganas];
                for (int k = 0; k < furiganas; k++)
                {
                    sentenceData.furigana[k] = binaryReader.ReadString();               // furigana
                    debugData += sentenceData.furigana[k];
                }
                pageData.sentenceObjects.Add(sentenceData);
            }
            level.pages.Add(pageData);
        }
        Debug.Log(debugData);
        // derive stats
        level.DeriveStats();
        // invalid level
        if (level.ParseLevel() != 0)
        {
            return null;
        }
        return level;
    }

    /**
     * This reloads all levels creating new metas and returns the progress with the following schema:
     * (state, file name, amount of loaded files, total files to load)
     * The state 1 if loading a level and 2 if generating a meta file.
     */
    public static IEnumerator HardReloadLevels()
    {
        List<Level> levels = new List<Level>();
        // clear all metas (why? because we're gonna reload everything anyways)
        levelMetas.Clear();
        // where to search
        DirectoryInfo directoryInfo = new DirectoryInfo("Levels/");
        int files = directoryInfo.GetFiles("*.nyl").Length;
        int count = 0;
        // load levels without meta and create meta
        foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.nyl"))
        {
            string fileName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
            // if level doesn't exist already
            if (!levels.Exists((level) => { return level.FileName == fileName; }))
            {
                // create a new level structure
                Level level = LoadLevelByName(fileName);
                // return progress
                yield return (1, fileInfo.Name, ++count, files);
                // create meta file
                {
                    level.DeriveStats();
                    LevelMeta levelMeta = LevelMeta.CreateFromLevel(level);
                    levelMetas.Add(levelMeta);
                    levels.Add(level);
                }
                // return progress
                yield return (2, fileInfo.Name, ++count, files);
            }
        }
    }

    /**
     * This coroutine loads all the meta files and returns the progress with the following schema:
     * (state, file name, amount of loaded files, total files to load)
     * The state is 0, if it's loading a meta file, 1 if loading a level and 2 if generating a meta file.
     */
    public static IEnumerator LoadLevels()
    {
        List<Level> levels = new List<Level>();
        levelMetas = new List<LevelMeta>();
        // where to search
        DirectoryInfo directoryInfo = new DirectoryInfo("Levels/");
        int files = directoryInfo.GetFiles("*.nyl.meta").Length;
        int count = 0;
        // load meta
        foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.nyl.meta"))
        {
            string name = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
            bool success = true;
            try
            {
                LevelMeta levelMeta;
                using (BinaryReader binaryReader = new BinaryReader(File.Open("Levels/" + fileInfo.Name, FileMode.Open)))
                {
                    // load data from file
                    string startStr = binaryReader.ReadString();         // file string
                    // check meta tag
                    if (!startStr.StartsWith("NYLM"))
                    {
                        // file corrupt or not a valid nihonyolevel meta
                        // skip this file
                        continue;
                    }
                    ushort version = binaryReader.ReadUInt16();         // version
                    if (version != LevelMeta.Version)
                    {
                        // skip this file if the version mismatches with the current
                        // this forces a creationg of a new meta file
                        continue;
                    }
                    string levelName = binaryReader.ReadString();                               // level name
                    string author = binaryReader.ReadString();                                  // author
                    float difficulty = binaryReader.ReadSingle();                               // difficulty
                    uint levelLength = binaryReader.ReadUInt32();                               // totalCharacters
                    uint kanjiCount = binaryReader.ReadUInt32();                                // total kanjis
                    uint sentenceLength = binaryReader.ReadUInt32();                            // longest sentence length
                    uint sentences = binaryReader.ReadUInt32();                                 // # sentences
                    sbyte parserResult = binaryReader.ReadSByte();                              // parser result
                    Level.PageData firstPage = new Level.PageData();
                    int backgroundDataSize = binaryReader.ReadInt32();                          // background image data size
                    if (backgroundDataSize == 0)
                    {
                        binaryReader.ReadBytes(1);
                        firstPage.backgrounImageData = null;
                    }
                    else
                    {
                        firstPage.backgrounImageData = binaryReader.ReadBytes(backgroundDataSize); // backgroundImageData
                    }
                    int sentenceObjects = binaryReader.ReadInt32();                             // sentence object count
                    firstPage.sentenceObjects = new List<Level.SentenceData>();
                    for (int i = 0; i < sentenceObjects; i++)
                    {
                        Level.SentenceData sentenceData = new Level.SentenceData();
                        float x = binaryReader.ReadSingle();
                        float y = binaryReader.ReadSingle();
                        float width = binaryReader.ReadSingle();
                        float height = binaryReader.ReadSingle();
                        sentenceData.rect = new Rect(x, y, width, height);
                        sentenceData.text = binaryReader.ReadString();
                        sentenceData.textSize = binaryReader.ReadInt32();
                        sentenceData.color = binaryReader.ReadBoolean();
                        sentenceData.vertical = binaryReader.ReadBoolean();
                        int furiganas = binaryReader.ReadInt32();
                        sentenceData.furigana = new string[furiganas];
                        for (int j = 0; j < furiganas; j++)
                        {
                            sentenceData.furigana[j] = binaryReader.ReadString();
                        }
                        firstPage.sentenceObjects.Add(sentenceData);
                    }
                    levelMeta = new LevelMeta(name, levelName, difficulty, levelLength, kanjiCount, author, sentenceLength, sentences, parserResult, firstPage);
                    levelMetas.Add(levelMeta);
                }
            }
            catch(EndOfStreamException)
            {
                Debug.LogWarning("Unabel to read meta file. skipping");
                success = false;
            }
            if(success)
            {
                // return progress
                yield return (0, fileInfo.Name, ++count, files);
            }
            else
            {
                // return progress
                yield return (0, fileInfo.Name + " FAILED!", ++count, files);
            }

        }
        files = directoryInfo.GetFiles("*.nyl").Length;
        count = 0;
        // load levels without meta and create meta
        foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.nyl"))
        {
            string fileName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
            Debug.Log(fileName);
            // if level and meta file don't exist already
            if (!levels.Exists((level) => { return level.FileName == fileName; }) && !levelMetas.Exists((levelMeta) => { return levelMeta.GetFileName() == fileName; }))
            {
                // create a new level structure
                Level level = LoadLevelByName(fileName);
                // return progress
                yield return (1, fileInfo.Name, ++count, files);
                // create meta file
                {
                    level.DeriveStats();
                    LevelMeta levelMeta = LevelMeta.CreateFromLevel(level);
                    levelMetas.Add(levelMeta);
                    levels.Add(level);
                }
                // return progress
                yield return (2, fileInfo.Name, ++count, files);
            }
        }
    }
}
