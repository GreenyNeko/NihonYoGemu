using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Script controlling the UI highScore elements
 */
public class UIHighScore : MonoBehaviour
{
    // Structure to combine mods with their respective images, used for unity inspector
    [System.Serializable]
    public struct ModImage
    {
        public int mod;
        public GameObject Image;
    }

    public TMPro.TMP_Text TextUserName; // reference to the username text element
    public TMPro.TMP_Text TextScore;    // reference to the score text element
    public TMPro.TMP_Text TextAccuracy; // reference to the accuracy text element
    public GameObject ModsContainer;    // reference to the container holding mod icons

    public List<ModImage> ModImages;    // connects UI image elements with their game mods

    // connects the UI image elements with their game mods internally
    Dictionary<int, GameObject> modImagesDict;

    // Start is called before the first frame update
    void Awake()
    {
        // converts the list of structures to a dictionary
        modImagesDict = new Dictionary<int, GameObject>();
        foreach (ModImage structure in ModImages)
        {
            // disable all mod images by default
            structure.Image.SetActive(false);
            modImagesDict.Add(structure.mod, structure.Image);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValues(string userName, int score, float accuracy, GameMods mods)
    {
        TextUserName.SetText(userName);
        string strScore = score.ToString();
        // adds 0's at the beginning
        while (strScore.Length < 8)
        {
            strScore = "0" + strScore;
        }
        TextScore.SetText(strScore);
        TextAccuracy.SetText(accuracy.ToString() + "%");
        // if it's not initialized run start manually
        if(mods.HasFlag(GameMods.Furigana) && modImagesDict.ContainsKey((int)GameMods.Furigana))
        {
            modImagesDict[(int)GameMods.Furigana].SetActive(true);
        }
    }
}
