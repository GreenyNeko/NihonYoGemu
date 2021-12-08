using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/**
 * Script that handles input during the game play
 */
public class InputHandler : MonoBehaviour
{
    public GameManager ScriptGameManager; // the game manager to tell it about the player input
    public GameObject LevelManager; // the level manager to progress through the level
    public GameObject TextDisplay;  // where the player's input is displayed

    string currentInput = "";       // the current input of the player
    float internCooldown = 0.5f;    // time to wait until holding a key counts as multiple inputs
    char lastChar = (char)0;        // the last character that was read


    // Update is called once per frame
    void Update()
    {
        // update internal cooldown if it's greater than 0
        internCooldown = internCooldown <= 0 ? 0 : internCooldown - Time.deltaTime;

        // handle player input
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                handleInput('a');
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                handleInput('b');
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                handleInput('c');
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                handleInput('d');
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                handleInput('e');
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                handleInput('f');
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                handleInput('g');
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                handleInput('h');
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                handleInput('i');
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                handleInput('j');
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                handleInput('k');
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                handleInput('m');
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                handleInput('n');
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                handleInput('o');
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                handleInput('p');
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                handleInput('r');
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                handleInput('s');
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                handleInput('t');
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                handleInput('u');
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                handleInput('w');
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                handleInput('y');
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                handleInput('z');
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                handleBackspace();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                handleEnter();
            }
        }
    }

    // handles any ASCII character input
    void handleInput(char character)
    {
        // if japanese keyboard input
        if(false)
        {
            // TODO: might wanna implement this if the file input method does not work
        }
        else
        {
            // first time press
            if (lastChar != character)
            {
                internCooldown = 0.5f;
                currentInput += character;
            }
            else
            {
                // TODO: requires a measure of time between presses, spamming should only happen when the key has not been released
                if (internCooldown > 0) return;
                currentInput += character;
            }
            // update the displayed text given the input and the input method converting it to hiragana
            TextDisplay.GetComponent<TMP_Text>().SetText(JapaneseDictionary.ConvertRomajiToKana(currentInput));
        }
    }

    // handles backspace, deletes characters if there are any
    void handleBackspace()
    {
        if(currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            TextDisplay.GetComponent<TMP_Text>().SetText(JapaneseDictionary.ConvertRomajiToKana(currentInput));
        }
    }

    // handles enter
    void handleEnter()
    {
        // if the game is running
        if (ScriptGameManager.IsRunning())
        {
            // confirm current Input and check for correctness
            LevelManager.GetComponent<LevelManager>().Progress(currentInput);
            // reset input
            currentInput = "";
            // update display
            TextDisplay.GetComponent<TMP_Text>().SetText(JapaneseDictionary.ConvertRomajiToKana(currentInput));
        }
        else
        {
            // if in end screen leave game
            ScriptGameManager.OnEnd();
        }
    }
}
