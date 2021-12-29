using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public GameObject GameStarter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // goes to previous scene when escape is pressed
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OpenPreviousScene();
        }
    }

    /**
     * Used to go to the main menu
     */
    public void OpenPreviousScene()
    {
        // remove to prevent memory leak
        Destroy(GameStarter);
        SceneManager.LoadScene("MainMenu");
    }
}
