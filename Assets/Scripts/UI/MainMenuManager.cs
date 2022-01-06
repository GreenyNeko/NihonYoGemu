using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // menus to control
    public GameObject MainMenu;
    public GameObject SettingsMenu;
    public GameObject ExitMenu;
    public GameObject DonationMenu;

    // Start is called before the first frame update
    void Start()
    {
        MainMenu.SetActive(true);
        SettingsMenu.SetActive(false);
        ExitMenu.SetActive(false);
        DonationMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // open exit menu when escape is pressed (shortcut)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMenu.SetActive(true);
        }
    }

    /**
     * Tells unity to switch to the level select scene
     */
    public void StartGame()
    {
        SceneManager.LoadScene("LevelSelect");
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
