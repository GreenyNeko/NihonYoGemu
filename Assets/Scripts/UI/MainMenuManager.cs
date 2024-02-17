using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    // menus to control
    public GameObject MainMenu;
    public GameObject SettingsMenu;
    public GameObject ExitMenu;
    public GameObject DonationMenu;
    public Image BackgroundImage;
    public float BackgroundTime;
    Sprite backgroundSprite;
    Texture2D backgroundTexture;
    float backgroundTimer;

    // Start is called before the first frame update
    void Start()
    {
        MainMenu.SetActive(true);
        SettingsMenu.SetActive(false);
        ExitMenu.SetActive(false);
        DonationMenu.SetActive(false);
        backgroundTimer = 0;
        // random based on real time instead of program
        Random.InitState(System.DateTime.Now.Second);
    }

    // Update is called once per frame
    void Update()
    {
        // open exit menu when escape is pressed (shortcut)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMenu.SetActive(true);
        }
        // update background
        if (backgroundTimer > 0)
        {
            backgroundTimer -= Time.deltaTime;
        }
        else
        {
            backgroundTimer = BackgroundTime;
            if(BackgroundImage.sprite != null)
            {
                DestroyImmediate(backgroundSprite);
                DestroyImmediate(backgroundTexture);
            }
            backgroundTexture = new Texture2D(1, 1);
            Level.PageData pageData = null;
            if (LevelLoader.GetLevelMetaCount() > 0)
            {
                pageData = LevelLoader.GetLevelMetaByIdx((int)(Random.value * LevelLoader.GetLevelMetaCount())).GetPageData();
            }
            if(pageData != null && pageData.backgrounImageData != null)
            {
                backgroundTexture.LoadImage(pageData.backgrounImageData);
                backgroundSprite = Sprite.Create(backgroundTexture, new Rect(0, 0, backgroundTexture.width, backgroundTexture.height), new Vector2(0.5f, 0.5f));
                BackgroundImage.sprite = backgroundSprite;
                BackgroundImage.color = Color.white;
            }
            else
            {
                BackgroundImage.sprite = null;
                BackgroundImage.color = new Color(0.1921569f, 0.3019608f, 0.4745098f);
            }
        }
    }

    /**
     * Tells unity to switch to the level select scene
     */
    public void StartGame()
    {
        MemoryStream memoryStream = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(memoryStream))
        {
            writer.Write(SceneManager.GetActiveScene().buildIndex);
            writer.Write(false);
        }
        SceneManagerPlus.LoadScene("LevelSelect", memoryStream.ToArray());
    }

    /**
     * Tells unity to switch to the level select for the level editor
     */
    public void StartEditor()
    {
        MemoryStream memoryStream = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(memoryStream))
        {
            writer.Write(SceneManager.GetActiveScene().buildIndex);
            writer.Write(true);
        }
        SceneManagerPlus.LoadScene("LevelSelect", memoryStream.ToArray());
    }

    /**
     * Tells unity to close the game
     */
    public void ExitGame()
    {
        Application.Quit();
    }

    /**
     * Toggles the donation menu
     */
    public void ToggleDonationMenu()
    {
        DonationMenu.SetActive(!DonationMenu.activeInHierarchy);
    }

    /**
     * Opens a URL (used for the donation options)
     */
    public void OpenURL(string URL)
    {
        Application.OpenURL(URL);
    }
}
