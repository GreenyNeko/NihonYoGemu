using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public GameObject MenuExit;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // open exit menu when escape is pressed (shortcut)
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            MenuExit.SetActive(true);
        }
    }

    /**
     * Used to tell the game to close
     */
    public void EndGame()
    {
        Application.Quit();
    }
}
