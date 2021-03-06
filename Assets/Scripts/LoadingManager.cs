using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Video;
using UnityEditor;

/**
 * This script loads all the game data needed globally in the game
 */
public class LoadingManager : MonoBehaviour
{
    public GameObject licenseScreen;// the screen containing the license
    public GameObject loadingScreen;// the screen with the loading information
    public VideoPlayer videoPlayer; // the video player of the splash
    public TextAsset KanjiDic;      // The xml file containing all kanji definitions
    public TextAsset KanjiDef;   // The binary file containing all kanji definitions
    public ProgressBar ProgressBar; // The progressbar to update given the loading progress
    public GameObject TextInfo;     // The game object that shows additional information

    IEnumerator loadingEnumerator;  // the current coroutine to go through

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.gameObject.SetActive(true);
        licenseScreen.SetActive(false);
        loadingScreen.SetActive(false);
        StartCoroutine("opening");
        CreateNecessaryFolders();
        StartCoroutine("loadData");
    }

    void Update()
    {
        if(licenseScreen.activeInHierarchy)
        {
            if(Input.anyKeyDown)
            {
                licenseScreen.SetActive(false);
                loadingScreen.SetActive(true);
            }
        }
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

    // controlls playing the opening
    IEnumerator opening()
    {
        // waiting for splash to play
        while(!videoPlayer.isPlaying)
        {
            yield return null;
        }
        // waiting for splash to end
        while(videoPlayer.isPlaying)
        {
            yield return null;
        }
        videoPlayer.gameObject.SetActive(false);
        licenseScreen.SetActive(true);
    }

    // Coroutine to load all data required by the program
    IEnumerator loadData()
    {
        // start with loading the kanjis from the xml
        //loadingEnumerator = JapaneseDictionary.CreateKanjiFromXML(KanjiDic);
        loadingEnumerator = JapaneseDictionary.CreateKanjiFromBinary(KanjiDef);
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
        // wait for license screen to be done
        while(licenseScreen.activeInHierarchy || videoPlayer.isPlaying)
        {
            yield return 0;
        }
        SceneManager.LoadScene("MainMenu");
    }
}
