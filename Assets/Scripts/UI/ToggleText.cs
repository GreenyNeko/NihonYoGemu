using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Changes the text of an text object when toggle is called
 */
public class ToggleText : MonoBehaviour
{
    public TMPro.TMP_Text TextObject;   // the text object whose text should be changed
    public string FirstText;            // the first text
    public string SecondText;           // the second text

    bool toggled = true;                // keeps track of the state it is in

    void Start()
    {
        // set first text by default
        TextObject.SetText(FirstText);    
    }

    /**
     * Changes the text of the text object when called. Used for events
     */
    public void OnToggle()
    {
        if(toggled)
        {
            TextObject.SetText(SecondText);
        }
        else
        {
            TextObject.SetText(FirstText);
        }
        toggled = !toggled;
    }
}
