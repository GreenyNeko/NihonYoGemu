using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

/**
 * Script that handles input during the game play
 */
public class InputHandler : MonoBehaviour
{
    public bool levelEditor;
    public TMP_Text targetText;
    //public GameManager ScriptGameManager; // the game manager to tell it about the player input
    //public GameObject LevelManager; // the level manager to progress through the level
    public UnityEvent OnEscapeButton;
    public UnityEvent OnEnterButton;


    string currentInput = "";       // the current input of the player
    float internCooldown = 0.5f;    // time to wait until holding a key counts as multiple inputs
    char lastChar = (char)0;        // the last character that was read

    int id = 0;

    void Start()
    {
        id = System.Threading.Thread.CurrentThread.ManagedThreadId;    
    }

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
            else if(Input.GetKeyDown(KeyCode.Escape))
            {
                handleEscape();
            }
        }
    }

    public void SetCurrentInput(string newInput)
    {
        currentInput = newInput;
        targetText.text = newInput;
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
                Debug.Log(currentInput);
                if(currentInput.Length < 13)
                {
                    currentInput += character;
                }
            }
            else
            {
                // TODO: requires a measure of time between presses, spamming should only happen when the key has not been released
                if (internCooldown > 0) return;
                currentInput += character;
            }
            if(id != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                Debug.Log("not on main thread...");
            }
            targetText.text = JapaneseDictionary.ConvertRomajiToKana(currentInput, true);//OnTextChanged.Invoke(JapaneseDictionary.ConvertRomajiToKana(currentInput, true));
        }
    }

    // handles backspace, deletes characters if there are any
    void handleBackspace()
    {
        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            //TextDisplay.GetComponent<TMP_Text>().SetText(JapaneseDictionary.ConvertRomajiToKana(currentInput, true));
            targetText.text = JapaneseDictionary.ConvertRomajiToKana(currentInput, true);//OnTextChanged.Invoke(JapaneseDictionary.ConvertRomajiToKana(currentInput, true));
        }
    }

    // handles enter
    void handleEnter()
    {
        OnEnterButton.Invoke();
    }

    void handleEscape()
    {
        OnEscapeButton.Invoke();
    }
}
