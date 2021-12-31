using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/**
 * This script loads all the game data needed globally in the game
 */
public class LoadingManager : MonoBehaviour
{
    public TextAsset KanjiDic;      // The file containing all kanji definitions
    public ProgressBar ProgressBar; // The progressbar to update given the loading progress
    public GameObject TextInfo;     // The game object that shows additional information

    IEnumerator loadingEnumerator;  // the current coroutine to go through

    // Start is called before the first frame update
    void Start()
    {
        CreateNecessaryFolders();
        StartCoroutine("loadData");
    }

    private void CreateNecessaryFolders()
    {
        if(!Directory.Exists("Levels"))
        {
            Directory.CreateDirectory("Levels");
        }
        if(!Directory.Exists("Scores"))
        {
            Directory.CreateDirectory("Scores");
        }
        if(!Directory.Exists("InputMethods"))
        {
            Directory.CreateDirectory("InputMethods");
        }
    }

    // Coroutine to load all data required by the program
    IEnumerator loadData()
    {
        // start with loading the kanjis from the xml
        loadingEnumerator = JapaneseDictionary.CreateKanjiFromXML(KanjiDic);
        object result;
        ProgressBar.MaxValue = 13108; // hard coded through loading all kanjis in kanjidic, won't change unless the textasset file is updated
        while(loadingEnumerator.MoveNext())
        {
            result = loadingEnumerator.Current;
            (string, int) actualResult = ((string, int))result;
            TextInfo.GetComponent<TMPro.TMP_Text>().SetText("Loading Kanji \"" + actualResult.Item1 + "\"");
            ProgressBar.Value = actualResult.Item2;
            yield return 0; // give control back to the program
        }

        // now load method inputs
        loadingEnumerator = InputFileHandler.LoadInputMethodFiles();
        while(loadingEnumerator.MoveNext())
        {
            result = loadingEnumerator.Current;
            (string, int, int) actualResult = ((string, int, int))result;
            TextInfo.GetComponent<TMPro.TMP_Text>().SetText("Loading input method file \"" + actualResult.Item1 + "\"");
            ProgressBar.MaxValue = actualResult.Item3;
            ProgressBar.Value = actualResult.Item2;
            yield return 0; // give control back to the program
        }

        // now load levels
        loadingEnumerator = LevelLoader.LoadLevels();
        while(loadingEnumerator.MoveNext())
        {
            result = loadingEnumerator.Current;
            (int, string, int, int) actualResult = ((int, string, int, int))result;
            // loading meta files
            if(actualResult.Item1 == 0)
            {
                TextInfo.GetComponent<TMPro.TMP_Text>().SetText("Loading level meta file \"" + actualResult.Item2 + "\"");
                ProgressBar.MaxValue = actualResult.Item4;
                ProgressBar.Value = actualResult.Item3;
            }
            // loading normal levels files that don't have a meta
            else if(actualResult.Item1 == 1)
            {
                TextInfo.GetComponent<TMPro.TMP_Text>().SetText("Loading level file \"" + actualResult.Item2 + "\"");
                ProgressBar.MaxValue = actualResult.Item4;
                ProgressBar.Value = actualResult.Item3;
            }
            // generating meta file
            else
            {
                TextInfo.GetComponent<TMPro.TMP_Text>().SetText("Creating meta file for level \"" + actualResult.Item2 + "\"");
                ProgressBar.MaxValue = actualResult.Item4;
                ProgressBar.Value = actualResult.Item3;
            }
            yield return 0; // give control back to the program
        }
        SceneManager.LoadScene("MainMenu");
    }
}
