using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/**
 * This class loads, stores and manages input method files.
 */
public static class InputFileHandler
{
    static List<string> files = new List<string>(); // a list of all file names that exist

    /**
     * Loads the file names of all input method .cfg files and returns
     * (name of the current file, how many files have been loaded, how many have been found)d
     */
    public static IEnumerator LoadInputMethodFiles()
    {
        // load all input defining files
        DirectoryInfo directoryInfo = new DirectoryInfo("InputMethods/");
        int count = 0;
        int foundFiles = directoryInfo.GetFiles().Length;
        // store their names
        foreach (FileInfo file in directoryInfo.GetFiles("*.cfg"))
        {
            files.Add(file.Name.Substring(0, file.Name.IndexOf(".")));
            yield return (file.Name, ++count, foundFiles);
        }
    }

    /**
     * Returns list of names of loaded files
     */
    public static string[] GetFileNames()
    {
        return files.ToArray();
    }

    /**
     * Returns a dictionary matching kana to romaji given the files content found in the list
     * of existing .cfg files
     */
    public static Dictionary<string, string> GetDefinition(int fileId)
    {
        // create the dictionary
        Dictionary<string, string> dict = new Dictionary<string, string>();
        // read the file
        using (StreamReader streamReader = new StreamReader("InputMethods/" + files[fileId] + ".cfg"))
        {
            while(!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                if(!line.StartsWith("#"))
                {
                    dict.Add(line.Substring(0, line.IndexOf("=")), line.Substring(line.IndexOf("=")+1));
                }
            }
        }
        return dict;
    }
       
}
