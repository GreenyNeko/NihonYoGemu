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
     * Loads a level given the name
     * return null on error
     */
    public static Level LoadLevelByName(string name)
    {
        Level level = null;
        using (StreamReader streamReader = new StreamReader("Levels/" + name + ".nyl"))
        {
            level = new Level(name);
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                if (line.StartsWith("["))
                {
                    if (line.Substring(1, line.IndexOf('=') - 1) == "author")
                    {
                        level.Author = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 2);
                    }
                }
                else
                {
                    level.AddLine(line);
                }
            }
        }
        level.DeriveStats();
        // invalid level
        if(level.GetLongestSentenceLength() > 105)
        {
            return null;
        }
        return level;
    }

    /**
     * Reload level given the name
     */
    public static void ReloadLevelByName(string name)
    {
        Level level = null;
        using (StreamReader streamReader = new StreamReader("Levels/" + name + ".nyl"))
        {
            level = new Level(name);
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                if (line.StartsWith("["))
                {
                    if (line.Substring(1, line.IndexOf('=') - 1) == "author")
                    {
                        level.Author = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 2);
                    }
                }
                else
                {
                    level.AddLine(line);
                }
            }
        }
        level.DeriveStats();
        // update meta
        for(int i = 0; i < levelMetas.Count; i++)
        {
            if(levelMetas[i].GetLevelName() == name)
            {
                levelMetas[i] = LevelMeta.CreateFromLevel(level);
            }
        }
    }

    /**
     * This reloads all levels that have a parsing error or are missing metas, creates them  and returns the progress with the following schema:
     * (state, file name, amount of loaded files, total files to load)
     * The state is 0, if it's loading a meta file, 1 if loading a level and 2 if generating a meta file.
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
            LevelMeta levelMeta = levelMetas.Find((m) => { return m.GetLevelName() == name; });
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
            using (StreamReader streamReader = new StreamReader("Levels/" + fileInfo.Name))
            {
                // load data from file
                string metaData = streamReader.ReadToEnd();
                // check meta tag
                if (!metaData.StartsWith("NYLM"))
                {
                    // file corrupt or not a valid nihonyolevel meta
                    // skip this file
                    continue;
                }

                if (metaData.Substring(metaData.IndexOf('v') + 1, 1) != LevelMeta.Version)
                {
                    // skip this file if the version mismatches with the current
                    // this forces a creationg of a new meta file
                    continue;
                }
                float difficulty = float.Parse(metaData.Substring(metaData.IndexOf('d') + 1, metaData.IndexOf('l') - metaData.IndexOf('d') - 1));
                string author = metaData.Substring(metaData.IndexOf("a_") + 2);
                int levelLength = int.Parse(metaData.Substring(metaData.IndexOf('l') + 1, metaData.IndexOf('k') - metaData.IndexOf('l') - 1));
                int kanjiCount = int.Parse(metaData.Substring(metaData.IndexOf('k') + 1, metaData.IndexOf('s') - metaData.IndexOf('k') - 1));
                int sentenceLength = int.Parse(metaData.Substring(metaData.IndexOf('s') + 1, metaData.IndexOf('p') - metaData.IndexOf('s') - 1));
                int parserResult = int.Parse(metaData.Substring(metaData.IndexOf('p') + 1, metaData.IndexOf('a') - metaData.IndexOf('p') - 1));
                levelMeta = new LevelMeta(name, difficulty, levelLength, kanjiCount, author, sentenceLength, parserResult);
                levelMetas.Add(levelMeta);
            }
            // return progress
            yield return (0, fileInfo.Name, ++count, files);
        }
        files = directoryInfo.GetFiles("*.nyl").Length;
        count = 0;
        // load levels without meta and create meta
        foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.nyl"))
        {
            string fileName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
            // if level and meta file don't exist already
            if (!levels.Exists((level) => { return level.Name == fileName; }) && !levelMetas.Exists((levelMeta) => { return levelMeta.GetLevelName() == fileName; }))
            {
                // create a new level structure
                Level level = new Level(fileName);
                using (StreamReader streamReader = new StreamReader("Levels/" + fileInfo.Name))
                {
                    // read all sentences
                    while (!streamReader.EndOfStream)
                    {
                        string line = streamReader.ReadLine();
                        // handle comments
                        if (line.StartsWith("#"))
                        {
                            continue;
                        }
                        // handle attribute tags
                        if (line.StartsWith("["))
                        {
                            if (line.Substring(1, line.IndexOf('=') - 1) == "author")
                            {
                                level.Author = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 2);
                            }

                        }
                        else
                        {
                            // load line as sentence
                            level.AddLine(line);
                        }
                    }
                }
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
            if (!levels.Exists((level) => { return level.Name == fileName; }))
            {
                // create a new level structure
                Level level = new Level(fileName);
                using (StreamReader streamReader = new StreamReader("Levels/" + fileInfo.Name))
                {
                    // read all sentences
                    while (!streamReader.EndOfStream)
                    {
                        string line = streamReader.ReadLine();
                        // handle comments
                        if (line.StartsWith("#"))
                        {
                            continue;
                        }
                        // handle attribute tags
                        if (line.StartsWith("["))
                        {
                            if (line.Substring(1, line.IndexOf('=') - 1) == "author")
                            {
                                level.Author = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 2);
                            }

                        }
                        else
                        {
                            // load line as sentence
                            level.AddLine(line);
                        }
                    }
                }
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
            using (StreamReader streamReader = new StreamReader("Levels/" + fileInfo.Name))
            {
                // load data from file
                string metaData = streamReader.ReadToEnd();
                // check meta tag
                if(!metaData.StartsWith("NYLM"))
                {
                    // file corrupt or not a valid nihonyolevel meta
                    // skip this file
                    continue;
                }
                
                if(metaData.Substring(metaData.IndexOf('v') + 1, 1) != LevelMeta.Version)
                {
                    // skip this file if the version mismatches with the current
                    // this forces a creationg of a new meta file
                    continue;
                }
                float difficulty = float.Parse(metaData.Substring(metaData.IndexOf('d') + 1, metaData.IndexOf('l') - metaData.IndexOf('d') - 1));
                string author = metaData.Substring(metaData.IndexOf("a_") + 2);
                int levelLength = int.Parse(metaData.Substring(metaData.IndexOf('l') + 1, metaData.IndexOf('k') - metaData.IndexOf('l') - 1));
                int kanjiCount = int.Parse(metaData.Substring(metaData.IndexOf('k') + 1, metaData.IndexOf('s') - metaData.IndexOf('k') - 1));
                int sentenceLength = int.Parse(metaData.Substring(metaData.IndexOf('s') + 1, metaData.IndexOf('p') - metaData.IndexOf('s') - 1));
                int parserResult = int.Parse(metaData.Substring(metaData.IndexOf('p') + 1, metaData.IndexOf('a') - metaData.IndexOf('p') - 1));
                LevelMeta levelMeta = new LevelMeta(name, difficulty, levelLength, kanjiCount, author, sentenceLength, parserResult);
                levelMetas.Add(levelMeta);
            }
            // return progress
            yield return (0, fileInfo.Name, ++count, files);
        }
        files = directoryInfo.GetFiles("*.nyl").Length;
        count = 0;
        // load levels without meta and create meta
        foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.nyl"))
        {
            string fileName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
            // if level and meta file don't exist already
            if (!levels.Exists((level) => { return level.Name == fileName; }) && !levelMetas.Exists((levelMeta) => { return levelMeta.GetLevelName() == fileName; }))
            {
                // create a new level structure
                Level level = new Level(fileName);
                using (StreamReader streamReader = new StreamReader("Levels/" + fileInfo.Name))
                {
                    // read all sentences
                    while (!streamReader.EndOfStream)
                    {
                        string line = streamReader.ReadLine();
                        // handle comments
                        if(line.StartsWith("#"))
                        {
                            continue;
                        }
                        // handle attribute tags
                        if (line.StartsWith("["))
                        {
                            if (line.Substring(1, line.IndexOf('=') - 1) == "author")
                            {
                                level.Author = line.Substring(line.IndexOf('=') + 1, line.Length - line.IndexOf('=') - 2);
                            }

                        }
                        else
                        {
                            // load line as sentence
                            level.AddLine(line);
                        }
                    }
                }
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
